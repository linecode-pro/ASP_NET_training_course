using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoCodeFactory.DataAccess.Repositories
{
    public class EfRepository<T>(DataContext _dataContext) : IRepository<T>
        where T : BaseEntity
    {
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var items = await _dataContext.Set<T>().AsNoTracking().ToListAsync();
            return items;
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            var item = await _dataContext.Set<T>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return item;
        }

        public async Task<T> CreateAsync(T item)
        {
            await _dataContext.Set<T>().AddAsync(item);
            await _dataContext.SaveChangesAsync();

            return item;
        }

        public async Task<T> UpdateAsync(T item)
        {
            var dbItem = await _dataContext.Set<T>().FirstOrDefaultAsync(e => e.Id == item.Id);

            if (dbItem is not null)
            {
                _dataContext.Set<T>().Remove(dbItem);
                await _dataContext.Set<T>().AddAsync(item);
                await _dataContext.SaveChangesAsync();
            }

            return item;
        }

        public async Task<bool> DeleteByIdAsync(Guid id)
        {
            var dbItem = await _dataContext.Set<T>().FirstOrDefaultAsync(e => e.Id == id);

            if (dbItem is not null)
            {
                _dataContext.Set<T>().Remove(dbItem);
                await _dataContext.SaveChangesAsync();

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
