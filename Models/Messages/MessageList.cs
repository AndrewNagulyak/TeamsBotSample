using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProactiveBot.Models.Messages
{
    public class MessageList
    {
        public List<NotifyMessage> messages { get; set; }
        public string MessageType { get; set; }
    }
}
