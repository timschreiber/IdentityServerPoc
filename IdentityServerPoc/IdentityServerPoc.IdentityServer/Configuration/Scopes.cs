using IdentityServer3.Core.Models;
using System.Collections.Generic;

namespace IdentityServerPoc.IdentityServer.Configuration
{
    public static class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                StandardScopes.Roles,
                StandardScopes.OfflineAccess
            };
        }
    }
}