using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestApi_Example.Models;

namespace RestApi_Example.Data
{
    public class RestApi_ExampleContext : DbContext
    {
        public RestApi_ExampleContext (DbContextOptions<RestApi_ExampleContext> options)
            : base(options)
        {
        }

        public DbSet<RestApi_Example.Models.Product> Product { get; set; }

        public DbSet<RestApi_Example.Models.Brand> Brand { get; set; }

        public DbSet<RestApi_Example.Models.Category> Category { get; set; }

        public DbSet<RestApi_Example.Models.Company> Company { get; set; }
    }
}
