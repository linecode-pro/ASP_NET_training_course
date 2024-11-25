using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoCodeFactory.DataAccess.Data
{
    public static class DbInitializer
    {
        public static void Initialize(DataContext context)
        {
            context.Database.EnsureCreated();

            if (context.Customers.Any())
            {
                return;   // База данных уже была создана и заполнена
            }

            //context.Roles.AddRange(FakeDataFactory.Roles.ToList());
            //context.SaveChanges();

            // ---- тут надо как-то освободить контекст - роли сотрудников - от трекинга!


            context.Employees.AddRange(FakeDataFactory.Employees.ToList());
            context.SaveChanges();

            context.Preferences.AddRange(FakeDataFactory.Preferences.ToList());
            context.SaveChanges();

            context.Customers.AddRange(FakeDataFactory.Customers.ToList());
            context.SaveChanges();
        }
    }
}
