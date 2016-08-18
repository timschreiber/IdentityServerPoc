using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IdentityServerPoc.ImplicitMvc1.Controllers
{
    public class AuthController : Controller
    {
        const string CLIENT_ID = "implicitmvc1";
        const string CLIENT_BASE_URI = "https://localhost:44368";
        const string CALLBACK_URI = CLIENT_BASE_URI + "/auth/callback";
        const string ID_SERVER_BASE_URI = "https://localhost:44301/core";
        const string AUTHORIZE_URI = ID_SERVER_BASE_URI + "/connect/authorize";
        const string PUBLIC_KEY = "MIIDBTCCAfGgAwIBAgIQNQb+T2ncIrNA6cKvUA1GWTAJBgUrDgMCHQUAMBIxEDAOBgNVBAMTB0RldlJvb3QwHhcNMTAwMTIwMjIwMDAwWhcNMjAwMTIwMjIwMDAwWjAVMRMwEQYDVQQDEwppZHNydjN0ZXN0MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAqnTksBdxOiOlsmRNd+mMS2M3o1IDpK4uAr0T4/YqO3zYHAGAWTwsq4ms+NWynqY5HaB4EThNxuq2GWC5JKpO1YirOrwS97B5x9LJyHXPsdJcSikEI9BxOkl6WLQ0UzPxHdYTLpR4/O+0ILAlXw8NU4+jB4AP8Sn9YGYJ5w0fLw5YmWioXeWvocz1wHrZdJPxS8XnqHXwMUozVzQj+x6daOv5FmrHU1r9/bbp0a1GLv4BbTtSh4kMyz1hXylho0EvPg5p9YIKStbNAW9eNWvv5R8HN7PPei21AsUqxekK0oW9jnEdHewckToX7x5zULWKwwZIksll0XnVczVgy7fCFwIDAQABo1wwWjATBgNVHSUEDDAKBggrBgEFBQcDATBDBgNVHQEEPDA6gBDSFgDaV+Q2d2191r6A38tBoRQwEjEQMA4GA1UEAxMHRGV2Um9vdIIQLFk7exPNg41NRNaeNu0I9jAJBgUrDgMCHQUAA4IBAQBUnMSZxY5xosMEW6Mz4WEAjNoNv2QvqNmk23RMZGMgr516ROeWS5D3RlTNyU8FkstNCC4maDM3E0Bi4bbzW3AwrpbluqtcyMN3Pivqdxx+zKWKiORJqqLIvN8CT1fVPxxXb/e9GOdaR8eXSmB0PgNUhM4IjgNkwBbvWC9F/lzvwjlQgciR7d4GfXPYsE1vf8tmdQaY8/PtdAkExmbrb9MihdggSoGXlELrPA91Yce+fiRcKY3rQlNWVd4DOoJ/cPXsXwry8pWjNCo5JD8Q+RQ5yZEy7YPoifwemLhTdsBz3hlZr28oCGJ3kbnpW0xGvQb3VHSTVVbeei0CfXoW6iz1";
        const string LOGOUT_URI = ID_SERVER_BASE_URI + "/connect/endsession";

        #region Actions
        public ActionResult SignIn()
        {
            var state = Guid.NewGuid().ToString("N");
            var nonce = Guid.NewGuid().ToString("N");
            var url = string.Format("{0}?client_id={1}&response_type=id_token&scope=openid+email+profile&redirect_uri={2}&response_mode=form_post&state={3}&nonce{4}", AUTHORIZE_URI, CLIENT_ID, CALLBACK_URI, state, nonce);

            setTempCookie(state, nonce);
            return Redirect(url);
        }

        [HttpPost]
        public async Task<ActionResult> Callback()
        {
            var token = Request.Form["id_token"];
            var state = Request.Form["state"];

            var claims = await ValidateIdentityTokenAsync(token, state);
            var id = new ClaimsIdentity(claims, "Cookies");

            Request.GetOwinContext().Authentication.SignIn(id);

            return Redirect("/");
        }

        public ActionResult SignOut()
        {
            Request.GetOwinContext().Authentication.SignOut();
            return Redirect(LOGOUT_URI);
        }
        #endregion

        void setTempCookie(string state, string nonce)
        {
            var tempId = new ClaimsIdentity("TempCookie");
            tempId.AddClaim(new Claim("state", state));
            tempId.AddClaim(new Claim("nonce", nonce));
            Request.GetOwinContext().Authentication.SignIn(tempId);
        }

        async Task<IEnumerable<Claim>> ValidateIdentityTokenAsync(string token, string state)
        {
            var cert = new X509Certificate2(Convert.FromBase64String(PUBLIC_KEY));

            var result = await Request.GetOwinContext().Authentication.AuthenticateAsync("TempCookie");

            if (result == null)
                throw new InvalidOperationException("No temp cookie");

            if (state != result.Identity.FindFirst("state").Value)
                throw new InvalidOperationException("Invalid state");

            var parameters = new TokenValidationParameters
            {
                ValidAudience = CLIENT_ID,
                ValidIssuer = ID_SERVER_BASE_URI,
                IssuerSigningKey = new X509SecurityKey(cert)
            };

            var handler = new JwtSecurityTokenHandler();
            var jwt = default(SecurityToken);
            var id = handler.ValidateToken(token, parameters, out jwt);

            if (id.FindFirst("nonce").Value != result.Identity.FindFirst("nonce").Value)
                throw new InvalidOperationException("Invalid nonce");

            Request.GetOwinContext().Authentication.SignOut("TempCookie");

            return id.Claims;
        }
    }
}