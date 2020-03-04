using ProactiveBot.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProactiveBot.Models
{
    public class UserProfile
    {
        public string Name { get; set; }
        public bool IsSend { get; set; } = true;
        public int Count { get; set; } = 0;


        public List<ThirdMessageType> messagesCarousel { get; set; }
        public string CarouselId { get; set; }

    }
}
