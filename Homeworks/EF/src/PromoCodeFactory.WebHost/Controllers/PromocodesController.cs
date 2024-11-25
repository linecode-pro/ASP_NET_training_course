using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.DataAccess.Data;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Промокоды
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PromocodesController(IRepository<PromoCode> repository, IRepository<Customer> repositoryCustomer, IRepository<Preference> repositoryPreference, DataContext _dataContext)
        : ControllerBase
    {
        /// <summary>
        /// Получить все промокоды
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<PromoCodeShortResponse>>> GetPromocodesAsync()
        {
            try
            {
                var codes = await repository.GetAllAsync();

                var response = codes.Select(x => new PromoCodeShortResponse()
                {
                    Id = x.Id,
                    Code = x.Code,
                    ServiceInfo = x.ServiceInfo,
                    BeginDate = x.BeginDate.ToString("yyyy-MM-dd"),
                    EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                    PartnerName = x.PartnerName
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Создать промокод и выдать его клиентам с указанным предпочтением
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GivePromoCodesToCustomersWithPreferenceAsync(GivePromoCodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Preference))
                return BadRequest("Не указано наименование 'предпочтения'!");

            try
            {
                // Предпочтения
                var allPreferences = await repositoryPreference.GetAllAsync();

                var preference = allPreferences.FirstOrDefault(x => x.Name == request.Preference);

                if (preference is null)
                    return BadRequest("Не найдено предпочтение по наименованию: " + request.Preference.Trim());


                // Клиенты с предпочтениями
                var customerPreferenceList = _dataContext.Set<CustomerPreference>().Where(x => x.PreferenceId == preference.Id).ToList();
                if (!customerPreferenceList.Any())
                    return NotFound($"Нет пользователей с предпочтением '{request.Preference.Trim()}' !");

                var customerIds = customerPreferenceList.Select(x => x.CustomerId).ToList();


                // Клиенты
                var allCustomer = await repositoryCustomer.GetAllAsync();

                var customers = allCustomer.Where(x => customerIds.Contains(x.Id));

                // Создать промокоды
                foreach (var customer in customers.ToList())
                {
                    PromoCode promoCode = new PromoCode
                    {
                        Id = Guid.NewGuid(),
                        ServiceInfo = request.ServiceInfo,
                        PartnerName = request.PartnerName,
                        Code = request.PromoCode,
                        Customer = _dataContext.Set<Customer>().FirstOrDefault(x => x.Id == customer.Id),
                        Preference = _dataContext.Set<Preference>().FirstOrDefault(x => x.Id == preference.Id),
                        BeginDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(1)
                    };

                    _dataContext.Set<PromoCode>().Add(promoCode);
                    _dataContext.SaveChanges();
                }

                return Ok("Промокод успешно создан!");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}