using MicrosoftGraphWebApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicrosoftGraphWebApp.Managers.GraphAPI
{
    public interface IGraphAPIManager
    {
        Task<List<UserModel>> GetCustomDataAsync();
    }
}