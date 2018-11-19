using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Autodesk.Forge;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ForgeDerivative.Controllers
{

    public struct AccessToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }
    public class OAuthController : Controller
    {
        [HttpGet]
        [Route("api/forge/oauth/token")]
        public async Task<AccessToken> GetPublicTokenAsync()
        {
            Credentials credentials = await Credentials.FromSessionAsync();
            return new AccessToken
            {
                access_token = credentials.TokenPublic,
                expires_in = (int)credentials.ExpiresAt.Subtract(DateTime.Now).TotalSeconds
            };
        }

        [HttpGet]
        [Route("api/forge/oauth/signout")]
        public HttpResponseMessage Signout()
        {
            HttpContext.Session.Clear();            
            HttpResponseMessage res = new HttpResponseMessage(HttpStatusCode.Moved);
            res.Headers.Location = new Uri("/", UriKind.Relative);
            return res;
        }

        [HttpGet]
        [Route("api/forge/oauth/url")]
        public string GetOAuthURL()
        {
            Scope[] scopes = { Scope.DataRead };
            ThreeLeggedApi _threeLeggedApi = new ThreeLeggedApi();
            string oauthUrl = _threeLeggedApi.Authorize(
                Startup.Configuration["ForgeAPIID:FORGE_CLIENT_ID"],
                oAuthConstants.CODE,
                Startup.Configuration["ForgeAPIID:FORGE_CALLBACK_URL"],
                new Scope[] { Scope.DataRead, Scope.ViewablesRead }
            );

            // string url=Uri.EscapeDataString(oauthUrl);
            string url = Uri.UnescapeDataString(oauthUrl);

            return oauthUrl;
        }

        [HttpGet]
        [Route("api/forge/callback/oauth")]        
        public async Task<HttpResponseMessage> OAuthCallbackAsync(string code)
        // public async void OAuthCallbackAsync(string code)
        {
            Credentials credentials = await Credentials.CreateFromCodeAsync(code);            
            HttpResponseMessage res =new HttpResponseMessage(HttpStatusCode.Moved);
            res.Headers.Location = new Uri("/", UriKind.Relative);              
            // Redirect("/");            
            return res;
            
        }
    }

    public class Credentials
    {
        //依赖注入不能在静态方法里正常使用
        // private readonly IHttpContextAccessor _httpContextAccessor
        // public Credentials(IHttpContextAccessor httpContextAccessor)
        // {
        //     _httpContextAccessor=httpContextAccessor;
        // }  
        // private   ISession _session=>_httpContextAccessor.HttpContext.Session;


        //https://stackoverflow.com/questions/37329354/how-to-use-ihttpcontextaccessor-in-static-class-to-set-cookies
        private static IHttpContextAccessor _httpContextAccessor;
        public static void SetHttpContextAccessor(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }
        private static ISession _session => _httpContextAccessor.HttpContext.Session;

        private Credentials() { }

        public string TokenInternal { get; set; }
        public string TokenPublic { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }

        public static async Task<Credentials> CreateFromCodeAsync(string code)
        {
            ThreeLeggedApi oauth = new ThreeLeggedApi();
            dynamic credentialInteral = await oauth.GettokenAsync(
                Startup.Configuration["ForgeAPIID:FORGE_CLIENT_ID"], Startup.Configuration["ForgeAPIID:FORGE_CLIENT_SECRET"],
                oAuthConstants.AUTHORIZATION_CODE, code, Startup.Configuration["ForgeAPIID:FORGE_CALLBACK_URL"]);

            dynamic credentialPublic = await oauth.RefreshtokenAsync(
                Startup.Configuration["ForgeAPIID:FORGE_CLIENT_ID"], Startup.Configuration["ForgeAPIID:FORGE_CLIENT_SECRET"],
                 "refresh_token", credentialInteral.refresh_token, new Scope[] { Scope.ViewablesRead });

            Credentials credentials = new Credentials
            {
                TokenInternal = credentialInteral.access_token,
                TokenPublic = credentialPublic.access_token,
                RefreshToken = credentialPublic.refresh_token,
                ExpiresAt = DateTime.Now.AddSeconds(credentialInteral.expires_in)
            };

            _session.SetString("ForgeCredentials", JsonConvert.SerializeObject(credentials));
            return credentials;
        }

        public static async Task<Credentials> FromSessionAsync()
        {
            if (_session == null || _session.GetString("ForgeCredentials") == null)
            {
                return null;
            }
            Credentials credentials = JsonConvert.DeserializeObject<Credentials>(_session.GetString("ForgeCredentials"));

            if (credentials.ExpiresAt < DateTime.Now)
            {
                Credentials refreshedCredentials = await credentials.RefreshAsync();
                return refreshedCredentials;
            }
            return credentials;
        }

        private async Task<Credentials> RefreshAsync()
        {
            ThreeLeggedApi oauth = new ThreeLeggedApi();

            dynamic credentialInteral = await oauth.RefreshtokenAsync(
                 Startup.Configuration["ForgeAPIID:FORGE_CLIENT_ID"], Startup.Configuration["ForgeAPIID:FORGE_CLIENT_SECRET"],
                  "refresh_token", RefreshToken, new Scope[] { Scope.ViewablesRead });

            dynamic credentialPublic = await oauth.RefreshtokenAsync(
                Startup.Configuration["ForgeAPIID:FORGE_CLIENT_ID"], Startup.Configuration["ForgeAPIID:FORGE_CLIENT_SECRET"],
                 "refresh_token", credentialInteral.refresh_token, new Scope[] { Scope.ViewablesRead });

            Credentials credentials = new Credentials
            {
                TokenInternal = credentialInteral.access_token,
                TokenPublic = credentialPublic.access_token,
                RefreshToken = credentialPublic.refresh_token,
                ExpiresAt = DateTime.Now.AddSeconds(credentialInteral.expires_in)
            };
            return credentials;
        }
    }
}