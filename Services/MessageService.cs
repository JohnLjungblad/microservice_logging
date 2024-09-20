using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.IO;

public class MessageService : IHostedService {

    // Två variabler för att spara kopplingen till RabbitMQ
    private IConnection connection;
    private IModel channel;
    private IServiceProvider provider;

     public MessageService(IServiceProvider provider)
    {
        this.provider = provider;
    }

    //Lyssna efter saker som händer
    void ListenForActions()
    {
        channel.ExchangeDeclare(exchange: "logging", type: ExchangeType.Fanout);

        var queueName = channel.QueueDeclare("logging-queue", true, false, false);
        channel.QueueBind(queue: queueName, exchange: "logging", routingKey: string.Empty);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var action = Encoding.UTF8.GetString(body);

            try
            {
                using (var scope = provider.CreateScope())
                {
                    var actionSaver = scope.ServiceProvider.GetRequiredService<ActionSaver>();
                    actionSaver.SaveActions(action);  
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Error processing action: " + e.ToString());
            }
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
    }

    // Anslut till RabbitMQ
    public void Connect() {
        var factory = new ConnectionFactory { HostName = "localhost" };
        connection = factory.CreateConnection();
        channel = connection.CreateModel();

        channel.ExchangeDeclare("logging", ExchangeType.Fanout);

    }
    
    // Anropas när programmet startas
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Connect();
        ListenForActions();
        return Task.CompletedTask;
    }

    // Anropas när programmet stoppas, och då kopplar vi bort från
    // RabbitMQ
    public Task StopAsync(CancellationToken cancellationToken)
    {
        channel.Close();
        connection.Close();
        return Task.CompletedTask;
    }
}