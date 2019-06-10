using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using System.Data.Entity.Migrations;

namespace TelegramConsoleTestBot
{
    class Program
    {
        static TelegramBotClient Bot;
        static bool AppealFlag = false;

        public static List<string> AdressList = new List<string>() {"kpi13", "kimo"};
        public static WashMachineData washMachineData;

        static void Main(string[] args)
        {
            Bot = new TelegramBotClient("857014288:AAHitL7NdG94HQ7LmYuIeVk7vugOVg7PYQo");

            Bot.OnMessage += BotOnMessageReceived;

            Bot.OnCallbackQuery += BotOnCallBackQueryReceived;

            var me = Bot.GetMeAsync().Result;

            Console.WriteLine(me.FirstName);
            

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static void LoadData(string adress)
        {
            washMachineData = null;
            string url = "http://m.postirayka.com/forward/forward/index?address_name=" + adress;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            string response;

            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }

            washMachineData = JsonConvert.DeserializeObject<WashMachineData>(response);
            washMachineData.param.address = adress;

        }

        private static async void BotOnCallBackQueryReceived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
           
            string buttonText = e.CallbackQuery.Data;

            string name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName}";
            try
            {
                if (buttonText == "Back")
                {
                    await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                    var changableKeyboard = new InlineKeyboardMarkup(new[] {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Виберіть гуртожиток", "Select"),
                            InlineKeyboardButton.WithCallbackData("Відправити скаргу","Appeal")
                        }
                    });
                    await Bot.EditMessageReplyMarkupAsync(e.CallbackQuery.From.Id, e.CallbackQuery.Message.MessageId, changableKeyboard);
                    return;
                }
                if (buttonText == "Appeal")
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(new[] {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Назад", "Back")
                        }
                    });

                    using (UsersContext db = new UsersContext())
                    {
                        var tmp = db.Users.Where(t => t.UserID == e.CallbackQuery.From.Id).ToList();
                        tmp[0].AppealFlag = true;
                        db.Users.AddOrUpdate(tmp[0]);
                        db.SaveChanges();
                    }

                    await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                   
                    await Bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Напишіть будь ласка вашу скаргу!");



                    await Bot.EditMessageReplyMarkupAsync(e.CallbackQuery.From.Id, e.CallbackQuery.Message.MessageId, inlineKeyboard);


                    return;
                }


                if (buttonText == "Select")
                {
                    await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                    var inlineKeyboard = new InlineKeyboardMarkup(new[] {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("КПІ", AdressList[0]),
                            InlineKeyboardButton.WithCallbackData("КІМО", AdressList[1])
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Назад", "Back")
                        }
                    });
                    await Bot.EditMessageReplyMarkupAsync(e.CallbackQuery.From.Id, e.CallbackQuery.Message.MessageId, inlineKeyboard);
                    return;
                }

                
            }
            catch
            {
                return;
            }

            LoadData(buttonText);

            Console.WriteLine($"{name} press button {buttonText}");
            
            if(buttonText == washMachineData.param.address)
            {
                await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                string message = "";
                message = "Params:\n" + "Balance holder: " +washMachineData.param.balance_holder + "\n " + 
                    "Bill acceptor status: "+washMachineData.param.bill_acceptor_status + "\n " + 
                    "Central board status: " +washMachineData.param.central_board_status + "\n " +
                    "Floor: "+washMachineData.param.floor + "\n " + "\n "+ "\n ";

              foreach(WashMachine washMachine in washMachineData.wash_machine.Values)
                {
                    message += "Device number: " + washMachine.device_number + "\n " + 
                        "Adress: "+washMachine.address + "\n " + 
                        "Display: "+washMachine.display + "\n " + 
                        "Date: "+washMachine.date + "\n " + 
                        "Satus: "+washMachine.status + "\n " + "\n ";
                }

                await Bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, message);
            }

        }

        private static async void BotOnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != MessageType.Text)
                return;
            string name = $"{message.From.FirstName} {message.From.LastName}";


            using (UsersContext db = new UsersContext())
            {

                var tmp = db.Users.Where(tm => tm.UserID == e.Message.From.Id).ToList();
                if (tmp[0].AppealFlag == true)
                {

                    if (DateTime.Compare(tmp[0].AppealDate.AddMinutes(15), DateTime.Now) < 0)
                    {

                        await Bot.SendTextMessageAsync(message.From.Id, "Дякую ми врахуємо вашу критику!");

                        tmp[0].AppealDate = DateTime.Now;

                        db.Users.AddOrUpdate(tmp[0]);

                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(message.From.Id, "Вибачте але скаргу можна відправляти раз в 15 хвилин");
                    }
                    tmp[0].AppealFlag = false;
                }
                db.SaveChanges();
            }
 
           
           
            
            
            Console.WriteLine($"{name} send: {message.Text}");

            switch(message.Text)
            {
                case "/start":
                    
                    using (UsersContext db = new UsersContext())
                    {
                        var tmp = db.Users.Where(tm => tm.UserID == e.Message.From.Id).ToList();
                        if (tmp.Count>0)
                            break;
                        User user = new User();
                        user.Name = name;
                        user.UserID = e.Message.From.Id;
                        if(e.Message.From.Username !=null)
                            user.UserName = e.Message.From.Username;
                        user.AppealDate = DateTime.Now;
                        user.Adress = null;
                        user.AppealFlag = false;
                        db.Users.Add(user);
                        db.SaveChanges();
                    }

                    break;
                case "/hello":
                    string text = "Oh Hello there";
                    await Bot.SendTextMessageAsync(message.From.Id, text);
                    break;

                case "/menu":
                    var changableKeyboard = new InlineKeyboardMarkup(new[] {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Гуртожиток", "Select"),
                            InlineKeyboardButton.WithCallbackData("Відправити скаргу","Appeal")
                        }
                    });
                    await Bot.SendTextMessageAsync(message.From.Id, "Виберіть пункт меню", replyMarkup: changableKeyboard);
                    break;
            }
        }
    }
}
