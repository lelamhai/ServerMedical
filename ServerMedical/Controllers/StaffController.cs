using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerMedical.Entities;
using ServerMedical.Models.Response.Staff;
using ServerMedical.Utils;

namespace ServerMedical.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly MedicalContext _db;

    public StaffController(MedicalContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Admin")]
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

    [Authorize(Roles = "Staff")]
    [HttpGet("schedule")]
    public async Task<IActionResult> GetCampaignSchedule([FromQuery] int page = 1)
    {
        const int pageSize = 10;

        if (page < 1)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Page must be greater than or equal to 1.",
                Data = null
            });
        }

        var staffIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(staffIdClaim, out var staffId))
        {
            return Unauthorized(new WrapperData<string>
            {
                Message = "The authenticated staff user is invalid.",
                Data = null
            });
        }

        var scheduleQuery = _db.CampaignAssignments
            .AsNoTracking()
            .Where(assignment => assignment.StaffId == staffId);

        var totalItems = await scheduleQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var schedules = await _db.CampaignAssignments
            .AsNoTracking()
            .Where(assignment => assignment.StaffId == staffId)
            .OrderBy(assignment => assignment.Campaign.EventDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(assignment => new ResponseStaffSchedule
            {
                CampaignId = assignment.CampaignId,
                Title = assignment.Campaign.Title,
                Location = assignment.Campaign.Location,
                EventDate = assignment.Campaign.EventDate,
                ShiftTime = assignment.ShiftTime
            })
            .ToListAsync();

        var response = new ResponseStaffSchedulePage
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = schedules
        };

        return Ok(new WrapperData<ResponseStaffSchedulePage>
        {
            Message = "Staff campaign schedule retrieved successfully.",
            Data = response
        });
    }
}
