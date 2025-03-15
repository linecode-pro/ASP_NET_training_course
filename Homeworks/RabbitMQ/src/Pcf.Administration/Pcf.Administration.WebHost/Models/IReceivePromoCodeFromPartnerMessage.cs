using System;

namespace OtusRabbitMQ.Contracts
{
    public interface IReceivePromoCodeFromPartnerMessage
    {
        ReceivePromoCodeFromPartnerDto PromoCode { get; }
    }

    public class ReceivePromoCodeFromPartnerDto
    {
        public string ServiceInfo { get; set; }

        public Guid PartnerId { get; set; }

        public Guid PromoCodeId { get; set; }

        public string PromoCode { get; set; }

        public Guid PreferenceId { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public Guid? PartnerManagerId { get; set; }
    }
}
