using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProactiveBot.Models.Messages
{
    public class ThirdMessageType : Message, ICloneable
    {
        public ThirdMessageType()
        {
            Type = "message";
        }
        public static ThirdMessageType messageApprove(ThirdMessageType message)
        {
            message.Approved = "Approved";
            return message;
        }
        public static ThirdMessageType messageReject(ThirdMessageType message)
        {
            message.Approved = "Rejected";
            return message;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public string Key { get; set; }

        public string IDTask { get; set; }
        public string Type { get; set; }
        public string TitleTask { get; set; }
        public string IDCard { get; set; }
        public string LibDispName { get; set; }
        public string Comment { get; set; }

        public string Link { get; set; }
        public string Approved { get; set; }
        public string TaskType { get; set; }
        public string CardType { get; set; } = "submit";


    }
}
