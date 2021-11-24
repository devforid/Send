using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using Repository.Models;

namespace Repository.Repositories
{
    public class UserRepository
    {
        private IMongoDatabase _db;
        public UserRepository()
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            _db = client.GetDatabase("UserDB");

        }
        public Users Get(string id)
        {
            return _db.GetCollection<Users>("Users").Find(u => u.Id == id).FirstOrDefault();
        }
        public Users GetUser(string email, string password)
        {
           return _db.GetCollection<Users>("Users").Find(u => u.Username == email && u.Password == password).FirstOrDefault();
        }
        public List<Users> GetLoggedInUsers()
        {
            return _db.GetCollection<Users>("Users").Find(u => u.isLoggedin == true).ToList();
        }
        public Users IsValidUser(string userEmail)
        {
            return _db.GetCollection<Users>("Users").Find(u => u.Username == userEmail).FirstOrDefault();
        }
        public Users Update(Users user)
        {
            try
            {
                var update = Builders<Users>.Update
                    .Set(u => u.isLoggedin, user.isLoggedin);
                var updateOption = new UpdateOptions { IsUpsert = true };
                _db.GetCollection<Users>("Users").UpdateOne(u => u.Id == user.Id, update, updateOption);
                return user;
            }
            catch
            {
                Console.WriteLine("Exception Caught");
                return null;
            }
        }

        public bool InsertThread(ChatThread chatThread)
        {
            try
            {
                _db.GetCollection<ChatThread>("ChatThread").InsertOne(chatThread);
                return true;
            }
            catch
            {
                Console.WriteLine("Exception caught");
                return false;
            }
        }
    }
}
