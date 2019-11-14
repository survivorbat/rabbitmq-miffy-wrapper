using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Minor.Miffy.MicroServices
{
    /// <summary>
    /// Creates and Configures a MicroserviceHost
    /// For example:
    ///     var builder = new MicroserviceHostBuilder()
    ///             .SetLoggerFactory(...)
    ///             .RegisterDependencies((services) =>
    ///                 {
    ///                     services.AddTransient<IFoo,Foo>();
    ///                 })
    ///             .WithBusContext(context)
    ///             .UseConventions();
    /// </summary>
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
        /// Configures the connection to the message broker
        /// </summary>
        public MicroserviceHostBuilder WithBusContext(IBusContext<IConnection> context)
        {
            _logger.LogDebug("Adding Bus Context", context);
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
                RegisterEventListener(type);
            }
            
            return this;
        }

        /// <summary>
        /// Manually adds EventListeners to the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder AddEventListener<T>()
        {
            TypeInfo type = typeof(T).GetTypeInfo();
            
            _logger.LogDebug($"Adding event listeners for type {type.FullName}");
            
            RegisterEventListener(type);
            return this;
        }

        /// <summary>
        /// Register possible event listeners
        /// </summary>
        private void RegisterEventListener(TypeInfo type)
        {
            ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();
            
            _logger.LogTrace($"Analysing type {type.Name}");
            string queueName = type.GetCustomAttribute<EventListenerAttribute>()?.QueueName;

            if (queueName == null)
            {
                _logger.LogTrace($"Type {type.Name} does not contain event listener attribute.");
                return;
            }

            _logger.LogTrace($"Instantiating {type.Name} with provided services.");
            var instance = ActivatorUtilities.CreateInstance(serviceProvider, type);

            foreach (var method in type.DeclaredMethods)
            {
                _logger.LogTrace($"Retrieving topic attributes from {type.Name}");
                var topicPatterns = method.GetCustomAttributes<TopicAttribute>()
                    .Select(e => e.TopicPattern)
                    .ToArray();
                
                _logger.LogTrace($"Retrieving parameter type of {method.Name} in {type.Name}");
                var parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;
                
                // If method is not suitable, skip it
                if (parameterType == null)
                {
                    _logger.LogWarning($"Method {method.Name} in {type.Name} does not have a parameter but is marked as event listener!");
                    continue;
                }

                _logger.LogInformation($"Adding method {method.Name} in {type.Name} to event listener collection.");
                _eventListeners.Add(new MicroserviceListener
                {
                    TopicExpressions = topicPatterns,
                    Queue = queueName,
                    Callback = message =>
                    {
                        var text = Encoding.Unicode.GetString(message.Body);
                        var jsonObject = JsonConvert.DeserializeObject(text, parameterType);
                        method.Invoke(instance, new[] {jsonObject});
                    }
                });
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
        public MicroserviceHost CreateHost() => new MicroserviceHost(_context, _eventListeners, _loggerFactory);
    }
}
