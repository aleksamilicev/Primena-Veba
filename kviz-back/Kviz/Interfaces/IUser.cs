using Kviz.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Kviz.Interfaces
{
    public interface IUser
    {
        Task<IActionResult> Register([FromBody] RegisterUserDto dto);
        Task<IActionResult> Login([FromBody] LoginUserDto dto);
        Task<IActionResult> GetProfile();
        Task<IActionResult> AdminGetUsers();
        Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto);
    }
}
