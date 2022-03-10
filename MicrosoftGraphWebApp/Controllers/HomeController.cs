using MicrosoftGraphWebApp.Managers.GraphAPI;
using MicrosoftGraphWebApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MicrosoftGraphWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGraphAPIManager _graphManager;

        public HomeController(IGraphAPIManager graphAPIManager)
        {
            _graphManager = graphAPIManager;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> UserDataTable()
        {
            try
            {
                List<UserModel> userList = await _graphManager.GetCustomDataAsync();
                return View(userList);
            }
            catch (Exception ex)
            {
                return View("Error", (object)ex.Message);
            }
        }
    }
}