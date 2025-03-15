using MassTransit;
using OtusRabbitMQ.Contracts;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.Integration;
using Pcf.GivingToCustomer.WebHost.Mappers;
using Pcf.GivingToCustomer.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.Core.Services
{
    public class PromoCodeService(IRepository<PromoCode> _promoCodesRepository,
            IRepository<Preference> _preferencesRepository, IRepository<Customer> _customersRepository) : IConsumer<IReceivePromoCodeFromPartnerMessage>
    {
        public async Task Consume(ConsumeContext<IReceivePromoCodeFromPartnerMessage> context)
        {
            // Логика обработки сообщения
            ReceivePromoCodeFromPartnerDto promocode = context.Message.PromoCode;

            // Получить предпочтение
            var preference = await _preferencesRepository.GetByIdAsync(promocode.PreferenceId);

            if (preference is null)
                throw new Exception("Ошибка! Не определено предпочтение по ID предпочтения!");

            //  Получить клиентов с этим предпочтением:
            var customers = await _customersRepository
                .GetWhere(d => d.Preferences.Any(x =>
                    x.Preference.Id == preference.Id));

            GivePromoCodeRequest request = new()
            {
                PartnerId = promocode.PartnerId,
                BeginDate = promocode.BeginDate,
                EndDate = promocode.EndDate,
                PreferenceId = promocode.PreferenceId,
                PromoCode = promocode.PromoCode,
                ServiceInfo = promocode.ServiceInfo,
                PromoCodeId = promocode.PromoCodeId
            };

            PromoCode promoCode = PromoCodeMapper.MapFromModel(request, preference, customers);

            await _promoCodesRepository.AddAsync(promoCode);
        }
    }
}
