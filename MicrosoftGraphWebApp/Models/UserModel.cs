using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicrosoftGraphWebApp.Models
{
    public class UserModel 
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Mail { get; set; }
        public List<GroupModel> GroupList { get; set; } = new List<GroupModel>();
        public int RootFilesCount { get; set; }
        public bool RootFilesCountResponseSuccesfull { get; set; }
        public string PhotoBase64 { get; set ; }
        public bool PhotoResponseSuccessful { get; set; }

        public UserModel()
        {

        }

        public UserModel(string id, string displayName, string mail)
        {
            this.Id = id;
            this.DisplayName = displayName;
            this.Mail = mail;
        }

        public void SetGroupList(List<GroupModel> groupList)
        {
            this.GroupList = groupList;
        }

        public void AddGroup(GroupModel group)
        {
            this.GroupList.Add(group);
        }

        public string GetGroupListString()
        {
            string result = string.Empty;
            foreach (var g in GroupList)
            {
                result += $"{g.DisplayName}\n";
            }
            return result;
        }
    }
}