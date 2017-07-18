using GenericBackoffice.models.auth;
using Microsoft.Owin.Security;
using System;
using System.Web;
using System.Web.Http;

namespace GenericBackoffice.controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        [HttpPost]
        [Route("signin")]
        public AuthenticationStatus SignIn(SignInRequest request)
        {
            var user = infrastructure.IdentityProvider.GetUser(request.username, request.password);

            if (null == user)
                return AuthenticationStatus.Fail;

            HttpContext.Current.GetOwinContext().Authentication.SignIn(
                new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(15),
                    IsPersistent = true
                },
                user.ToIdentity("appAuth")
            );
            return user;
        }

        [HttpGet]
        [Route("signout")]
        public void SignOut() => HttpContext.Current.GetOwinContext().Authentication.SignOut();
    }

    public class SignInRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}
