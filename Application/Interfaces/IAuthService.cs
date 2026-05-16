using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(RegisterUserDto request);
        Task<UserResponseDto> LoginAsync(LoginUserDto request);
        Task<UserResponseDto> GetUserByIdAsync(int userId);
    }
}