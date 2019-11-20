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
            _logger.LogDebug($"Adding Bus Context with exchange {context.ExchangeName}");
            _context = context;
            return this;
        }

        /// <summary>
        /// Scans the assemblies for EventListeners and adds them to the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder UseConventions()
        {
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            _logger.LogDebug($"Using conventions, applying types from assembly: {callingAssembly.GetName()}");
            
            foreach (var type in callingAssembly.DefinedTypes)
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
            return ActivatorUtilities.CreateInstance(serviceProvider, type);
        }

        private IEnumerable<MethodInfo> GetRelevantMethods(TypeInfo type) =>
            type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(e => !e.IsSpecialName);


        /// <summary>
        /// Register a listener for a specific event with topic expressions
        /// </summary>
        private void RegisterEventListener(TypeInfo type, string queueName)
        {
            var methods = GetRelevantMethods(type);

            foreach (var method in methods)
            {
                var parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;
                
                // If method is not suitable, skip it
                if (parameterType == null)
                {
                    _logger.LogWarning($"Method {method.Name} in {type.Name} is not an event callback, skipping");
                    continue;
                }
                
                var topicPatterns = method.GetCustomAttributes<TopicAttribute>()
                    .Select(e => e.TopicPattern)
                    .ToArray();
                
                _eventListeners.Add(new MicroserviceListener
                {
                    TopicExpressions = topicPatterns,
                    Queue = queueName,
                    Callback = message =>
                    {
                        var instance = InstantiatePopulatedType(type);
                        var text = Encoding.Unicode.GetString(message.Body);
                        var jsonObject = JsonConvert.DeserializeObject(text, parameterType);
                        method.Invoke(instance, new[] {jsonObject});
                    }
                });
            }
        }

        /// <summary>
        /// Register a command listener
        /// </summary>
        private void RegisterCommandListener(TypeInfo type, string queueName)
        {
            var methods = GetRelevantMethods(type);
            
            foreach (var method in methods)
            {
                var parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;
                
                // If method is not suitable, skip it
                if (parameterType == null)
                {
                    _logger.LogWarning($"Method {method.Name} in {type.Name} is not an event callback, skipping");
                    continue;
                }
                
                _commandListeners.Add(new MicroserviceCommandListener
                {
                    Queue = queueName,
                    Callback = message =>
                    {
                        var instance = InstantiatePopulatedType(type);

                        var text = Encoding.Unicode.GetString(message.Body);
                        var jsonObject = JsonConvert.DeserializeObject(text, parameterType);
                        var command = method.Invoke(instance, new[] {jsonObject}) as DomainCommand;
                        
                        return new CommandMessage
                        {
                            Timestamp = command.Timestamp,
                            Body = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(command)),
                            CorrelationId = command.Id,
                            EventType = command.GetType().Name
                        };
                    }
                });
            }
        }

        /// <summary>
        /// Register possible event listeners
        /// </summary>
        private void RegisterListener(TypeInfo type)
        {
            string eventQueueName = type.GetCustomAttribute<EventListenerAttribute>()?.QueueName;
            string commandQueueName = type.GetCustomAttribute<CommandListenerAttribute>()?.QueueName;

            if (eventQueueName != null) RegisterEventListener(type, eventQueueName);
            else if (commandQueueName != null) RegisterCommandListener(type, commandQueueName);
            else _logger.LogTrace($"Type {type.Name} does not contain event listener attribute.");
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
            servicesConfiguration.Invoke(_serviceCollection);
            return this;
        }

        /// <summary>
        /// Creates the MicroserviceHost, based on the configurations
        /// </summary>
        /// <returns></returns>
        public MicroserviceHost CreateHost() =>
            new MicroserviceHost(
                _context,
                _eventListeners,
                _commandListeners,
                _loggerFactory.CreateLogger<MicroserviceHost>());
    }
}
