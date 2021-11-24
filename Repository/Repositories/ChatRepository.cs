using MongoDB.Driver;
using Send.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class ChatRepository
    {
        private IMongoDatabase _db;
        public ChatRepository()
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            _db = client.GetDatabase("UserDB");
        }

        public Task StoreChatMessage(Message message)
        {
            try
            {
                _db.GetCollection<Message>("Messages").InsertOne(message);
                return Task.FromResult<Message>(message);
            }
            catch
            {
                Console.WriteLine("Exception caught");
                return Task.FromResult<object>(null);
            }
        }
    }
}
