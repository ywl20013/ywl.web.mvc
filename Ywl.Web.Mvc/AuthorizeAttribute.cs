using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Ywl.Web.Mvc
{
    public class AuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        private void SetPrincipal(System.Security.Principal.IPrincipal principal)
        {
            System.Threading.Thread.CurrentPrincipal = principal;
            if (System.Web.HttpContext.Current != null)
            {
                System.Web.HttpContext.Current.User = principal;
            }
        }
        // TODO: Here is where you would validate the username and password.
        protected virtual bool CheckPassword(string username, string password)
        {
            return username == "admin" && password == "abc.123";
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("HttpContext");
            }
            return base.AuthorizeCore(httpContext);
        }
        private class LoginInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Pw { get; set; }
        }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            //string actionName = filterContext.ActionDescriptor.ActionName;

            //已登录
            //System.Web.HttpContext.Current.Request.IsAuthenticated

            var cookie = filterContext.HttpContext.Request.Cookies["ywl.web.login"];
            var authHeader = filterContext.HttpContext.Request.Headers["Authorization"];
            if (authHeader == null && cookie == null)
            {
                var response = HttpContext.Current.Response;
                HttpContext.Current.Response.StatusCode = 401;
                if (response.StatusCode == 401)
                {
                    //response.Headers.Add("WWW-Authenticate",
                    //    string.Format("Basic realm=\"{0}\"", "ywl.web Realm"));
                }
            }
            else if (cookie != null)
            {
                var value = filterContext.HttpContext.Server.UrlDecode(filterContext.HttpContext.Request.Cookies[0].Value);
                var loginInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginInfo>(value);

                if (CheckPassword(loginInfo.Id.ToString(), loginInfo.Pw))
                {
                    var identity = new GenericIdentity(loginInfo.Id.ToString());
                    SetPrincipal(new GenericPrincipal(identity, null));
                }
                else
                {
                    // Invalid username or password.
                    HttpContext.Current.Response.StatusCode = 401;
                }
            }
            else if (authHeader != null)
            {
                var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);
                // RFC 2617 sec 1.2, "scheme" name is case-insensitive
                if (authHeaderVal.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) &&
                    authHeaderVal.Parameter != null)
                {
                    var credentials = authHeaderVal.Parameter;

                    var encoding = Encoding.GetEncoding("iso-8859-1");
                    credentials = encoding.GetString(Convert.FromBase64String(credentials));

                    int separator = credentials.IndexOf(':');
                    string name = credentials.Substring(0, separator);
                    string password = credentials.Substring(separator + 1);

                    if (CheckPassword(name, password))
                    {
                        var identity = new GenericIdentity(name);
                        SetPrincipal(new GenericPrincipal(identity, null));
                    }
                    else
                    {
                        // Invalid username or password.
                        HttpContext.Current.Response.StatusCode = 401;
                    }
                }
            }

            // base.OnAuthorization(filterContext);
            //throw new NotImplementedException();
        }
    }
}
