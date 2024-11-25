using PromoCodeFactory.Core.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PromoCodeFactory.Core.Domain.PromoCodeManagement
{
    public class Customer
        : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; }

        public IList<CustomerPreference> CustomerPreferences { get; set; }  // связь - предпочтения

        public IList<PromoCode> PromoCodes { get; set; }  // промокоды

        [StringLength(200)]
        public string Comment { get; set; } = string.Empty; // добавлено новое поле - для создания новой дополнительной миграции (п.8 ДЗ)
    }
}