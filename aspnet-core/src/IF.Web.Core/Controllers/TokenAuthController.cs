using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.UI;
using IF.Authentication.External;
using IF.Authentication.JwtBearer;
using IF.Authorization;
using IF.Authorization.Users;
using IF.Models.TokenAuth;
using IF.MultiTenancy;
using Abp.Domain.Repositories;
using IF.Porsche;
using Abp;
using Microsoft.EntityFrameworkCore;
using Abp.Domain.Uow;
using IF.Configuration;
using Newtonsoft.Json;

namespace IF.Controllers
{
    /// <summary>
    /// 鉴权服务接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : IFControllerBase
    {
        private readonly LogInManager _logInManager;
        private readonly ITenantCache _tenantCache;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly IExternalAuthConfiguration _externalAuthConfiguration;
        private readonly IExternalAuthManager _externalAuthManager;
        private readonly UserRegistrationManager _userRegistrationManager;
        IRepository<SmsAggregate> SmsRepository;
        IRepository<User, long> UserRepository;
        public TokenAuthController(
            LogInManager logInManager,
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            IExternalAuthConfiguration externalAuthConfiguration,
            IExternalAuthManager externalAuthManager,
            UserRegistrationManager userRegistrationManager,
            IRepository<SmsAggregate> SmsRepository,
            IRepository<User, long> UserRepository)
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _externalAuthConfiguration = externalAuthConfiguration;
            _externalAuthManager = externalAuthManager;
            _userRegistrationManager = userRegistrationManager;
            this.SmsRepository = SmsRepository;
            this.UserRepository = UserRepository;
        }

        /// <summary>
        /// 账号密码登录获取授权Token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            var loginResult = await GetLoginResultAsync(
                model.UserNameOrMobile,
                model.Password,
                GetTenancyNameOrNull()
            );

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id,
                UserName = loginResult.User.UserName,
                StoreId = loginResult.User.StoreId
            };
        }

        /// <summary>
        /// 手机验证码登录获取授权Token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [UnitOfWork(isTransactional: false)]
        [HttpPost]
        public async Task<AuthenticateResultModel> MobileAuthenticate([FromBody] MobileAuthenticateModel model)
        {
            var Sms = await SmsRepository.GetAll().Where(k => k.Mobile == model.Mobile
                    && k.Code == model.Code
                    && k.CreationTime.AddMinutes((Double)k.EffectiveTime) >= DateTime.Now)
                    .FirstOrDefaultAsync();

            if (Sms == null)
            {
                Logger.Error("短信验证失败！");
                throw new AbpException("短信验证失败!");
            }
            var DefualtPassWord = "123456";
            var user = await UserRepository.FirstOrDefaultAsync(p => p.Mobile.Equals(model.Mobile));
            if (user == null)
            {
                user = await _userRegistrationManager.RegisterAsync(
                model.Mobile,
                DefualtPassWord);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            var loginResult = await GetLoginResultAsync(
                user.Mobile,
                DefualtPassWord,
                GetTenancyNameOrNull()
            );

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id,
                Mobile = user.Mobile
            };
        }

        /// <summary>
        /// 小程序登录获取授权Token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [UnitOfWork(isTransactional: false)]
        [HttpPost]
        public async Task<AuthenticateResultModel> DecodeUserInfo([FromBody] WechatLoginModel model)
        {
            Logger.InfoFormat($"小程序登录获取授权Token开始{JsonConvert.SerializeObject(model)}");
            //获取sessionkey
            var WeChatConfig = AppConfigurations.GetAppSettings().GetSection("WeChat");
            var appid = WeChatConfig.GetSection("AppId").Value;
            var secret = WeChatConfig.GetSection("Secret").Value;
            var obj = HttpHelper.HttpGet($"https://api.weixin.qq.com/sns/jscode2session?js_code={model.js_code}&appid={appid}&secret={secret}&grant_type=authorization_code");
            Logger.InfoFormat($"小程序登录获取授权Token请求jsCode返回结果{obj}");
            var result = JsonConvert.DeserializeObject<JsCodeResult>(obj);
            if (string.IsNullOrWhiteSpace(result.session_key))
                throw new AbpException(result.errmsg);
            //创建解密对象
            DecodeInfo loginInfo = new DecodeInfo();
            loginInfo.encryptedData = model.encryptedData;
            loginInfo.iv = model.iv;
            loginInfo.session_key = result.session_key;
            var DecodeUserInfo = WeChatAppDecrypt.Decrypt(loginInfo);
            Logger.Error($"小程序登录获取授权Token解密用户信息{DecodeUserInfo}");
            var UserInfo = JsonConvert.DeserializeObject<DecodeUserInfo>(DecodeUserInfo);
            var DefualtPassWord = "123456";
            //JsCodeResult result = new JsCodeResult();
            //DecodeUserInfo UserInfo = new DecodeUserInfo();
            //UserInfo.phoneNumber = "18175950003";
            var user = await UserRepository.GetAll().Include(p => p.Wechats).FirstOrDefaultAsync(p => p.Mobile.Equals(UserInfo.phoneNumber));
            if (user == null)
            {
                user = await _userRegistrationManager.RegisterAsync(
                UserInfo.phoneNumber,
                DefualtPassWord,
                result.unionid);
                Wechat wechat = new Wechat();
                wechat.OpenId = result.openid;
                wechat.Type = 2;
                user.Wechats.Add(wechat);
                await CurrentUnitOfWork.SaveChangesAsync();
                Logger.Error($"小程序登录获取授权Token注册用户信息{JsonConvert.SerializeObject(user)}");
            }

            var loginResult = await GetLoginResultAsync(
                UserInfo.phoneNumber,
                DefualtPassWord,
                GetTenancyNameOrNull()
            );

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));
            Logger.Error($"小程序登录获取授权Token成功{loginResult.User.Id}:{UserInfo.phoneNumber}");
            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id,
                Mobile = UserInfo.phoneNumber
            };
        }



        [HttpGet]
        public List<ExternalLoginProviderInfoModel> GetExternalAuthenticationProviders()
        {
            return ObjectMapper.Map<List<ExternalLoginProviderInfoModel>>(_externalAuthConfiguration.Providers);
        }

        [HttpPost]
        public async Task<ExternalAuthenticateResultModel> ExternalAuthenticate([FromBody] ExternalAuthenticateModel model)
        {
            var externalUser = await GetExternalUserInfo(model);

            var loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    {
                        var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));
                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = accessToken,
                            EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                            ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds
                        };
                    }
                case AbpLoginResultType.UnknownExternalLogin:
                    {
                        var newUser = await RegisterExternalUserAsync(externalUser);
                        if (!newUser.IsActive)
                        {
                            return new ExternalAuthenticateResultModel
                            {
                                WaitingForActivation = true
                            };
                        }

                        // Try to login again with newly registered user!
                        loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());
                        if (loginResult.Result != AbpLoginResultType.Success)
                        {
                            throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                                loginResult.Result,
                                model.ProviderKey,
                                GetTenancyNameOrNull()
                            );
                        }

                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity)),
                            ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds
                        };
                    }
                default:
                    {
                        throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                            loginResult.Result,
                            model.ProviderKey,
                            GetTenancyNameOrNull()
                        );
                    }
            }
        }

        private async Task<User> RegisterExternalUserAsync(ExternalAuthUserInfo externalUser)
        {
            var user = await _userRegistrationManager.RegisterAsync(
                externalUser.Mobile,
                Authorization.Users.User.CreateRandomPassword());

            user.Logins = new List<UserLogin>
            {
                new UserLogin
                {
                    LoginProvider = externalUser.Provider,
                    ProviderKey = externalUser.ProviderKey,
                    TenantId = user.TenantId
                }
            };

            await CurrentUnitOfWork.SaveChangesAsync();

            return user;
        }

        private async Task<ExternalAuthUserInfo> GetExternalUserInfo(ExternalAuthenticateModel model)
        {
            var userInfo = await _externalAuthManager.GetUserInfo(model.AuthProvider, model.ProviderAccessCode);
            if (userInfo.ProviderKey != model.ProviderKey)
            {
                throw new UserFriendlyException(L("CouldNotValidateExternalUser"));
            }

            return userInfo;
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrMobile, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrMobile, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrMobile, tenancyName);
            }
        }

        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? _configuration.Expiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            });

            return claims;
        }

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
        }
    }
}
