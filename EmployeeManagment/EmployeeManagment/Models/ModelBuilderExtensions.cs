using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                    new Employee
                    {
                        Id = 1,
                        Name = "Mahmoud",
                        Email = "Mahmoud@yasso.com",
                        Department = Dept.HR,
                    },
                    new Employee
                    {
                        Id = 2,
                        Name = "Yasso",
                        Email = "Yasso@yasso.com",
                        Department = Dept.IT,
                    }
                );
        }
    }
}
