using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Client.Model;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Client.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class AccountController: ControllerBase
    {
        /// <summary>
        /// http客户端
        /// </summary>
        private readonly HttpClient _client;

        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger<AccountController> _logger;

        /// <summary>
        /// 身份认证配置
        /// </summary>
        private readonly JwtBearerOptions _authOption;

        public AccountController(
            IHttpClientFactory httpClientFactory,
            ILogger<AccountController> logger,
            IOptions<JwtBearerOptions> authOptions)
        {
            _client = httpClientFactory.CreateClient();
            _logger = logger;
            _authOption = authOptions.Value;
        }

        [NonAction]
        private async Task<LoginResult> GenerateTokenAsync(string account, string password, string enterpriseId)
        {
            var disco = await _client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _authOption.Authority,
                Policy =
                {
                    RequireHttps = false
                }
            });
            if (disco.IsError)
            {
                var msg = $"用户{account}登陆到注册中心出错，错误码{disco.StatusCode}，详情{disco.Json}";
                _logger.LogError(msg);
                throw new InvalidOperationException(msg);
            }

            var parameters = new Dictionary<string, string>
            {
                { ClaimsConst.EnterpriseId, enterpriseId }
            };

            // request token
            var tokenResponse = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",
                UserName = account,
                Password = password,
                Parameters = parameters
            });

            if (tokenResponse.IsError)
            {
                var msg = $"用户{account}获取token出错，错误码{tokenResponse.HttpStatusCode}，详情{tokenResponse.Json}";
                _logger.LogError(msg);
                throw new InvalidOperationException(msg);
            }

            return new LoginResult
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                TokenType = tokenResponse.TokenType,
                ExpiresIn = tokenResponse.ExpiresIn
            };
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="login">登录信息</param>
        /// <returns>结果</returns>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResult>> Login([FromBody]Login login)
        {
            var result = await GenerateTokenAsync(login.Account, login.Password, "自定义参数");
            if (result != null)
            {
                return Ok(result);
            }
            return Unauthorized();
        }

        /// <summary>
        /// 用refresh token获取新的access token
        /// </summary>
        /// <param name="token">refresh token</param>
        /// <returns></returns>
        [HttpGet("refresh/{token}")]
        public async Task<ActionResult<LoginResult>> Refresh(string token)
        {
            var disco = await _client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _authOption.Authority,
                Policy =
                {
                    RequireHttps = false
                }
            });
            if (disco.IsError)
            {
                _logger.LogError($"获取刷新token出错，错误码{disco.StatusCode}，详情{disco.Json}");
                return null;
            }

            var tokenResponse = await _client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",
                RefreshToken = token,
                Scope = OidcConstants.StandardScopes.OfflineAccess,
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError($"获取刷新token出错，错误码{tokenResponse.HttpStatusCode}，详情{tokenResponse.Json}");
                return Unauthorized(new  { Code = 1, Msg = "刷新token已过期" });
            }

            var tokenResult = tokenResponse.MapTo<LoginResult>();
            return Ok(tokenResult);
        }
    }
}