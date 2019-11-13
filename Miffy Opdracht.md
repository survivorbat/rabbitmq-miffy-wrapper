# RabbitMQ Framework

In deze 'oefening' bouwen we een RabbitMQ-frameworkje.

Mijn suggestie op te werken in viertallen en dan te doen aan pairprogramming en het werk te verdelen. 

# Stap 1 - Eenvoudige wrapper rond RabbitMQ
Bouw een kleine wrapper rond RabbitMQ.
 - Minor.Miffy
 - Minor.Miffy.RabbitMQBus

Maak aparte Test-projecten om deze functionaliteit te testen
Maak ook een apart IntegratieTest-project om de functionaliteit te testen tegen een draaiende RabbitMQ instantie.

## Definieer EventMessage
Maak een abstractie rond de berichten:

    public class EventMessage
    {
        public string Topic { get; set; }
        public Guid CorrelationId { get; set; }
        public int Timestamp { get; set; }
        public string EventType { get; set; }
        public byte[] Body { get; set; }
    }

Breidt deze class, indien nodig, uit met nuttige functionaliteit, zoals bijvoorbeeld een override van ToString().

## Definieer BusContext
Schrijf een class 'RabbitMQBusContext'. Deze moet het onderstaande IBusContext-interface implementeren.

    public interface IBusContext<TConnection> : IDisposable
    {
        TConnection Connection { get; }
        string ExchangeName { get; }

        IMessageSender CreateMessageSender();
        IMessageReceiver CreateMessageReceiver();
    }

Implementeer onderste twee de methodes pas als je ook de bijbehorene RabbitMQMessageSender/RabbitMQMessageReceiver gaat schrijven.

De RabbitMQBusContext kan worden aangemaakt door een RabbitMQContextBuilder class. Hier kan de Exchange name worden bepaald, alsmede de configuratie van RabbitMQ (Host, port, Username, Password).

    public class RabbitMQContextBuilder
    {
        public RabbitMQContextBuilder WithExchange(string exchangeName)
        public RabbitMQContextBuilder WithAddress(string hostName, int port)
        public RabbitMQContextBuilder WithCredentials(string userName, string password)
        public RabbitMQContextBuilder ReadFromEnvironmentVariables()
        public RabbitMQBusContext CreateContext()
    }

Bij het aanmaken van een BusContext opent de RabbitMQContextBuilder alvast een Connection Met RabbitMQ, die hij meegeeft aan de constructor van de RabbitMQBusContext. Ook zorgt de RabbitMQContextBuilder dat de Exchange al is gedeclareerd.

    public class RabbitMQBusContext : IBusContext<IConnection>
    {
        public RabbitMQBusContext(IConnection connection, string exchangeName)
    }


## Definieer EventMessage Sender
Schrijf een class 'RabbitMQMessageSender' om EventMessages te versturen. Deze moet het onderstaande IMessageSender-interface implementeren.

    public interface IMessageSender : IDisposable
    {
        void SendMessage(EventMessage message);
    }

De RabbitMQMessageSender krijgt een RabbitMQBusContext geinjecteerd. Deze bevat onder andere de (al geopende) connectie met RabbitMQ en de naam van de Exchange waarover de MessageSender zijn event messages zal versturen.

    public class RabbitMQMessageSender : IMessageSender
    {
        public RabbitMQMessageSender(RabbitMQBusContext context)
    }

## Definieer EventMessage Reveiver
Schrijf een class 'RabbitMQMessageReceiver' om EventMessages te ontvangen. Deze moet het onderstaande IMessageReceiver-interface implementeren.

    public interface IMessageReceiver : IDisposable
    {
        string QueueName { get; }
        IEnumerable<string> TopicExpressions { get; }

        void DeclareQueueWithTopics();
         void StartReceivingMessages(EventMessageReceivedCallback Callback);
    }

    public delegate void EventMessageReceivedCallback(EventMessage eventMessage);

De RabbitMQMessageReceiver krijgt een RabbitMQBusContext geinjecteerd. Deze bevat onder andere de (al geopende) connectie met RabbitMQ en de naam van de Exchange waar de MessageReceiver zijn queue aan zal koppelen.

De DeclareQueueWithTopics-method zal een squeue aanmaken met de naam QueueName (indien deze nog niet bestaat). Vanaf dat moment zullen alle relevante messages in de queue bewaard blijven totdat de Receiver klaar is om ze te verwerken.
De DeclareQueueWithTopics-method moet maar eenmalig worden aangeroepen. Een tweede aanroep moet een BusConfigurationException opleveren.

De StartReceivingMessages-method geeft aan det de Reciever klaar is om messages te verwerken. De event messages worden een voor een uit de queue gehaald, en voor elke event message wordt de callback-methode uitgevoerd.
De StartReceivingMessages-method moet maar eenmalig worden aangeroepen. Een tweede aanroep moet een BusConfigurationException opleveren.

    public class RabbitMQMessageReceiver : IMessageReceiver
    {
        public RabbitMQMessageReceiver(RabbitMQBusContext context, 
                        string queueName, IEnumerable<string> topicExpressions)
    }

.

# Stap 3 - Event Bus Framework

 Bouw een Framework om events te versturen en naar events te luisteren. Bouw deze abstractielaag bovenop de MessageSender en -Receiver. 

Een EventListener zou er dan mogelijk zo uit zien:

    [EventListener("MVM.TestService.PolisEventListenerQueue")]
    public class PolisEventListener
    {
        private readonly IDbContextOptions<PolisContext> _context;

        public PolisEventListener(IDbContextOptions<PolisContext> context)
        {
            _context = context;
        }

        [Topic("MVM.Polisbeheer.PolisToegevoegd")]
        public void Handles(PolisToegevoegdEvent evt)
        {
            _context.Polissen.Add(evt.Polis);
            _context.SaveChanges();
        }
    }

# Stap 3 - Testbus om RabbitMQ te vervangen

Schrijf een (zo simpel mogelijke) in-memory versie van RabbitMQ. Deze moet de volgende interfaces ondersteunen.

    public interface IBusContext : IDisposable

    public interface IMessageSender : IDisposable
   
    public interface IMessageReceiver : IDisposable
 
 Applicatie(onderdelen) die dezelfde TestBusContext gebruiken zullen hierdoor via een stelsel van in-memory queues met elkaar kunnen communiceren.

 De Testbus hoeft maar 1 Exchange te hebben. Die hoef je dus **niet** te modeleren. De Testbus hoeft ook **geen** Host, Port, Username en Password te hebben.

 De TestBusContext moet extra functionaliteit hebben om te kunnen zien welke queues er zijn gedeclareerd en welke messages daar in staan. Bovendien moet het mogelijk zijn om Messages aan een queue toe te kunnen voegen.
 
 Met behulp van deze TestBus kunnnen voortaan alle applicaties getest worden zonder dat ze gebruik hoeven te maken van RabbitMQ.

.

# Stap 4 - Commands toevoegen aan Simpele RabbitMQ Wrapper
Voeg Commands toe aan de simpele RabbitMQ wrapper.
Voer hiervoor twee interface toe:

    public interface ICommandSender : IDisposable
    {
        Task<CommandMessage> SendCommandAsync(CommandMessage request);
    }

    public interface ICommandReceiver : IDisposable
    {
        string QueueName { get; }

        void DeclareCommandQueue();
        void StartReceivingCommands(CommandReceivedCallback callback);
    }

    public delegate CommandMessage CommandReceivedCallback(CommandMessage commandMessage);

Breidt de IBusContext ook uit met twee Create-methodes:

    public interface IBusContext : IDisposable
    {
        // ..

        ICommandSender CreateCommandSender();
        ICommandReceiver CreateCommandReceiver();
    }

Implementeer vervolgens de ICommandSender en ICommandReceiver interfaces voor RabbitMQ. De Commands wordn binnen RabbitMQ niet over de exchange gestuurd, maar rechtstreeks naar de queue van de ontvangende partij. De Sender moet daartoe de naam van de Reply-queue met het Command meesturen.

    public class RabbitMQCommandSender : ICommandSender
    public class RabbitMQCommandReceiver : ICommandReceiver


# Stap 5 - Commands toevoegen aan Testbus


# Stap 6 - Commands toevoegen aan Eventbus Framework
