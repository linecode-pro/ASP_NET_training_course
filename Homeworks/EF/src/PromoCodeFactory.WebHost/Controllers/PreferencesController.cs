using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Предпочтения
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PreferencesController(IRepository<Preference> repository) : ControllerBase
    {
        /// <summary>
        /// Получить список всех предпочтений
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public async Task<List<PreferenceResponse>> GetPreferencesAsync()
        {
            var preferences = await repository.GetAllAsync();
            var preferenceResponseList = preferences.Select(x => new PreferenceResponse { Id = x.Id, Name = x.Name }).ToList();

            return preferenceResponseList;
        }
    }
}
