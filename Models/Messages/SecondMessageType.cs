using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProactiveBot.Models.Messages
{
    public class SecondMessageType : Message
    {

        public string NewTasks { get; set; }
        public string InProgressTasks { get; set; }
        public string Link { get; set; }
        public string TerminateTasks { get; set; }
    }
}
