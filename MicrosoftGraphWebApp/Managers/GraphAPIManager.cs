using Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using MicrosoftGraphWebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
//using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace MicrosoftGraphWebApp.Managers.GraphAPI
{
    public class GraphAPIManager : IGraphAPIManager
    {
        private GraphServiceClient graphClient;

        public GraphAPIManager()
        {
            var config = LoadAppSettings();
            if (config == null)
            {
                throw new Exception("Invalid appsettings.json file.");
            }
            this.graphClient = GetAuthenticatedGraphClient(config);
        }

        public async Task<List<UserModel>> GetCustomDataAsync()
        {
            List<UserModel> users = await GetUsersAsync();
            foreach (UserModel user in users)
            {
                await GetAdditionalUserDataAsync(user);
            }
            return users;
        }

        private async Task<UserModel> GetAdditionalUserDataAsync(UserModel user)
        {
            var rootFilesRequest = graphClient.Users[user.Id].Drive.Root.Children.Request().Select(x => new { x.Id, x.Name, x.Size });
            var userPhotoRequest = graphClient.Users[user.Id].Photo.Content.Request();

            Dictionary<string, IBaseRequest> requestList = new Dictionary<string, IBaseRequest>();
            requestList.Add("rootFiles", rootFilesRequest);
            requestList.Add("userPhoto", userPhotoRequest);

            Dictionary<string, HttpResponseMessage> batchResponses = await GetBatchResponsesAsync(requestList);
            await ProcessRootFileCountResponseAsync(batchResponses["rootFiles"], user);
            await ProcessPhotoResponseAsync(batchResponses["userPhoto"], user);

            return user;
        }

        private async Task<Dictionary<string, HttpResponseMessage>> GetBatchResponsesAsync(Dictionary<string, IBaseRequest> requestList)
        {
            Dictionary<string, HttpResponseMessage> resultList = new Dictionary<string, HttpResponseMessage>();

            BatchRequestContent batchRequestContent = new BatchRequestContent();

            Dictionary<string, string> requestIds = new Dictionary<string, string>();
            foreach (var request in requestList)
            {
                string requestId = batchRequestContent.AddBatchRequestStep(request.Value);
                requestIds.Add(request.Key, requestId);
            }

            BatchResponseContent batchResponseContent = await graphClient.Batch.Request().PostAsync(batchRequestContent);
            foreach (var requestId in requestIds)
            {
                HttpResponseMessage responseMsg = await batchResponseContent.GetResponseByIdAsync(requestId.Value);
                resultList.Add(requestId.Key, responseMsg);
            }
            return resultList;
        }

        private async Task ProcessRootFileCountResponseAsync(HttpResponseMessage response, UserModel user)
        {
            if (response.IsSuccessStatusCode)
            {
                user.RootFilesCountResponseSuccesfull = true;
                string content = await response.Content.ReadAsStringAsync();
                RootFilesResponse rootFilesResponse = JsonConvert.DeserializeObject<RootFilesResponse>(content);
                if (rootFilesResponse.ItemList != null)
                {
                    user.RootFilesCount = rootFilesResponse.ItemList.Count();
                }
            }
        }

        private async Task ProcessPhotoResponseAsync(HttpResponseMessage response, UserModel user)
        {
            if (response.IsSuccessStatusCode)
            {
                user.PhotoResponseSuccessful = true;
                string content = await response.Content.ReadAsStringAsync();
                if (content != null && content.Length > 0)
                {
                    user.PhotoBase64 = content;
                }
            }
        }

        private async Task<List<UserModel>> GetUsersAsync()
        {
            List<UserModel> userModelList = new List<UserModel>();
            var usersRequest = graphClient.Users.Request().Select(u => new { u.Id, u.DisplayName, u.Mail }).Expand("memberOf($select=displayName)");
            IGraphServiceUsersCollectionPage results = await usersRequest.GetAsync();
            userModelList = GetUserListFromUserCollectionPage(results);
            return userModelList;
        }

        private List<UserModel> GetUserListFromUserCollectionPage(IGraphServiceUsersCollectionPage userCollectionPage)
        {
            List<UserModel> userModelList = new List<UserModel>();
            foreach (Microsoft.Graph.User user in userCollectionPage)
            {
                UserModel userModel = new UserModel(user.Id, user.DisplayName, user.Mail);
                foreach (DirectoryObject membership in user.MemberOf)
                {
                    if (membership.GetType().Equals(typeof(Microsoft.Graph.Group)))
                    {
                        userModel.AddGroup(new GroupModel(((Microsoft.Graph.Group)membership).DisplayName));
                    }
                }
                userModelList.Add(userModel);
            }
            return userModelList;
        }

        private IConfigurationRoot LoadAppSettings()
        {
            try
            {
                var config = new ConfigurationBuilder()
                                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                .AddJsonFile("appsettings.json", false, true)
                                .Build();

                if (string.IsNullOrEmpty(config["applicationId"]) ||
                    string.IsNullOrEmpty(config["applicationSecret"]) ||
                    string.IsNullOrEmpty(config["redirectUri"]) ||
                    string.IsNullOrEmpty(config["tenantId"]))
                {
                    return null;
                }

                return config;
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
        }

        private IAuthenticationProvider CreateAuthorizationProvider(IConfigurationRoot config)
        {
            var clientId = config["applicationId"];
            var clientSecret = config["applicationSecret"];
            var redirectUri = config["redirectUri"];
            var authority = $"https://login.microsoftonline.com/{config["tenantId"]}/v2.0";

            List<string> scopes = new List<string>();
            scopes.Add("https://graph.microsoft.com/.default");

            var cca = ConfidentialClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .WithRedirectUri(redirectUri)
                                                    .WithClientSecret(clientSecret)
                                                    .Build();
            return new MsalAuthenticationProvider(cca, scopes.ToArray());
        }

        private GraphServiceClient GetAuthenticatedGraphClient(IConfigurationRoot config)
        {
            IAuthenticationProvider authenticationProvider = CreateAuthorizationProvider(config);
            GraphServiceClient graphClient = new GraphServiceClient(authenticationProvider);
            return graphClient;
        }
    }
}