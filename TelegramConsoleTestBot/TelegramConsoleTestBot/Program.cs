using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramConsoleTestBot
{
    class Program
    {
        static TelegramBotClient Bot;

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

            if (buttonText == "Select")
            {
                await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                var inlineKeyboard = new InlineKeyboardMarkup(new[] {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Об'єкт 1", AdressList[0]),
                            InlineKeyboardButton.WithCallbackData("Об'єкт 2", AdressList[1])
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Назад", "Back")
                        }
                    }); 
                await Bot.EditMessageReplyMarkupAsync(e.CallbackQuery.From.Id, e.CallbackQuery.Message.MessageId, inlineKeyboard);
                return;
            }

            if (buttonText == "Back")
            {
                await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                var changableKeyboard = new InlineKeyboardMarkup(new[] {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Гуртожиток", "Select")
                        }
                    });
                await Bot.EditMessageReplyMarkupAsync(e.CallbackQuery.From.Id, e.CallbackQuery.Message.MessageId, changableKeyboard);
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

            Console.WriteLine($"{name} send: {message.Text}");

            switch(message.Text)
            {
                case "/hello":
                    string text = "Oh Hello there";
                    await Bot.SendTextMessageAsync(message.From.Id, text);
                    break;

                case "/menu":
                    var changableKeyboard = new InlineKeyboardMarkup(new[] {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Гуртожиток", "Select")
                        }
                    });
                    await Bot.SendTextMessageAsync(message.From.Id, "Воно змінюється", replyMarkup: changableKeyboard);
                    break;
            }
        }
    }
}
