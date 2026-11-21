using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerMedical.Entities;
using ServerMedical.Models.Response.Volunteer;
using ServerMedical.Utils;

namespace ServerMedical.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "Admin, Staff")]
public class VolunteerController : ControllerBase
{
    private readonly MedicalContext _db;

    public VolunteerController(MedicalContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpGet("list")]
    public async Task<IActionResult> GetList()
    {
        var volunteers = await _db.Users
            .AsNoTracking()
            .Where(user => user.Role == "Volunteer")
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .Select(user => new ResponseVolunteerProfile
            {
                VolunteerId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                Phone = user.Phone,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Avatar = user.Avatar,
                CreatedAt = user.CreatedAt
            })
            .ToListAsync();

        return Ok(new WrapperData<List<ResponseVolunteerProfile>>
        {
            Message = volunteers.Count == 0
                ? "No volunteer profile found."
                : "Volunteer profiles retrieved successfully.",
            Data = volunteers
        });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Enter a phone number or email to search for a volunteer.",
                Data = null
            });
        }

        var normalizedText = text.Trim();

        var volunteers = await _db.Users
            .AsNoTracking()
            .Where(user =>
                user.Role == "Volunteer" &&
                (user.Phone != null && user.Phone.Contains(normalizedText) ||
                 user.Email.Contains(normalizedText)))
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .Select(user => new ResponseVolunteerProfile
            {
                VolunteerId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                Phone = user.Phone,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Avatar = user.Avatar,
                CreatedAt = user.CreatedAt
            })
            .ToListAsync();

        return Ok(new WrapperData<List<ResponseVolunteerProfile>>
        {
            Message = volunteers.Count == 0
                ? "No volunteer profile found."
                : "Volunteer profiles retrieved successfully.",
            Data = volunteers
        });
    }

    [HttpGet("history/{volunteerId:int}")]
    public async Task<IActionResult> GetDonationHistory(int volunteerId)
    {
        var volunteerExists = await _db.Users
            .AsNoTracking()
            .AnyAsync(user => user.UserId == volunteerId && user.Role == "Volunteer");

        if (!volunteerExists)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Volunteer not found.",
                Data = null
            });
        }

        var screenings = await _db.HealthScreenings
            .AsNoTracking()
            .Include(screening => screening.Campaign)
            .Include(screening => screening.BloodTestResult)
            .Where(screening => screening.VolunteerId == volunteerId)
            .OrderByDescending(screening => screening.CreatedAt)
            .ToListAsync();

        var history = screenings.Select(screening => new ResponseDonationHistory
        {
            ScreeningId = screening.ScreeningId,
            VolunteerId = screening.VolunteerId,
            CampaignId = screening.CampaignId,
            CampaignTitle = screening.Campaign.Title,
            CampaignLocation = screening.Campaign.Location,
            CampaignEventDate = screening.Campaign.EventDate,
            DonationDate = screening.CreatedAt,
            ScreeningResult = screening.ScreeningResult,
            RejectionReason = screening.RejectionReason,
            BloodVolume = screening.BloodVolume,
            BloodType = screening.BloodTestResult?.BloodType,
            IsSuccessful = screening.BloodVolume > 0
        }).ToList();

        return Ok(new WrapperData<List<ResponseDonationHistory>>
        {
            Message = "Volunteer donation history retrieved successfully.",
            Data = history
        });
    }
}
