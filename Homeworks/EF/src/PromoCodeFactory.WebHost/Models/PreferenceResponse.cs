using System;

namespace PromoCodeFactory.WebHost.Models
{
    public record PreferenceResponse
    {
        /// <summary>
        /// Id предпочтения
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Название предпочтения
        /// </summary>
        public string Name { get; set; }
    }
}
