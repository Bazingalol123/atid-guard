namespace AuthWithAdmin.Models.Bot
{
    public class BotOptions
    {
        public float Id { get; set; }
        public int stateId { get; set; } = 0;
        public string text { get; set; } = "";
        public int nextStateId { get; set; } = 0;
        public string action { get; set; } = "";
        public string actionParams { get; set; } = "";
    }
}
