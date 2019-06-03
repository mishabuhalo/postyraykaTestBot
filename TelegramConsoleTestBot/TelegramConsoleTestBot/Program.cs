using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net;
using System.IO;

namespace TelegramConsoleTestBot
{
    class Program
    {
        static TelegramBotClient Bot;

        static void Main(string[] args)
        {
            Bot = new TelegramBotClient("857014288:AAHitL7NdG94HQ7LmYuIeVk7vugOVg7PYQo");

            Bot.OnMessage += BotOnMessageReceived;

            Bot.OnCallbackQuery += BotOnCallBackQueryReceived;

            var me = Bot.GetMeAsync().Result;

            Console.WriteLine(me.FirstName);

            LoadData();

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static void LoadData()
        {
            string url = "http://m.postirayka.com/forward/forward/index?address_name=kpi13";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            string response;

            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }



        }

        private static async void BotOnCallBackQueryReceived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            string buttonText = e.CallbackQuery.Data;
            string name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName}";
            Console.WriteLine($"{name} press button {buttonText}");

            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Oh shit here we go again");
            
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
                case "/keyboard":
                    var replyKeyBoard = new ReplyKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            new KeyboardButton("Contact") {RequestContact = true},
                            new KeyboardButton("Geo") { RequestLocation = true}
                        }
                    });
                    await Bot.SendTextMessageAsync(message.From.Id, "WTF", replyMarkup: replyKeyBoard);
                    break;
                case "/menu":
                    var inlinekeyboard = new InlineKeyboardMarkup(new[] {
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("Free Money", "https://www.youtube.com/watch?v=dQw4w9WgXcQ"),
                            InlineKeyboardButton.WithUrl("Subscribe to pewdiepie", "https://t.me/sheva_quotes")

                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("F")
                        }
                    });
                    await Bot.SendTextMessageAsync(message.From.Id, "Вибери кнопочку", replyMarkup: inlinekeyboard );
                    break;
            }
        }
    }
}
