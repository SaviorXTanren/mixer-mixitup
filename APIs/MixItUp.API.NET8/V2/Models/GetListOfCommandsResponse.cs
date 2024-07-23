using System.Collections.Generic;

namespace MixItUp.API.V2.Models
{
    public class GetListOfCommandsResponse
    {
        public int TotalCount { get; set; }
        public List<Command> Commands { get; set; } = new List<Command>();
    }
}
