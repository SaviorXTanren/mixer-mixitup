using System.Collections.Generic;

namespace MixItUp.API.V2.Models
{
    public class GetListOfUsersResponse
    {
        public int TotalCount { get; set; }

        public List<User> Users { get; set; } = new List<User>();
    }
}
