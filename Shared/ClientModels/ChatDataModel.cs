using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthWithAdmin.Shared.ClientModels
{
    public class ChatDataModel
    {
        public Dictionary<int, BotStates> States { get; set; } = new();
        public List<Categories> Categories { get; set; } = new();
        public Dictionary<int, List<Questions>> Questions { get; set; } = new();
        public Dictionary<int, string> nextSteps { get; set; } = new();
    }
}
