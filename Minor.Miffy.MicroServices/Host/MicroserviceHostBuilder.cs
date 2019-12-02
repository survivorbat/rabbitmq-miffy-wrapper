using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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
    public class MicroserviceHostBuilder 
    {
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
        private readonly List<MicroserviceListener> _eventListeners = new List<MicroserviceListener>();
        
        /// <summary>
        /// Registered command listeners
        /// </summary>
        private readonly List<MicroserviceCommandListener> _commandListeners = new List<MicroserviceCommandListener>();
        
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
        /// <param name="type">Type to analyze</param>
        /// <returns>A list of methods</returns>
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
            _logger.LogDebug($"Evaluating parameter type {type.Name} of method {method.Name}");
            Type parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;
            
            _logger.LogDebug($"Found parameter type {parameterType?.Name} on method {method.Name} of type {type.Name}");
            
            // If method is not suitable, skip it
            if (parameterType == null)
            {
                _logger.LogWarning($"Method {method.Name} in {type.Name} is not an event callback, skipping");
                return;
            }
            
            _logger.LogTrace($"Evaluating parameter type {type.Name} of method {method.Name}");
            string[] topicPatterns = method.GetCustomAttributes<TopicAttribute>()
                .Select(e => e.TopicPattern)
                .ToArray();
            
            _logger.LogDebug($"Found topic patterns {string.Join(", ", topicPatterns)} on method {method.Name} in type {type.Name}");

            _logger.LogDebug($"Adding MicroserviceListener with queue {queueName}, type {type.Name} and method {method.Name}");
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
                    
                    _logger.LogTrace($"Deserialized object from message with id {message.CorrelationId} and body {text}");
                    object jsonObject = JsonConvert.DeserializeObject(text, parameterType);
                    
                    if (jsonObject == null)
                    {
                        _logger.LogCritical($"Deserializing {text} to type {parameterType.Name} resulted in a null object in eventlistener");
                        throw new BusConfigurationException($"Deserializing {text} to type {parameterType.Name} resulted in a null object in eventlistener");
                    }
                    
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
            _logger.LogDebug($"Evaluating parameter type {type.Name} of command method {method.Name}");
            Type parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;
            
            _logger.LogDebug($"Found parameter type {parameterType?.Name} on command method {method.Name} of type {type.Name}");

            // If method is not suitable, skip it
            if (parameterType == null)
            {
                _logger.LogWarning($"Method {method.Name} in {type.Name} is not a command callback, skipping");
                return;
            }
            
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

                    if (jsonObject == null)
                    {
                        _logger.LogCritical($"Deserializing {text} to command type {parameterType.Name} resulted in a null object in commandlistener");
                        throw new BusConfigurationException($"Deserializing {text} to command type {parameterType.Name} resulted in a null object in commandlistener");
                    }

                    _logger.LogTrace($"Invoking method {method.Name} with command message id {message.CorrelationId} and instance of type {type.Name} with data {text}");
                    DomainCommand command = method.Invoke(instance, new[] {jsonObject}) as DomainCommand;

                    _logger.LogTrace("Serializing result command");
                    string jsonReturn = JsonConvert.SerializeObject(command);
                    
                    _logger.LogTrace($"Returning new CommandMessage with timestamp {command?.Timestamp}, id {command?.Id}, EventType {command?.GetType().Name}, body {jsonReturn}");
                    return new CommandMessage
                    {
                        Timestamp = message.Timestamp,
                        Body = Encoding.Unicode.GetBytes(jsonReturn),
                        CorrelationId = message.CorrelationId,
                        EventType = message.EventType
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
        /// Configures logging functionality for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _serviceCollection.AddSingleton(loggerFactory);
            _logger = loggerFactory.CreateLogger<MicroserviceHostBuilder>();
            _logger.LogInformation("Successfully set logger factory :-)");
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
            _logger.LogDebug($"Instantiating microservicehost with {_eventListeners.Count} eventlisteners and {_commandListeners.Count} commandlisteners");
            return new MicroserviceHost(
                _context,
                _eventListeners,
                _commandListeners,
                _loggerFactory.CreateLogger<MicroserviceHost>());
        }
    }
}
