using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerMedical.Entities;
using ServerMedical.Models.Response.Staff;
using ServerMedical.Utils;

namespace ServerMedical.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase
{
    private readonly MedicalContext _db;

    public StaffController(MedicalContext db)
    {
        _db = db;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Enter a name or email to search for staff.",
                Data = null
            });
        }

        var normalizedText = text.Trim();

        var staff = await _db.Users
            .AsNoTracking()
            .Where(user =>
                user.Role == "Staff" &&
                (EF.Functions.Like(
                     user.FirstName + " " + user.LastName,
                     $"%{normalizedText}%") ||
                 EF.Functions.Like(user.Email, $"%{normalizedText}%")))
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .Select(user => new ResponseStaffProfile
            {
                StaffId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Avatar = user.Avatar,
                CreatedAt = user.CreatedAt
            })
            .ToListAsync();

        return Ok(new WrapperData<List<ResponseStaffProfile>>
        {
            Message = staff.Count == 0
                ? "No staff found."
                : "Staff profiles retrieved successfully.",
            Data = staff
        });
    }
}
