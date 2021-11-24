using System;
using System.Collections.Generic;
using System.Text;

namespace Repository.Models
{
    public class ChatThread
    {
        public Guid ThreadId { get; set; }
        public string CreatorId { get; set; }
        public string CreatorUsername { get; set; }
        public List<string> ReceipientsId { get; set; }
    
    }
}
