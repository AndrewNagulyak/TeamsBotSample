using ProactiveBot.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProactiveBot.Models.Messages
{
    public class NotifyMessage
    {
       
       
       
        public string MessageType { get; set; }
        public string AssignedTo { get; set; }
        public string TitleTask { get; set; }
        public string IDTask { get; set; }
        public string Link { get; set; }
        public string LibDispName { get; set; }

        public string NewTasks { get; set; }
        public string InProgressTasks { get; set; }
        public string TerminateTasks { get; set; }
        public string IDCard { get; set; }
        public string TaskType { get; set; }

    }
}
