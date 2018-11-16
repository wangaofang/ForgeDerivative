using System;
using System.Threading.Tasks;
using Autodesk.Forge;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ForgeDerivative.Controllers
{
    public class UserController:Controller
    {
        [HttpGet]
        [Route("api/forge/user/profile")]
        public async Task<JObject> GetUserProfileAsync()
        {
            Credentials credentials=await Credentials.FromSessionAsync();
            if(credentials==null)
            {
                return null;
            }
            UserProfileApi userApi=new UserProfileApi();
            userApi.Configuration.AccessToken=credentials.TokenInternal;
            dynamic userProfile=await userApi.GetUserProfileAsync();
            dynamic response=new JObject();
            response.name=string.Format("{0}{1}",userProfile.firstName,userProfile.lastName);
            response.picture= userProfile.profileImages.sizeX40;
            return response;
        }
    }
}