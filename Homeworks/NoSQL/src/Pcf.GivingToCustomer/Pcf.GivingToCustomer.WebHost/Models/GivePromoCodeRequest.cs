using System;

namespace Pcf.GivingToCustomer.WebHost.Models
{
    /// <example>
    /// {
    ///     "serviceInfo": "Магазины Магнит",
    ///     "partnerId": "df472142-5b58-4f4a-8b77-b94b00a8e357",
    ///     "promoCodeId": "4fd06557-a994-481f-be32-782239c2d367",
    ///     "promoCode": "123-456",
    ///     "preferenceId": "c4bda62e-fc74-4256-a956-4760b3858cbd",
    ///     "beginDate": "2025-03-01",
    ///     "endDate": "2025-03-15"
    ///}
    /// </example>>
    public class GivePromoCodeRequest
    {
        public string ServiceInfo { get; set; }

        public Guid PartnerId { get; set; }

        public Guid PromoCodeId { get; set; }
        
        public string PromoCode { get; set; }

        public Guid PreferenceId { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }
    }
}