using System.Collections.Generic;
using IdentityModel;
using IdentityServer4.Models;

namespace Auth
{
    /// <summary>
    /// 配置
    /// </summary>
    public class Config
    {
        /// <summary>
        /// 定义身份资源
        /// </summary>
        /// <returns>身份资源</returns>
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                //身份资源是用户的用户ID
                new IdentityResources.OpenId()
            };
        }

        /// <summary>
        /// 定义API资源
        /// </summary>
        /// <returns>API资源</returns>
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new[]
            {
                new ApiResource("api1", new[] {"uid", JwtClaimTypes.Name}),
                new ApiResource("api2", new[] { "uid", "name"})
            };
        }

        /// <summary>
        /// 定义客户端
        /// </summary>
        /// <returns>客户端</returns>
        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    AllowOfflineAccess = true,
                    ClientSecrets = {new Secret("secret".Sha256())},
                    AllowedScopes = {OidcConstants.StandardScopes.OfflineAccess, "api1", "api2"}
                }
            };
        }
    }
}