using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using QC = Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace botik
{
    class ClientData
    {
        public string phoneNumber { get; set; }
        public string name { get; set; }
        public string destination { get; set; }
    }
    class Order 
    {
        public int id { get; set; }
        public string menu { get; set; }
        public string phoneNumber { get; set; }
        public string destination { get; set; }
        public orderStatuses orderStatus { get; set; }

        public string ToString(int Count) 
        {
            string orderCount = "The number of an order: " + Convert.ToString(Count) + "\n";
            string order = orderCount + "Your order ID: " + id + "\n" + "Phone number: " + phoneNumber + "\n" + "Menu: " + menu + "\n" + "Destination: " + destination + "\n" + "Status: " + orderStatus + "\n" + "\n";
            return order;
        }
    }

    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("5811430679:AAFlg0EeBWV1Jg5UGAErZb2hng6IbSW76U8");

        static DbHelper dbHelper_ = new DbHelper();
        static Dictionary<string, clientStatus> UserState = new Dictionary<string, clientStatus>();
        static Dictionary<string, Order> pendingOrders = new Dictionary<string, Order>();   // key - telegram user id, value - order
        static Dictionary<string?, Positions> Workers = new Dictionary<string?, Positions>();
        static List<Order> orderStorage = new List<Order>();
        static int currentOrderID = 0;
        //static int maxSqlID = 0;

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                if (update == null)
                {
                    return;
                }

                var message = update.Message;
                var nickname = update.Message.From.FirstName;
                int enteredChoice = 0;
                
                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Hi<3");
                    await botClient.SendTextMessageAsync(message.Chat, "Menu:\n1 - create new order;\n2 - show all orders;\n3 - change the order status to 'Preparing';\n4 - change the order status to 'Ready';\n5 - change the order status to 'En route';\n6 - change the order status to 'Delivered'; \nPlease enter your choice: ");

                    UserState.Add(nickname, clientStatus.Started);
                    return;
                }

                if (!UserState.ContainsKey(nickname))
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Not started");
                    return;
                }

                clientStatus state = UserState[nickname];

                if (state == clientStatus.Started)    // further goes menu choosing
                {
                    try
                    {
                        enteredChoice = Convert.ToInt32(message.Text.ToLower());
                        if (enteredChoice > 7 || enteredChoice < 1)
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Out of range");
                        }
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Not int");
                    }

                    switch (enteredChoice)
                    {
                        case 1:
                            await botClient.SendTextMessageAsync(message.Chat, "You've choosen the option 1. \nPlease enter your phone number: ");
                            UserState[nickname] = clientStatus.enteringPhoneNumber;
                            break;

                        case 2: // send all orders
                            string Orders = "";
                            string allOrders = "";
                            for (int i = 0; i < orderStorage.Count; i++)
                            {
                                Orders = orderStorage[i].ToString(i + 1);
                                allOrders = allOrders + Orders;
                            }
                            await botClient.SendTextMessageAsync(message.Chat, allOrders);
                            UserState[nickname] = clientStatus.Started;
                            break;

                        case 3: //status - Preparing
                            await botClient.SendTextMessageAsync(message.Chat, "Please choose the order which status u want to change and write its ID: ");
                            string OrdersCase4 = "";
                            string allOrdersCase4 = "";
                            for (int i = 0; i < orderStorage.Count; i++)
                            {
                                OrdersCase4 = orderStorage[i].ToString(i + 1);
                                allOrdersCase4 = allOrdersCase4 + OrdersCase4;
                            }
                            await botClient.SendTextMessageAsync(message.Chat, allOrdersCase4);
                            UserState[nickname] = clientStatus.statusPreparing;
                            break;

                        case 4: //status - Ready
                            await botClient.SendTextMessageAsync(message.Chat, "Please choose the order which status u want to change and write its ID: ");
                            string OrdersCase5 = "";
                            string allOrdersCase5 = "";
                            for (int i = 0; i < orderStorage.Count; i++)
                            {
                                if (orderStorage[i].orderStatus == orderStatuses.Preparing)
                                {
                                    OrdersCase5 = orderStorage[i].ToString(i + 1);
                                    allOrdersCase5 = allOrdersCase5 + OrdersCase5;
                                }
                            }
                            await botClient.SendTextMessageAsync(message.Chat, allOrdersCase5);
                            UserState[nickname] = clientStatus.statusReady;
                            break; 

                        case 5: //status - enRoute
                            await botClient.SendTextMessageAsync(message.Chat, "Please choose the order which status u want to change and write its ID: ");
                            string OrdersCase6 = "";
                            string allOrdersCase6 = "";
                            for (int i = 0; i < orderStorage.Count; i++)
                            {
                                if (orderStorage[i].orderStatus == orderStatuses.Ready)
                                {
                                    OrdersCase6 = orderStorage[i].ToString(i + 1);
                                    allOrdersCase6 = allOrdersCase6 + OrdersCase6;
                                }
                            }
                            await botClient.SendTextMessageAsync(message.Chat, allOrdersCase6);
                            UserState[nickname] = clientStatus.statusEnRoute;
                            break;

                        case 6: //status - Delivered
                            await botClient.SendTextMessageAsync(message.Chat, "Please choose the order which status u want to change and write its ID: ");
                            string OrdersCase7 = "";
                            string allOrdersCase7 = "";
                            for (int i = 0; i < orderStorage.Count; i++)
                            {
                                if (orderStorage[i].orderStatus == orderStatuses.enRoute)
                                {
                                    OrdersCase7 = orderStorage[i].ToString(i + 1);
                                    allOrdersCase7 = allOrdersCase7 + OrdersCase7;
                                }
                            }
                            await botClient.SendTextMessageAsync(message.Chat, allOrdersCase7);
                            UserState[nickname] = clientStatus.statusDelivered;
                            break;
                    }
                }

                switch (state)
                {
                    case clientStatus.enteringPhoneNumber:
                        Order order = new Order();
                        order.id = ++currentOrderID;
                        order.phoneNumber = message.Text;
                        order.orderStatus = orderStatuses.notAccepted;
                        pendingOrders[nickname] = order;

                        await botClient.SendTextMessageAsync(message.Chat, "Please enter order menu: ");
                        UserState[nickname] = clientStatus.enteringMenu;
                        break;

                    case clientStatus.enteringMenu:
                        Order order2 = pendingOrders[nickname];
                        order2.menu = message.Text;
                        pendingOrders[nickname] = order2;

                        await botClient.SendTextMessageAsync(message.Chat, "Please enter order destination: ");
                        UserState[nickname] = clientStatus.enteringDestination;
                        break;

                    case clientStatus.enteringDestination:
                        Order order3 = pendingOrders[nickname]; 
                        order3.destination = message.Text;
                        order3.orderStatus = orderStatuses.Accepted;
                        orderStorage.Add(order3);
                        pendingOrders.Remove(nickname);
                            
                        dbHelper_.executeCommand($"INSERT INTO dbo.Orders VALUES ({order3.id}, '{order3.phoneNumber}', '{order3.destination}', {(int)order3.orderStatus})");

                        await botClient.SendTextMessageAsync(message.Chat, "Order entered successfully!");
                        UserState[nickname] = clientStatus.Started;
                        break;

                    case clientStatus.statusPreparing:
                        int enteredOrderNumber = Convert.ToInt32(message.Text.ToLower());

                        if (enteredOrderNumber > currentOrderID || enteredOrderNumber <= 0) 
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "out of range");
                        }

                        for (int i = 0; i < orderStorage.Count; i++) 
                        {
                           if (orderStorage[i].id == enteredOrderNumber) 
                           {
                              orderStorage[i].orderStatus = orderStatuses.Preparing;
                              dbHelper_.executeCommand($"UPDATE BIBI.dbo.Orders SET STATUS = 2 WHERE ID = {enteredOrderNumber}");
                              await botClient.SendTextMessageAsync(message.Chat, "The status is changed to 'Preparing', you may check it by the option 2 in menu.");
                                    
                           }
                        }

                        UserState[nickname] = clientStatus.Started;
                        break;

                    case clientStatus.statusReady:
                        int enteredOrderNumberCase5 = Convert.ToInt32(message.Text.ToLower());

                        if (enteredOrderNumberCase5 > currentOrderID || enteredOrderNumberCase5 <= 0)
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "out of range");
                        }

                        for (int i = 0; i < orderStorage.Count; i++)
                        {
                            if (orderStorage[i].id == enteredOrderNumberCase5)
                            {
                                orderStorage[i].orderStatus = orderStatuses.Ready;
                                dbHelper_.executeCommand($"UPDATE BIBI.dbo.Orders SET STATUS = 3 WHERE ID = {enteredOrderNumberCase5}");
                                await botClient.SendTextMessageAsync(message.Chat, "The status is changed to 'Ready', you may check it by the option 2 in menu.");

                            }
                        }

                        UserState[nickname] = clientStatus.Started;
                        break;

                    case clientStatus.statusEnRoute:
                        int enteredOrderNumberCase6 = Convert.ToInt32(message.Text.ToLower());


                        if (enteredOrderNumberCase6 > currentOrderID || enteredOrderNumberCase6 <= 0)
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "out of range");
                        }

                        for (int i = 0; i < orderStorage.Count; i++)
                        {
                            if (orderStorage[i].id == enteredOrderNumberCase6)
                            {
                                orderStorage[i].orderStatus = orderStatuses.enRoute;
                                dbHelper_.executeCommand($"UPDATE BIBI.dbo.Orders SET STATUS = 4 WHERE ID = {enteredOrderNumberCase6}");
                                await botClient.SendTextMessageAsync(message.Chat, "The status is changed to 'en Route', you may check it by the option 2 in menu.");

                            }
                        }

                        UserState[nickname] = clientStatus.Started;
                        break;

                    case clientStatus.statusDelivered:
                        int enteredOrderNumberCase7 = Convert.ToInt32(message.Text.ToLower());

                        if (enteredOrderNumberCase7 > currentOrderID || enteredOrderNumberCase7 <= 0)
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "out of range");
                        }

                        for (int i = 0; i < orderStorage.Count; i++)
                        {
                            if (orderStorage[i].id == enteredOrderNumberCase7)
                            {
                                orderStorage[i].orderStatus = orderStatuses.Delivered;
                                dbHelper_.executeCommand($"UPDATE BIBI.dbo.Orders SET STATUS = 5 WHERE ID = {enteredOrderNumberCase7}");
                                await botClient.SendTextMessageAsync(message.Chat, "The status is changed to 'Delivered', you may check it by the option 2 in menu.");

                            }
                        }

                        UserState[nickname] = clientStatus.Started;
                        break;

                }

                if (UserState[nickname] == clientStatus.Started) 
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Menu:\n1 - create new order;\n2 - show all orders; \n3 - change the order status to 'Preparing';\n4 - change the order status to 'Ready';\n5 - change the order status to 'En route';\n6 - change the order status to 'Delivered'; \nPlease enter your choice: ");
                    
                    return;
                }
            } 
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


        static void Main(string[] args)
        {

            string cs = "Server=(localdb)\\bibi;Database=BIBI;Trusted_Connection=True;";
            dbHelper_.connectAsync(cs);
            currentOrderID = dbHelper_.executeSELECTCommand("SELECT MAX(ID) FROM BIBI.dbo.Orders");


            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}
