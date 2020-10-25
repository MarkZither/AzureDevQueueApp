using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;

namespace QueueApp
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            // tell the builder to look for the appsettings.json file
            builder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Program>();

            Configuration = builder.Build();

            if (args.Length > 0)
            {
                string value = String.Join(" ", args);
                await SendArticleAsync(value);
                Console.WriteLine($"Sent: {value}");
            }
            else{
                var message = await ReceiveArticleAsync();
                Console.WriteLine($"Received {message}");
            }
        }

        static CloudQueue GetQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionString"]);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            return queueClient.GetQueueReference("newsqueue");
        }

        static async Task SendArticleAsync(string newsMessage)
        {
            var CloudQueue = GetQueue();
            bool queueCreated = await CloudQueue.CreateIfNotExistsAsync();
            if (queueCreated)
            {
                Console.WriteLine("The queue of news articles was created.");
            }

            var message = new CloudQueueMessage(newsMessage);
            await CloudQueue.AddMessageAsync(message);
        }

        static async Task<string> ReceiveArticleAsync()
        {
            var queue = GetQueue();
            bool exists = await queue.ExistsAsync();
            if (exists)
            {
                var message = await queue.GetMessageAsync();
                if(message != null)
                {
                    var newsMessage = message.AsString;
                    await queue.DeleteMessageAsync(message);
                    return newsMessage;
                }
            }

            return "<queue empty or not created>";
        }
    }
}
