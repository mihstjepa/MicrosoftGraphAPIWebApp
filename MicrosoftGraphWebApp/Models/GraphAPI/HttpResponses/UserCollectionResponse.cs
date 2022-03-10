using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicrosoftGraphWebApp.Models
{
    public class UserCollectionResponse
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("value")]
        public List<User> UserList { get; set; }
    }

    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("mail")]
        public string Mail { get; set; }

        [JsonProperty("memberOf")]
        public List<Group> GroupList { get; set; }
    }

    public class Group
    {
        [JsonProperty("@odata.type")]
        public string OdataType { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
}