using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProactiveBot.Models.Messages
{
    public class FirstMessageType : Message
    {

        public string TitleTask { get; set; }
        public string IDTask { get; set; }
        public string Link { get; set; }
        public string IDCard { get; set; }
        public string LibDispName { get; set; }
    }
}
