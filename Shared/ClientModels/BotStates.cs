using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthWithAdmin.Shared.ClientModels
{
    public class BotStates
    {
        public int Id { get; set; }
        public string type { get; set; }
        public string content { get; set; }
        public string contentEN { get; set; }
        public string contentAR { get; set; }

        public int? categoryId { get; set; }
        public List<BotOptions> Options { get; set; } = new List<BotOptions>();
    }
}
