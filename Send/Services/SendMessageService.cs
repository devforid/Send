using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Repository.Models;
using Repository.Repositories;
using Send.DTOs;

namespace Send.Services
{
    public class SendMessageService
    {
        ChatRepository chatRepository = new ChatRepository();

        public SendMessageService()
        {

        }
        public void subscribeRecipientsToThread(ChatThread chatThread)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    //channel.ExchangeDeclare(exchange: "chatSubscribe", type: "topic");
                    var queueName = channel.QueueDeclare().QueueName;
                    channel.QueueBind(queue: queueName, exchange: "chatExchange", routingKey: "subscribe_receiver");
                    //channel.QueueDeclare(queue: "subscribe_thread",
                    //             durable: false,
                    //             exclusive: false,
                    //             autoDelete: false,
                    //             arguments: null);
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(chatThread));
                    channel.BasicPublish(exchange: "chatExchange", routingKey: "subscribe_receiver", basicProperties: null, body: body);

                    startChatAsync(chatThread);

                    //// Comsume Message
                    //var consumer = new EventingBasicConsumer(channel);
                    //consumer.Received += (model, ea) =>
                    //{
                    //    var responseBody = ea.Body;
                    //    var receivedMessage = Encoding.UTF8.GetString(responseBody);
                    //    ChatThread receiveMessageBody = (ChatThread)Newtonsoft.Json.JsonConvert.DeserializeObject(receivedMessage, typeof(ChatThread));
                    //    var routingKey = ea.RoutingKey;
                    //    Console.WriteLine(routingKey);
                    //    if (receiveMessageBody.CreatorId == chatThread.CreatorId && routingKey == chatThread.ThreadId.ToString())
                    //    {
                    //        startChat(chatThread);
                    //    }
                    //};
                    //channel.BasicConsume(queue: queueName,autoAck: true,consumer: consumer);
                }
            }
        }

        public void startChatAsync(ChatThread chatThread)
        {
            //List<Task> tasks = new List<Task>();
            string message = "";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: chatThread.ThreadId.ToString(), type: "topic");
                    var queueName = channel.QueueDeclare().QueueName;
                    channel.QueueBind(queue: queueName, exchange: chatThread.ThreadId.ToString(), routingKey: chatThread.ThreadId.ToString());
                    //var queueName = chatThread.ThreadId.ToString();
                    //channel.QueueDeclare(queue: queueName,
                    //                 durable: false,
                    //                 exclusive: false,
                    //                 autoDelete: false,
                    //                 arguments: null);

                    // Comsume Message
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                                    {
                                        var body = ea.Body;
                                        var receivedMessage = Encoding.UTF8.GetString(body);
                                        MessageBody receiveMessageBody = (MessageBody)Newtonsoft.Json.JsonConvert.DeserializeObject(receivedMessage, typeof(MessageBody));
                                        var routingKey = ea.RoutingKey;
                                        //Console.WriteLine(routingKey);
                                        if (receiveMessageBody.SenderId != chatThread.CreatorId)
                                        {
                                            Console.WriteLine(" {0}>> {1}", receiveMessageBody.Sender, receiveMessageBody.Message);
                                        }
                                    };
                    channel.BasicConsume(queue: queueName,
                                         autoAck: true,
                                         consumer: consumer);

                    // Comsume Message end

                    //Send Message
                    Console.WriteLine("[1] to exit.");
                    while (true)
                    {
                        Console.Write("{0} >> ", chatThread.CreatorUsername);
                        message = Console.ReadLine();

                        if (message == "1")
                        {
                            System.Environment.Exit(0);
                        }
                        if (!String.IsNullOrWhiteSpace(message))
                        {
                            var messageBody = new MessageBody()
                            {
                                Sender = chatThread.CreatorUsername,
                                SenderId = chatThread.CreatorId,
                                Message = message
                            };
                            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageBody));
                            channel.BasicPublish(exchange: chatThread.ThreadId.ToString(), routingKey: chatThread.ThreadId.ToString(), basicProperties: null, body: body);
                            Task task = Task.Run(() => saveChatMessage(messageBody, chatThread));
                        }
                    }

                    //Send Message end
                }
            }
        }

        private async Task saveChatMessage(MessageBody messageBody, ChatThread chatThread)
        {
            Message message = new Message()
            {
                _id = Guid.NewGuid().ToString(),
                CreatorId = chatThread.CreatorId,
                SenderId = messageBody.SenderId,
                MessageBody = messageBody.Message,
                CreatorUsername = messageBody.Sender,
                ReceipientsId = chatThread.ReceipientsId,
                Sender = messageBody.Sender,
                ThreadId = chatThread.ThreadId.ToString()
            };
            await chatRepository.StoreChatMessage(message);
        }
    }
}
