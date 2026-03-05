using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Services.Configuration;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Annique.Plugins.Payments.AdumoOnline.Services
{
    public class AdumoOnlinePaymentService : IAdumoOnlinePaymentService
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public AdumoOnlinePaymentService(ISettingService settingService,
            IStoreContext storeContext)
        {
            _settingService= settingService;
            _storeContext= storeContext;
        }

        #endregion

        /// <summary>
        /// Generate new JWT token
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="merchantReference">merchant reference</param>
        /// <param name="setting">Adumo online setting</param>
        /// <returns>JWT token</returns>
        public string GetNewJwtToken(decimal amount, string merchantReference, AdumoOnlineSettings settings)
        {
            //prepare claims
            var claims = new List<Claim>
            {
                new Claim("cuid", settings.MerchantId),
                new Claim("auid", settings.ApplicationId),
                new Claim("amount", amount.ToString("F2")), // Format as 2 decimal places
                new Claim("mref", merchantReference)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            //key for jwt token
            var key = Encoding.UTF8.GetBytes(settings.Secret);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(new JwtHeader(signingCredentials), new JwtPayload(claims));

            return tokenHandler.WriteToken(token);
        }
    }
}
