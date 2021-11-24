using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Repository.Models;
using Send.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Send
{
    class Program
    {
        static void Main(string[] args)
        {
            SendMessageService sendMessageService = new SendMessageService();
            UserService userService = new UserService();

            Console.WriteLine("Enter your email");
            var userEmail = Console.ReadLine();
            Users userInfo = userService.IsValidUser(userEmail);
            if(userInfo == null)
            {
                Console.WriteLine("Please Signup to start chat.");
                Console.ReadKey();
                return;
            }
            if ( userInfo.isLoggedin == false)
            {
                Console.WriteLine("Please login to start chat.");
                Console.WriteLine("Enter Password: ");
                var password = Console.ReadLine();
                var attempt = 0;
                bool isLoggedIn = false;
                while (attempt < 3)
                {
                    attempt += 1;
                    isLoggedIn = userService.login(userEmail, password);
                    if(isLoggedIn == false)
                    {
                        Console.WriteLine("Enter Password again: ");
                        password = Console.ReadLine();
                    }
                    else
                    {
                        break;
                    }
                }
                if(attempt==3 && isLoggedIn == false)
                {
                    System.Environment.Exit(0);
                }
            }
            Console.WriteLine("Select User to start chat. [99] to exit. ");
            List<Users> loggedInUsers = userService.GetLoggedInUsers();
            for(int i = 0; i < loggedInUsers.Count; i++)
            {
                Console.WriteLine("{0}. {1}", i, loggedInUsers[i].Username);
            }

            Console.Write("Select: ");
            var index = Console.ReadLine();
            List<Users> listedUser = new List<Users>();
            int userSerialNo;
            while (true)
            {
                try
                {
                    userSerialNo = Convert.ToInt32(index);
                    
                }catch(Exception e)
                {
                    Console.Write("Enter number only");
                    Console.ReadKey();
                    System.Environment.Exit(0);
                }
                if (Convert.ToInt32(index) == 99)
                {
                    Console.ReadKey();
                    break;
                }
                var user = loggedInUsers[Convert.ToInt32(index)];
                listedUser.Add(user);
                Console.WriteLine("Add another? y/n");
                var yesNo = Console.ReadLine();
                if (string.Equals(yesNo, "y") || string.Equals(yesNo,"Y"))
                {
                    Console.Write("Select: ");
                    index = Console.ReadLine();
                }
                else
                {
                    //Console.ReadKey();
                    break;
                }
            }
            if (listedUser.Count > 0)
            {
                listedUser.Add(userInfo);
                var chatThread = userService.createChatThread(listedUser, userInfo);
                if(chatThread != null)
                {
                    sendMessageService.subscribeRecipientsToThread(chatThread);

                    //new Thread(() =>
                    //{
                    //    sendMessageService.subscribeRecipientsToThread(chatThread);

                    //}).Start();

                }
            }

          //  sendMessageService.startChat();

        }
    }

}
