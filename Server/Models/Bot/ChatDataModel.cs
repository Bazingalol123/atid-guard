using AuthWithAdmin.Models;
using AuthWithAdmin.Models.Bot;
namespace AuthWithAdmin.Server.Models.Bot
{
    public class ChatDataModel
    {
        public Dictionary<int, BotStates> States { get; set; } = new();
        public List<Categories> Categories { get; set; } = new();
        public Dictionary<int, List<Questions>> Questions { get; set; } = new();
        public Dictionary<int, string> nextSteps { get; set; } = new();
    }
}
