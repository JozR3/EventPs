using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EventoPS.Service
{
    public class AuthService : IAuthService
    {
        private readonly EventDbContext _context;

        public AuthService(EventDbContext context)
        {
            _context = context;
        }

        public async Task<UserResponseDto> RegisterAsync(RegisterUserDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email es requerido");

            if (request.Password != request.ConfirmPassword)
                throw new ArgumentException("Las contraseñas no coinciden");

            if (request.Password.Length < 6)
                throw new ArgumentException("La contraseña debe tener al menos 6 caracteres");

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
                throw new InvalidOperationException("El email ya está registrado");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }

        public async Task<UserResponseDto> LoginAsync(LoginUserDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email es requerido");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new InvalidOperationException("Usuario o contraseña incorrectos");

            if (!VerifyPassword(request.Password, user.PasswordHash))
                throw new InvalidOperationException("Usuario o contraseña incorrectos");

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }

        public async Task<UserResponseDto> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado");

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}