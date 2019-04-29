using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace Auth
{
    /// <summary>
    /// Handles validation of resource owner password credentials
    /// </summary>
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        /// <summary>Validates the resource owner password credential</summary>
        /// <param name="context">The context.</param>
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            try
            {
                //根据context.UserName和context.Password与数据库的数据做校验，判断是否合法
                if (context.UserName == "wjk" && context.Password == "123")
                {
                    context.Result = new GrantValidationResult(
                        subject: context.UserName,
                        authenticationMethod: "custom",
                        claims: GetUserClaims());
                }
                else
                {

                    //验证失败
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "invalid custom credential");
                }
            }
            catch (System.Exception exception)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid username or password");
                throw;
            }
        }

        //可以根据需要设置相应的Claim
        private static IEnumerable<Claim> GetUserClaims()
        {
            return new[]
            {
                new Claim("uid", "1"),
                new Claim(JwtClaimTypes.Name,"wjk"),
                new Claim(JwtClaimTypes.GivenName, "GivenName"),
                new Claim(JwtClaimTypes.FamilyName, "yyy"),
                new Claim(JwtClaimTypes.Email, "977865769@qq.com"),
                new Claim(JwtClaimTypes.Role,"admin")
            };
        }
    }
}