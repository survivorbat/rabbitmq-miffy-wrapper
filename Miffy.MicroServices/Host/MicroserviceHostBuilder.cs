﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Miffy.MicroServices.Commands;
using Miffy.MicroServices.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Miffy.MicroServices.Host
{
    public class MicroserviceHostBuilder
    {
        /// <summary>
        /// The parent type of all domain events
        /// </summary>
        protected static readonly Type DomainEventType = typeof(DomainEvent);

        /// <summary>
        /// The parent type of all domain commands
        /// </summary>
        protected static readonly Type DomainCommandType = typeof(DomainCommand);

        /// <summary>
        /// Type of a string
        /// </summary>
        protected static readonly Type StringType = typeof(string);

        /// <summary>
        /// Bus context that houses configuration for the message bus
        /// </summary>
        protected IBusContext<IConnection> Context { get; set; }

        /// <summary>
        /// Loggerfactory to create logging instances
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();

        /// <summary>
        /// Logger to log everything in here
        /// </summary>
        protected ILogger<MicroserviceHostBuilder> Logger { get; set; }

        /// <summary>
        /// Queue that is used to listen
        /// </summary>
        public string QueueName { get; protected set; } = "unset_queue_name_" + Guid.NewGuid();

        /// <summary>
        /// Service collection to collect all services in
        /// </summary>
        public IServiceCollection ServiceCollection { get; } = new ServiceCollection();

        /// <summary>
        /// Registered event listeners
        /// </summary>
        protected List<MicroserviceListener> EventListeners { get; } = new List<MicroserviceListener>();

        /// <summary>
        /// Registered command listeners
        /// </summary>
        protected List<MicroserviceCommandListener> CommandListeners { get; } = new List<MicroserviceCommandListener>();

        /// <summary>
        /// Initialize a new builder with a null logger factory
        /// </summary>
        public MicroserviceHostBuilder()
        {
            ServiceCollection.AddSingleton(LoggerFactory);
            Logger = LoggerFactory.CreateLogger<MicroserviceHostBuilder>();
        }

        /// <summary>
        /// Configures the connection to the message broker
        /// </summary>
        public virtual MicroserviceHostBuilder WithBusContext(IBusContext<IConnection> context)
        {
            Logger.LogDebug("Adding Bus Context");
            Context = context;
            ServiceCollection.AddSingleton(context);
            return this;
        }

        /// <summary>
        /// Set the name of the queue that this service will utilise
        ///
        /// If this value is not set, a GUID will be generated with a queue_ prefix
        /// </summary>
        public virtual MicroserviceHostBuilder WithQueueName(string queueName)
        {
            Logger.LogDebug($"Setting queuename as {queueName}");
            QueueName = queueName;
            return this;
        }

        /// <summary>
        /// Scans the assemblies for EventListeners and adds them to the MicroserviceHost
        /// </summary>
        public virtual MicroserviceHostBuilder UseConventions()
        {
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            Logger.LogDebug($"Using conventions, applying types from assembly: {callingAssembly.GetName()}");

            foreach (TypeInfo type in callingAssembly.DefinedTypes)
            {
                RegisterListener(type);
            }

            return this;
        }

        /// <summary>
        /// Manually adds EventListeners or CommandListeners to the MicroserviceHost
        /// </summary>
        public virtual MicroserviceHostBuilder AddEventListener<T>()
        {
            TypeInfo type = typeof(T).GetTypeInfo();

            Logger.LogDebug($"Adding event listeners for type {type.FullName}");

            RegisterListener(type);
            return this;
        }

        /// <summary>
        /// Instantiate an instance of a type with populated services
        /// </summary>
        protected virtual object InstantiatePopulatedType(TypeInfo type)
        {
            Logger.LogTrace("Building service provider...");
            ServiceProvider serviceProvider;

            try
            {
                serviceProvider = ServiceCollection.BuildServiceProvider();
            }
            catch (Exception exception)
            {
                Logger.LogCritical($"Error occured while building service provider with type {type.Name}, error: {exception.Message}");
                throw;
            }

            try
            {
                Logger.LogTrace($"Instantiating {type.Name} with provided services.");
                object instance = ActivatorUtilities.CreateInstance(serviceProvider, type);

                Logger.LogTrace($"Instantiated {type.Name} with provided services, return instance.");
                return instance;
            }
            catch (InvalidOperationException e)
            {
                Logger.LogCritical($"Type {type.Name} could not be properly instantiated " +
                                    "with provided services. Did you register all your dependencies? " +
                                    $"Listener will NOT be called! Exception: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieve all the publicly declared instance methods from a type
        /// </summary>
        protected virtual IEnumerable<MethodInfo> GetRelevantMethods(TypeInfo type)
        {
            Logger.LogDebug($"Retrieving methods from type {type.Name}");
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(e => !e.IsSpecialName);
        }


        /// <summary>
        /// Register a listener for a specific event with topic expressions
        /// </summary>
        protected virtual void RegisterEventListener(TypeInfo type, MethodInfo method)
        {
            Logger.LogDebug($"Checking if method {method.Name} is a suitable event listener");

            if (!IsMethodSuitableEvent(method, false))
            {
                Logger.LogCritical($"Method {method.Name} does not have a proper event listener signature in type {type.Name}");
                throw new BusConfigurationException($"Method {method.Name} does not have a proper eventlistener signature in type {type.Name}");
            }

            Logger.LogDebug($"Evaluating parameter type {type.Name} of method {method.Name}");
            Type parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;

            TopicAttribute[] topicAttributes = method.GetCustomAttributes<TopicAttribute>()
                .ToArray();

            string[] topicPatterns = topicAttributes
                .Select(e => e.TopicPattern)
                .ToArray();

            Regex[] topicRegularExpressions = topicAttributes
                .Select(e => e.TopicRegularExpression)
                .ToArray();

            Logger.LogDebug($"Found topic patterns {string.Join(", ", topicPatterns)} on method {method.Name} in type {type.Name}");

            Logger.LogTrace($"Adding MicroserviceListener with type {type.Name} and method {method.Name}");
            EventListeners.Add(new MicroserviceListener
            {
                TopicExpressions = topicPatterns,
                TopicRegularExpressions = topicRegularExpressions,
                Callback = message =>
                {
                    Logger.LogDebug($"Attempting to instantiate type {type.Name}");
                    object instance = InstantiatePopulatedType(type);

                    Logger.LogTrace($"Retrieving string data from message with id {message.CorrelationId}");
                    string text = Encoding.Unicode.GetString(message.Body);

                    Logger.LogTrace("Checking if parameter type is equal to a string");
                    if (parameterType == StringType)
                    {
                        Logger.LogTrace($"Parameter type is a string, invoking method for message {message.CorrelationId} with body {text}");
                        method.Invoke(instance, new object[] {text});
                        return;
                    }

                    try
                    {
                        Logger.LogTrace($"Deserialized object from message with id {message.CorrelationId} and body {text}");
                        object jsonObject = JsonConvert.DeserializeObject(text, parameterType);

                        Logger.LogTrace($"Invoking method {method.Name} with message id {message.CorrelationId} and instance " +
                                        $"of supposed type {type.Name} and actual type {jsonObject?.GetType().Name} with data {text}");

                        method.Invoke(instance, new[] {jsonObject});
                    }
                    catch (JsonReaderException readerException)
                    {
                        Logger.LogCritical($"JsonReaderException occured while deserializing message with type {message.EventType} and topic {message.Topic}," +
                                           $" consider changing the parameter type of method {method.Name} of type {type.Name} to string. Data: {text}, exception: {readerException.Message}");
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Register a command listener
        /// </summary>
        protected virtual void RegisterCommandListener(TypeInfo type, MethodInfo method, string queueName)
        {
            Logger.LogDebug($"Checking if method {method.Name} is a suitable command listener");
            if (!IsMethodSuitableEvent(method, true))
            {
                throw new BusConfigurationException($"Method {method.Name} does not have a proper commandlistener signature in type {type.Name}");
            }

            Type parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;
            Type returnType = method.ReturnType;

            Logger.LogDebug($"Adding MicroserviceCommandListener with queue {queueName}, type {type.Name} and method {method.Name}");
            CommandListeners.Add(new MicroserviceCommandListener
            {
                Queue = queueName,
                Callback = message =>
                {
                    Logger.LogTrace($"Attempting to instantiate type {type.Name} in queue {queueName}");
                    object instance = InstantiatePopulatedType(type);

                    Logger.LogTrace($"Retrieving string command data from message with id {message.CorrelationId}");
                    string text = Encoding.Unicode.GetString(message.Body);

                    Logger.LogTrace($"Deserialized command object from message with id {message.CorrelationId} and body {text}");
                    object jsonObject = JsonConvert.DeserializeObject(text, parameterType);

                    Logger.LogTrace($"Invoking method {method.Name} with command message id {message.CorrelationId} and instance of type {type.Name} with data {text}");
                    object returnCommand = method.Invoke(instance, new[] {jsonObject});

                    Logger.LogTrace("Serializing result command");
                    string jsonReturn = JsonConvert.SerializeObject(returnCommand);

                    Logger.LogTrace($"Returning new CommandMessage with timestamp {message?.Timestamp}, id {message.CorrelationId}, EventType {returnType.Name} and body {jsonReturn}");
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
        protected virtual void RegisterListener(TypeInfo type)
        {
            Logger.LogTrace($"Retrieving relevant methods from type {type.Name}");

            foreach (MethodInfo methodInfo in GetRelevantMethods(type))
            {
                EventListenerAttribute eventQueueName = methodInfo.GetCustomAttribute<EventListenerAttribute>();
                string commandQueueName = methodInfo.GetCustomAttribute<CommandListenerAttribute>()?.QueueName;

                if (eventQueueName != null)
                {
                    RegisterEventListener(type, methodInfo);
                }
                else if (commandQueueName != null)
                {
                    RegisterCommandListener(type, methodInfo, commandQueueName);
                }
                else
                {
                    Logger.LogTrace($"Method {methodInfo.Name} does not contain listener attributes.");
                }
            }
        }

        /// <summary>
        /// Check if method has a suitable signature to be used as a command or event listener
        /// </summary>
        protected virtual bool IsMethodSuitableEvent(MethodInfo method, bool isCommandListener)
        {
            Logger.LogTrace($"Evaluating whether {method.Name} has 1 parameter");
            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length != 1)
            {
                return false;
            }

            ParameterInfo parameter = parameters.First();
            Type parameterType = parameter.ParameterType;

            Logger.LogTrace($"Evaluating whether method {method.Name}'s parameter {parameter.Name} is " +
                             "a string or is derived from DomainEvent or CommandEvent");

            if ((!isCommandListener && !parameterType.IsSubclassOf(DomainEventType)
                 || isCommandListener && !parameterType.IsSubclassOf(DomainCommandType))
                && parameterType != StringType)
            {
                Logger.LogCritical($"Parameter {parameter.Name} of method {method.Name} has " +
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
        public virtual MicroserviceHostBuilder SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
            ServiceCollection.AddSingleton(loggerFactory);
            Logger = loggerFactory.CreateLogger<MicroserviceHostBuilder>();
            return this;
        }

        /// <summary>
        /// Configures Dependency Injection for the MicroserviceHost
        /// </summary>
        public virtual MicroserviceHostBuilder RegisterDependencies(Action<IServiceCollection> servicesConfiguration)
        {
            Logger.LogDebug("Registering dependencies");
            servicesConfiguration.Invoke(ServiceCollection);
            return this;
        }

        /// <summary>
        /// Adds dependencies of existing servicecollection to the dependencies list
        /// </summary>
        public virtual MicroserviceHostBuilder RegisterDependencies(IServiceCollection serviceCollection)
        {
            Logger.LogDebug("Adding registered dependencies from provided collection");
            foreach (ServiceDescriptor serviceDescriptor in serviceCollection)
            {
                ServiceCollection.Add(serviceDescriptor);
            }
            return this;
        }

        /// <summary>
        /// Creates the MicroserviceHost, based on the configurations
        /// </summary>
        /// <returns></returns>
        public virtual IMicroserviceHost CreateHost()
        {
            Logger.LogDebug($"Instantiating microservicehost with {EventListeners.Count} event listeners and {CommandListeners.Count} command listeners");
            return new MicroserviceHost(
                Context,
                EventListeners,
                CommandListeners,
                QueueName,
                LoggerFactory.CreateLogger<MicroserviceHost>());
        }
    }
}
