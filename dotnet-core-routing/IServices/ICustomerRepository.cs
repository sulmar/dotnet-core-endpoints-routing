using dotnet_core_routing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_core_routing.IServices
{
    public interface ICustomerRepository
    {
        Customer Get(int id);
    }
}
