using Kviz.DTOs;
using Kviz.Interfaces;
using Kviz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kviz.Services
{
    public class UserService : IUser
    {
        public Task<IActionResult> AdminGetUsers()
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetProfile()
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
