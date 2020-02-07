using dotnet_core_routing.IServices;
using dotnet_core_routing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_core_routing.Services
{
    public class FakeCustomerRepository : ICustomerRepository
    {
        public Customer Get(int id)
        {
            var customer = new  Customer { FirstName = "Marcin", LastName = "Sulecki" };

            return customer;
        }
    }
}
