using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.DBModels
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            Database.SetCommandTimeout(9000);
        }

        public DbSet<ByDEmployee> ByDEmployees { get; set; }
        public DbSet<ByDCustomer> ByDCustomers { get; set; }
    }
}
