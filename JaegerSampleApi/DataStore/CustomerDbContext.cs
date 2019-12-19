using CustomersApi.Entities;

using Microsoft.EntityFrameworkCore;

namespace CustomersApi.DataStore
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        public void Seed()
        {
            Database.EnsureCreated();

            Customers.Add(new Customer(1, "Marcelo Silva"));
            Customers.Add(new Customer(2, "Estefanie Oliveira"));
            Customers.Add(new Customer(3, "Felipe Santos"));
            Customers.Add(new Customer(4, "Veronica Lima"));
            Customers.Add(new Customer(5, "Karen Souza"));

            SaveChanges();
        }
    }
}
