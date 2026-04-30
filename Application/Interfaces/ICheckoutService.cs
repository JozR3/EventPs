using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces
{
    public interface ICheckoutService
    {
        Task<bool> CreateCheckoutAsync(CreateCheckoutDto request);
    }
}
