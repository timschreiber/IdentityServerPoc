using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServerPoc.IdentityServer.Configuration
{
    public static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Subject = "123",
                    Username = "test123",
                    Password = "Password123!",
                    Claims = new List<Claim>
                    {
                        new Claim(Constants.ClaimTypes.GivenName, "Test"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Onetwothree"),
                        new Claim(Constants.ClaimTypes.Email, "test123@test.test"),
                        new Claim(Constants.ClaimTypes.Role, "Administrator")
                    }
                }
            };
        }
    }
}