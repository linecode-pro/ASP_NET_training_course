using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.DataAccess.Data;
using PromoCodeFactory.DataAccess.Repositories;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Клиенты
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomersController(IRepository<Customer> repository, IRepository<Preference> repositoryPreference, DataContext _dataContext)
        : ControllerBase
    {
        /// <summary>
        /// Получить данные всех клиентов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<Customer>>> GetCustomersAsync()
        {
            try
            {
                var customers = await repository.GetAllAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }           
        }

        /// <summary>
        /// Получить данные клиента по Id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerResponse>> GetCustomerAsync(Guid id)
        {
            try
            {
                // 1. Получить клиента
                var customer = await repository.GetByIdAsync(id);

                if (customer is null)
                    return NotFound($"Не найден клиент по id: {id}");


                // 2. Получить предпочтения
                List<CustomerPreference> customerPreferenceList = _dataContext.Set<CustomerPreference>().Where(x => x.CustomerId == id).ToList();

                foreach (var item in customerPreferenceList)
                {
                   item.Preference = _dataContext.Set<Preference>().FirstOrDefault(x => x.Id == item.PreferenceId);
                }

                
                // 3. Получить промокоды
                List<PromoCode> promoCodeList = _dataContext.Set<PromoCode>().Where(x => x.Customer.Id == id).ToList();

                
                // 4. Подготовить ответ
                var customerResponse = new CustomerResponse(customer, customerPreferenceList, promoCodeList);
                
                return Ok(customerResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Добавить нового клиента
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateCustomerAsync(CreateOrEditCustomerRequest request)
        {
            try
            {
                var preferences = await repositoryPreference.GetAllAsync();

                // 1. Создать нового клиента
                var customer = new Customer()
                {
                    Id = Guid.NewGuid(),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    CustomerPreferences = request.PreferenceIds?.Select(id => new CustomerPreference()
                    {
                        PreferenceId = id
                    }).ToList(),
                };

                var dbCustomer = await repository.CreateAsync(customer);


                // 2. Сформировать список предпочтений
                List<CustomerPreference> customerPreferenceList = _dataContext.Set<CustomerPreference>().Where(x => x.CustomerId == dbCustomer.Id).ToList();

                foreach (var item in customerPreferenceList)
                {
                    item.Preference = _dataContext.Set<Preference>().FirstOrDefault(x => x.Id == item.PreferenceId);
                }


                // 3. Подготовить ответ
                var customerResponse = new CustomerResponse(dbCustomer, customerPreferenceList, default);

                return StatusCode(StatusCodes.Status201Created, customerResponse);      
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Обновить данные клиента
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> EditCustomersAsync(Guid id, CreateOrEditCustomerRequest request)
        {
            try
            {
                // 1. Получить клиента
                var customer = await repository.GetByIdAsync(id);

                if (customer is null)
                    return NotFound($"Не найден клиент по id: {id}");


                // 2. Получить текущие предпочтения и удалить их
                var customerPreferenceList = _dataContext.Set<CustomerPreference>().Where(x => x.CustomerId == customer.Id).ToList();

                if (customerPreferenceList.Any())
                {   
                    _dataContext.Set<CustomerPreference>().RemoveRange(customerPreferenceList);
                    _dataContext.SaveChanges();
                }


                // 3. Задать новые значения
                customer.FirstName = request.FirstName;
                customer.LastName = request.LastName;
                customer.Email = request.Email;
                customer.CustomerPreferences = request.PreferenceIds?.Select(id => new CustomerPreference()
                {
                    PreferenceId = id
                }).ToList();

                customer = await repository.UpdateAsync(customer);


                // 4. Сформировать новый список предпочтений
                customerPreferenceList = _dataContext.Set<CustomerPreference>().Where(x => x.CustomerId == customer.Id).ToList();

                foreach (var item in customerPreferenceList)
                {
                    item.Preference = _dataContext.Set<Preference>().FirstOrDefault(x => x.Id == item.PreferenceId);
                }


                // 5. Получить промокоды
                List<PromoCode> promoCodeList = _dataContext.Set<PromoCode>().Where(x => x.Customer.Id == id).ToList();


                // 6. Подготовить ответ
                var customerResponse = new CustomerResponse(customer, customerPreferenceList, default);

                return Ok(customerResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Удалить клиента по Id
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            try
            {
                // 1. Получить клиента
                var customer = await repository.GetByIdAsync(id);

                if (customer is null)
                    return NotFound($"Не найден клиент по id: {id}");


                await repository.DeleteByIdAsync(id);

                return Ok("Клиент был успешно удален");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}