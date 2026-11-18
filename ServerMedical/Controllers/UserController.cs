using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerMedical.Entities;
using ServerMedical.Models.Response.User;
using ServerMedical.Utils;

namespace ServerMedical.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly MedicalContext _db;

    public UserController(MedicalContext db)
    {
        _db = db;
    }

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetById(int userId)
    {
        if (userId <= 0)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "User ID must be greater than zero.",
                Data = null
            });
        }

        var user = await _db.Users
            .AsNoTracking()
            .Where(user => user.UserId == userId)
            .Select(user => new ResponseUserProfile
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Qrcode = user.Qrcode,
                Avatar = user.Avatar,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "User not found.",
                Data = null
            });
        }

        return Ok(new WrapperData<ResponseUserProfile>
        {
            Message = "User information retrieved successfully.",
            Data = user
        });
    }
}
