using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Host
{
    public class MicroserviceHostBuilder : IDisposable
    {
        /// <summary>
        /// The parent type of all domain events
        /// </summary>
        private static readonly Type DomainEventType = typeof(DomainEvent);

        /// <summary>
        /// The parent type of all domain commands
        /// </summary>
        private static readonly Type DomainCommandType = typeof(DomainCommand);

        /// <summary>
        /// Type of a string
        /// </summary>
        private static readonly Type StringType = typeof(string);

        /// <summary>
        /// Bus context that houses configuration for the message bus
        /// </summary>
        private IBusContext<IConnection> _context;

        /// <summary>
        /// Loggerfactory to create logging instances
        /// </summary>
        private ILoggerFactory _loggerFactory;

        /// <summary>
        /// Logger to log everything in here
        /// </summary>
        private ILogger<MicroserviceHostBuilder> _logger;

        /// <summary>
        /// Service collection to collect all services in
        /// </summary>
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();

        /// <summary>
        /// Registered event listeners
        /// </summary>
        public readonly List<MicroserviceListener> _eventListeners = new List<MicroserviceListener>();

        /// <summary>
        /// Registered command listeners
        /// </summary>
        public readonly List<MicroserviceCommandListener> _commandListeners = new List<MicroserviceCommandListener>();

        /// <summary>
        /// Initialize a new builder with a null logger factory
        /// </summary>
        public MicroserviceHostBuilder()
        {
            _loggerFactory = new NullLoggerFactory();
            _serviceCollection.AddSingleton(_loggerFactory);
            _logger = _loggerFactory.CreateLogger<MicroserviceHostBuilder>();
        }

        /// <summary>
        /// Configures the connection to the message broker
        /// </summary>
        public MicroserviceHostBuilder WithBusContext(IBusContext<IConnection> context)
        {
            _logger.LogDebug("Adding Bus Context");
            _context = context;
            _serviceCollection.AddSingleton(context);
            return this;
        }

        /// <summary>
        /// Scans the assemblies for EventListeners and adds them to the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder UseConventions()
        {
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            _logger.LogDebug($"Using conventions, applying types from assembly: {callingAssembly.GetName()}");

            foreach (TypeInfo type in callingAssembly.DefinedTypes)
            {
                RegisterListener(type);
            }

            return this;
        }

        /// <summary>
        /// Manually adds EventListeners or CommandListeners to the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder AddEventListener<T>()
        {
            TypeInfo type = typeof(T).GetTypeInfo();

            _logger.LogDebug($"Adding event listeners for type {type.FullName}");

            RegisterListener(type);
            return this;
        }

        /// <summary>
        /// Instantiate an instance of a type with populated services
        /// </summary>
        private object InstantiatePopulatedType(TypeInfo type)
        {
            ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();
            _logger.LogTrace($"Instantiating {type.Name} with provided services.");

            try
            {
                object instance = ActivatorUtilities.CreateInstance(serviceProvider, type);
                return instance;
            }
            catch (InvalidOperationException e)
            {
                _logger.LogCritical($"Type {type.Name} could not be properly instantiated " +
                                    "with provided services. Did you register all your dependencies? " +
                                    $"Listener will NOT be called! Exception: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieve all the publicly declared instance methods from a type
        /// </summary>
        private IEnumerable<MethodInfo> GetRelevantMethods(TypeInfo type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(e => !e.IsSpecialName);
        }


        /// <summary>
        /// Register a listener for a specific event with topic expressions
        /// </summary>
        private void RegisterEventListener(TypeInfo type, MethodInfo method, string queueName)
        {
            if (!IsMethodSuitableEvent(method, false))
            {
                throw new BusConfigurationException($"Method {method.Name} does not have a proper commandlistener signature in type {type.Name}");
            }

            _logger.LogDebug($"Evaluating parameter type {type.Name} of method {method.Name}");
            Type parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;

            string[] topicPatterns = method.GetCustomAttributes<TopicAttribute>()
                .Select(e => e.TopicPattern)
                .ToArray();

            _logger.LogDebug($"Found topic patterns {string.Join(", ", topicPatterns)} on method {method.Name} in type {type.Name}");

            _logger.LogTrace($"Adding MicroserviceListener with queue {queueName}, type {type.Name} and method {method.Name}");
            _eventListeners.Add(new MicroserviceListener
            {
                TopicExpressions = topicPatterns,
                Queue = queueName,
                Callback = message =>
                {
                    _logger.LogDebug($"Attempting to instantiate type {type.Name} in queue {queueName}");
                    object instance = InstantiatePopulatedType(type);

                    _logger.LogTrace($"Retrieving string data from message with id {message.CorrelationId}");
                    string text = Encoding.Unicode.GetString(message.Body);

                    if (parameterType == StringType)
                    {
                        _logger.LogTrace($"Parameter type is a string, invoking method for message {message.CorrelationId} with body {text}");
                        method.Invoke(instance, new object[] {text});
                        return;
                    }

                    _logger.LogTrace($"Deserialized object from message with id {message.CorrelationId} and body {text}");
                    object jsonObject = JsonConvert.DeserializeObject(text, parameterType);

                    _logger.LogTrace($"Invoking method {method.Name} with message id {message.CorrelationId} and instance of type {type.Name} with data {text}");
                    method.Invoke(instance, new[] {jsonObject});
                }
            });
        }

        /// <summary>
        /// Register a command listener
        /// </summary>
        private void RegisterCommandListener(TypeInfo type, MethodInfo method, string queueName)
        {
            if (!IsMethodSuitableEvent(method, true))
            {
                throw new BusConfigurationException($"Method {method.Name} does not have a proper commandlistener signature in type {type.Name}");
            }

            Type parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;
            Type returnType = method.ReturnType;

            _logger.LogDebug($"Adding MicroserviceCommandListener with queue {queueName}, type {type.Name} and method {method.Name}");
            _commandListeners.Add(new MicroserviceCommandListener
            {
                Queue = queueName,
                Callback = message =>
                {
                    _logger.LogTrace($"Attempting to instantiate type {type.Name} in queue {queueName}");
                    object instance = InstantiatePopulatedType(type);

                    _logger.LogTrace($"Retrieving string command data from message with id {message.CorrelationId}");
                    string text = Encoding.Unicode.GetString(message.Body);

                    _logger.LogTrace($"Deserialized command object from message with id {message.CorrelationId} and body {text}");
                    object jsonObject = JsonConvert.DeserializeObject(text, parameterType);

                    _logger.LogTrace($"Invoking method {method.Name} with command message id {message.CorrelationId} and instance of type {type.Name} with data {text}");
                    object returnCommand = method.Invoke(instance, new[] {jsonObject});

                    _logger.LogTrace("Serializing result command");
                    string jsonReturn = JsonConvert.SerializeObject(returnCommand);

                    _logger.LogTrace($"Returning new CommandMessage with timestamp {message?.Timestamp}, id {message.CorrelationId}, EventType {returnType.Name} and body {jsonReturn}");
                    return new CommandMessage
                    {
                        Timestamp = message.Timestamp,
                        Body = Encoding.Unicode.GetBytes(jsonReturn),
                        CorrelationId = message.CorrelationId,
                        EventType = returnType.Name
                    };
                }
            });
        }

        /// <summary>
        /// Register possible event listeners
        /// </summary>
        private void RegisterListener(TypeInfo type)
        {
            _logger.LogTrace($"Retrieving relevant methods from type {type.Name}");

            foreach (MethodInfo methodInfo in GetRelevantMethods(type))
            {
                string eventQueueName = methodInfo.GetCustomAttribute<EventListenerAttribute>()?.QueueName;
                string commandQueueName = methodInfo.GetCustomAttribute<CommandListenerAttribute>()?.QueueName;

                if (eventQueueName != null)
                {
                    RegisterEventListener(type, methodInfo, eventQueueName);
                }
                else if (commandQueueName != null)
                {
                    RegisterCommandListener(type, methodInfo, commandQueueName);
                }
                else
                {
                    _logger.LogTrace($"Method {methodInfo.Name} does not contain listener attributes.");
                }
            }
        }

        /// <summary>
        /// Check if method has a suitable signature to be used as a command or event listener
        /// </summary>
        private bool IsMethodSuitableEvent(MethodInfo method, bool isCommandListener)
        {
            _logger.LogTrace($"Evaluating whether {method.Name} has 1 parameter");
            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length != 1)
            {
                return false;
            }

            ParameterInfo parameter = parameters.First();
            Type parameterType = parameter.ParameterType;

            _logger.LogTrace($"Evaluating whether method {method.Name}'s parameter {parameter.Name} is " +
                             "a string or is derived from DomainEvent or CommandEvent");

            if ((!isCommandListener && parameterType.IsAssignableFrom(DomainEventType)
                 || isCommandListener && parameterType.IsAssignableFrom(DomainCommandType))
                && parameterType != StringType)
            {
                _logger.LogCritical($"Parameter {parameter.Name} of method {method.Name} has " +
                                    $"type {parameterType.Name} which is not a proper type for a " +
                                    $"{(isCommandListener ? "Command" : "Event")}Listener");

                return false;
            }

            if (!isCommandListener)
            {
                return method.ReturnType == typeof(void);
            }

            return true;
        }

        /// <summary>
        /// Configures logging functionality for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _serviceCollection.AddSingleton(loggerFactory);
            _logger = loggerFactory.CreateLogger<MicroserviceHostBuilder>();
            return this;
        }

        /// <summary>
        /// Configures Dependency Injection for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder RegisterDependencies(Action<IServiceCollection> servicesConfiguration)
        {
            _logger.LogDebug("Registering dependencies");
            servicesConfiguration.Invoke(_serviceCollection);
            return this;
        }

        /// <summary>
        /// Creates the MicroserviceHost, based on the configurations
        /// </summary>
        /// <returns></returns>
        public MicroserviceHost CreateHost()
        {
            _logger.LogDebug($"Instantiating microservicehost with {_eventListeners.Count} event listeners and {_commandListeners.Count} command listeners");
            return new MicroserviceHost(
                _context,
                _eventListeners,
                _commandListeners,
                _loggerFactory.CreateLogger<MicroserviceHost>());
        }

        /// <summary>
        /// Dispose of the logger factoryu
        /// </summary>
        public void Dispose()
        {
            _loggerFactory?.Dispose();
        }
    }
}
