using System.Collections.Generic;

namespace PromoCodeFactory.Core.Domain.PromoCodeManagement
{
    public class Preference
        : BaseEntity
    {
        public string Name { get; set; }

        public IList<CustomerPreference> CustomerPreferences { get; set; }  // связь - клиенты
    }
}