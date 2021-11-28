using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // in Package manager console
        // Add-Migration migratin-name
        // Update-Database
        // Update-Database migration-name => if old migration undo the next migration
        // Remove-Migration => remove last migration if undo of not applyed in database

        public DbSet<Employee> Employees { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // seed data
            builder.Seed();

            // make on delete no action insted of on delete cascade(default)
            foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
