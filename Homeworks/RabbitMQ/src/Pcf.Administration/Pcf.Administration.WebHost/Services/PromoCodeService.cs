using MassTransit;
using OtusRabbitMQ.Contracts;
using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Domain.Administration;
using Pcf.Administration.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pcf.Administration.Core.Services
{
    public class PromoCodeService(IRepository<Employee> _employeeRepository) : IConsumer<IReceivePromoCodeFromPartnerMessage>
    {
        public async Task Consume(ConsumeContext<IReceivePromoCodeFromPartnerMessage> context)
        {
            // Логика обработки сообщения
            ReceivePromoCodeFromPartnerDto promocode = context.Message.PromoCode;

            if (promocode.PartnerManagerId is null)
                throw new Exception("Ошибка! В промокоде не указан ID менеджера!");

            var employee = await _employeeRepository.GetByIdAsync(promocode.PartnerManagerId ?? Guid.Empty);

            if (employee is null)
                throw new Exception($"Ошибка! Не найден сотрудник с ID: {promocode.PartnerManagerId}");

            employee.AppliedPromocodesCount++;

            await _employeeRepository.UpdateAsync(employee);
        }
    }
}
