using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.DataAccess.Data
{
    public class MongoDbInitializer(IRepository<Preference> _preferenceCollection, IRepository<Customer> _customerCollection) : IDbInitializer
    {
        public async void InitializeDb()
        {
            foreach (var role in FakeDataFactory.Preferences)
            {
                if (await _preferenceCollection.GetByIdAsync(role.Id) is not null)
                    continue;

                await _preferenceCollection.AddAsync(role);
            }

            foreach (var employee in FakeDataFactory.Customers)
            {
                if (await _customerCollection.GetByIdAsync(employee.Id) is not null)
                    continue;

                await _customerCollection.AddAsync(employee);
            }
        }
    }
}
