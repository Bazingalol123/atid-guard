using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthWithAdmin.Shared.ClientModels
{
    public class BotOptions
    {
        public int Id { get; set; }
        public int stateId { get; set; }
        public string text { get; set; }
        public int nextStateId { get; set; }
        public string action { get; set; }
        public string actionParams { get; set; }
    }
}
