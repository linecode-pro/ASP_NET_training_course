using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Domain.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pcf.Administration.DataAccess.Data
{
    public class MongoDbInitializer(IRepository<Role> _roleCollection, IRepository<Employee> _employeeCollection) : IDbInitializer
    {
        public async void InitializeDb()
        {
            foreach (var role in FakeDataFactory.Roles)
            {
                if (await _roleCollection.GetByIdAsync(role.Id) is not null)
                    continue;

                await _roleCollection.AddAsync(role);
            }

            foreach (var employee in FakeDataFactory.Employees)
            {
                if (await _employeeCollection.GetByIdAsync(employee.Id) is not null)
                    continue;

                await _employeeCollection.AddAsync(employee);
            }
        }
    }
}
