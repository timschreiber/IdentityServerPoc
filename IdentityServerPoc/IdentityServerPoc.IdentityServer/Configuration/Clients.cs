using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using System.Collections.Generic;

namespace IdentityServerPoc.IdentityServer.Configuration
{
    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "implicitmvc1",
                    ClientName = "ASP.NET MVC Implicit Flow Client #1",
                    Enabled = true,
                    Flow = Flows.Implicit,
                    RequireConsent = true,
                    AllowRememberConsent = true,
                    RedirectUris = new List<string> { "https://localhost:44368/auth/callback" },
                    PostLogoutRedirectUris = new List<string> { "https://localhost:44368" },
                    AllowedScopes = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email
                    },
                    AccessTokenType = AccessTokenType.Jwt
                }
            };
        }
    }
}