using System.Threading.Tasks;

namespace Annique.Plugins.Payments.AdumoOnline.Services
{
    public interface IAdumoOnlinePaymentService
    {
        /// <summary>
        /// Generate new JWT token
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="merchantReference">merchent reference</param>
        /// <param name="setting">Adumo online setting</param>
        /// <returns>JWT token</returns>
        string GetNewJwtToken(decimal amount, string merchantReference, AdumoOnlineSettings settings);
    }
}
