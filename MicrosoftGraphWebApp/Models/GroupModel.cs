using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicrosoftGraphWebApp.Models
{
    public class GroupModel
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }

        public GroupModel()
        {

        }

        public GroupModel(string displayName)
        {
            this.DisplayName = displayName;
        }
    }
}