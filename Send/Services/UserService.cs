using Repository.Models;
using Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;

namespace Send.Services
{
   public class UserService
    {
        UserRepository userRepository = new UserRepository();
        public UserService()
        {

        }
        public Users IsValidUser(string userEmail)
        {
            return userRepository.IsValidUser(userEmail);
        }
        public bool login(string email, string password)
        {
            var hashPassword = ComputeSha256Hash(password);
            var user = userRepository.GetUser(email, hashPassword);
            if(user != null)
            {
                user.isLoggedin = true;
                userRepository.Update(user);
                return true;
            }
            return false;
        }

        public List<Users> GetLoggedInUsers()
        {
            List<Users> userList = userRepository.GetLoggedInUsers();
            List<Users> modifieduserList = new List<Users>();
            var totalUser = userList.Count;
            for (int i = 0; i < totalUser; i++)
            {
                var modified = modifiedUserInfo(userList[i]);
                modifieduserList.Add(modified);
            }
            return modifieduserList;
        }

        static Users modifiedUserInfo(Users user)
        {
            Users userInfo = new Users();
            userInfo.Id= user.Id;
            userInfo.Username = user.Username;
            userInfo.isLoggedin = user.isLoggedin;
            return userInfo;
        }
        public ChatThread createChatThread(List<Users> listedUsers, Users creator)
        {
            List<string> userIds = new List<string>();
            for(int i=0;i< listedUsers.Count; i++)
            {
                userIds.Add(listedUsers[i].Id);
            }
            ChatThread chatThread = new ChatThread()
            {
                ThreadId = Guid.NewGuid(),
                CreatorId = creator.Id,
                CreatorUsername = creator.Username,
                ReceipientsId = userIds
            };
           var response = userRepository.InsertThread(chatThread);
           if (response == true) return chatThread;
           return null;
        }

        static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
