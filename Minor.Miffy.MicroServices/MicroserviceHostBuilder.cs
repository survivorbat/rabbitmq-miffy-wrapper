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
        /// Service provider to enable us to register services
        /// </summary>
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Registered event listeners
        /// </summary>
        private readonly Dictionary<(string, string[]), EventMessageReceivedCallback> _eventListeners = 
            new Dictionary<(string, string[]), EventMessageReceivedCallback>();
        
        /// <summary>
        /// Configures the connection to the message broker
        /// </summary>
        public MicroserviceHostBuilder WithBusContext(IBusContext<IConnection> context)
        {
            _context = context;
            return this;
        }

        /// <summary>
        /// Scans the assemblies for EventListeners and adds them to the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder UseConventions()
        {
            IEnumerable<TypeInfo> types = Assembly.GetCallingAssembly().DefinedTypes;

            foreach (var type in types)
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
            RegisterEventListener(typeof(T).GetTypeInfo());
            return this;
        }

        /// <summary>
        /// Register possible event listeners
        /// </summary>
        private void RegisterEventListener(TypeInfo type)
        {
            string queueName = type.GetCustomAttribute<EventListenerAttribute>()?.QueueName;
            
            if (queueName == null) return;

            var instance = ActivatorUtilities.CreateInstance(_serviceProvider, type);

            foreach (var method in type.DeclaredMethods)
            {
                var topicPatterns = method.GetCustomAttributes<TopicAttribute>()
                    .Select(e => e.TopicPattern)
                    .ToArray();
                
                var parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;
                
                // If method is not suitable, skip it
                if (parameterType == null) continue;

                _eventListeners[(queueName, topicPatterns)] = message =>
                {
                    var text = Encoding.Unicode.GetString(message.Body);
                    var jsonObject = JsonConvert.DeserializeObject(text, parameterType);
                    method.Invoke(instance, new[] {jsonObject});
                };
            }
        }

        /// <summary>
        /// Configures logging functionality for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            return this;
        }

        /// <summary>
        /// Configures Dependency Injection for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder RegisterDependencies(Action<IServiceCollection> servicesConfiguration)
        {
            var collection = new ServiceCollection();
            servicesConfiguration.Invoke(collection);
            _serviceProvider = collection.BuildServiceProvider();
            return this;
        }

        /// <summary>
        /// Creates the MicroserviceHost, based on the configurations
        /// </summary>
        /// <returns></returns>
        public MicroserviceHost CreateHost() => new MicroserviceHost(_context, _eventListeners);
    }
}
