using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace postyraykaTest.Models.Commands
{
    public class FirstCommand : Command
    {
        public override string Name => "test";

        public override void Execute(Message message, TelegramBotClient client)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;
            // Tyt logic for command

            client.SendTextMessageAsync(chatId, "Льоша лох", replyToMessageId: messageId);
        }
    }
}