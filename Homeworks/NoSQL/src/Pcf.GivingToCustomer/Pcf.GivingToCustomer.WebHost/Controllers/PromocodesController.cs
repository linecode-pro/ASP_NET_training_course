﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.Mappers;
using Pcf.GivingToCustomer.WebHost.Models;

namespace Pcf.GivingToCustomer.WebHost.Controllers
{
    /// <summary>
    /// Промокоды
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PromocodesController
        : ControllerBase
    {
        private readonly IRepository<PromoCode> _promoCodesRepository;
        private readonly IRepository<Preference> _preferencesRepository;
        private readonly IRepository<Customer> _customersRepository;

        public PromocodesController(IRepository<PromoCode> promoCodesRepository, 
            IRepository<Preference> preferencesRepository, IRepository<Customer> customersRepository)
        {
            _promoCodesRepository = promoCodesRepository;
            _preferencesRepository = preferencesRepository;
            _customersRepository = customersRepository;
        }
        
        /// <summary>
        /// Получить все промокоды
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<PromoCodeShortResponse>>> GetPromocodesAsync()
        {
            var promocodes = await _promoCodesRepository.GetAllAsync();

            var response = promocodes.Select(x => new PromoCodeShortResponse()
            {
                Id = x.Id,
                Code = x.Code,
                BeginDate = x.BeginDate.ToString("yyyy-MM-dd"),
                EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                PartnerId = x.PartnerId,
                ServiceInfo = x.ServiceInfo
            }).ToList();

            return Ok(response);
        }
        
        /// <summary>
        /// Создать промокод и выдать его клиентам с указанным предпочтением
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GivePromoCodesToCustomersWithPreferenceAsync(GivePromoCodeRequest request)
        {
            //Получаем предпочтение по имени
            var preference = await _preferencesRepository.GetByIdAsync(request.PreferenceId);

            if (preference == null)
            {
                return BadRequest();
            }

            //  Получаем клиентов с этим предпочтением:
            var customers = (await _customersRepository
                .GetWhere(d => d.Preferences.Any(x =>
                    x.Preference.Id == preference.Id))).ToList(); 

            PromoCode promoCode = PromoCodeMapper.MapFromModel(request, preference, customers);

            await _promoCodesRepository.AddAsync(promoCode);

            // Записать новый промокод в customers
            foreach (var customer in customers)
            {
                // Проверка с помощью LINQ
                bool hasPromoCode = customer.PromoCodes.Any(pc => pc.PromoCodeId == promoCode.Id);
                if (!hasPromoCode)
                {
                    var code = await _promoCodesRepository.GetByIdAsync(promoCode.Id);

                    if (code is null)
                    {
                        return BadRequest("Промокод не найден по id");
                    }

                    // Очистить список пользователей, так как он нам здесь не нужен!
                    code.Customers.Clear();

                    customer.PromoCodes.Add(new() {
                        PromoCodeId = promoCode.Id,
                        PromoCode = code,
                        CustomerId = customer.Id
                    });

                    // Записать изменения customer
                    await _customersRepository.UpdateAsync(customer);
                }
            }

            return CreatedAtAction(nameof(GetPromocodesAsync), new { }, null);
        }
    }
}