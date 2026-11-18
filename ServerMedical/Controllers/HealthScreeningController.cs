using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerMedical.Entities;
using ServerMedical.Models.Request.HealthScreening;
using ServerMedical.Models.Response.HealthScreening;
using ServerMedical.Utils;

namespace ServerMedical.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "Staff")]
public class HealthScreeningController : ControllerBase
{
    private readonly MedicalContext _db;

    public HealthScreeningController(MedicalContext db)
    {
        _db = db;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetList(
        [FromQuery] int? campaignId,
        [FromQuery] int? volunteerId)
    {
        var query = _db.HealthScreenings
            .AsNoTracking()
            .Include(item => item.Campaign)
            .Include(item => item.Volunteer)
            .AsQueryable();

        if (campaignId.HasValue)
        {
            query = query.Where(item => item.CampaignId == campaignId.Value);
        }

        if (volunteerId.HasValue)
        {
            query = query.Where(item => item.VolunteerId == volunteerId.Value);
        }

        var screenings = await query
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();

        return Ok(new WrapperData<List<ResponseHealthScreening>>
        {
            Message = "Health screening list retrieved successfully.",
            Data = screenings.Select(MapResponse).ToList()
        });
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(RequestCreateHealthScreening request)
    {
        if (request.VolunteerId <= 0 || request.CampaignId <= 0)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "VolunteerId and CampaignId are required.",
                Data = null
            });
        }

        if (!TryNormalizeScreeningResult(request.ScreeningResult, request.RejectionReason,
                out var screeningResult, out var rejectionReason, out var validationMessage))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = validationMessage,
                Data = null
            });
        }

        var staffId = GetStaffId();
        if (staffId == null)
        {
            return Unauthorized(new WrapperData<string>
            {
                Message = "The authenticated staff user is invalid.",
                Data = null
            });
        }

        var volunteerExists = await _db.Users.AnyAsync(user =>
            user.UserId == request.VolunteerId && user.Role == "Volunteer");
        if (!volunteerExists)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Volunteer not found.",
                Data = null
            });
        }

        var campaignExists = await _db.Campaigns.AnyAsync(campaign =>
            campaign.CampaignId == request.CampaignId);
        if (!campaignExists)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Campaign not found.",
                Data = null
            });
        }

        var isRegistered = await _db.Registrations.AnyAsync(registration =>
            registration.VolunteerId == request.VolunteerId &&
            registration.CampaignId == request.CampaignId);
        if (!isRegistered)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "The volunteer has not registered for this campaign.",
                Data = null
            });
        }

        var screeningExists = await _db.HealthScreenings.AnyAsync(screening =>
            screening.VolunteerId == request.VolunteerId &&
            screening.CampaignId == request.CampaignId);
        if (screeningExists)
        {
            return Conflict(new WrapperData<string>
            {
                Message = "A health screening record already exists for this volunteer and campaign.",
                Data = null
            });
        }

        var screening = new HealthScreening
        {
            VolunteerId = request.VolunteerId,
            CampaignId = request.CampaignId,
            ScreeningResult = screeningResult,
            RejectionReason = rejectionReason,
            StaffId = staffId.Value,
            CreatedAt = DateTime.Now
        };

        _db.HealthScreenings.Add(screening);
        await _db.SaveChangesAsync();

        await _db.Entry(screening).Reference(item => item.Campaign).LoadAsync();
        await _db.Entry(screening).Reference(item => item.Volunteer).LoadAsync();

        return Ok(new WrapperData<ResponseHealthScreening>
        {
            Message = "Health screening created successfully.",
            Data = MapResponse(screening)
        });
    }

    [HttpPut("update/{screeningId:int}")]
    public async Task<IActionResult> Update(int screeningId, RequestUpdateHealthScreening request)
    {
        if (string.IsNullOrWhiteSpace(request.ScreeningResult))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Screening result is required.",
                Data = null
            });
        }

        var screeningResult = request.ScreeningResult.Trim();
        if (!screeningResult.Equals("Passed", StringComparison.OrdinalIgnoreCase) &&
            !screeningResult.Equals("Failed", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Screening result must be Passed or Failed.",
                Data = null
            });
        }

        var isFailed = screeningResult.Equals("Failed", StringComparison.OrdinalIgnoreCase);
        if (isFailed && string.IsNullOrWhiteSpace(request.RejectionReason))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Rejection reason is required when the screening result is Failed.",
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

        var screening = await _db.HealthScreenings
            .Include(item => item.Campaign)
            .Include(item => item.Volunteer)
            .FirstOrDefaultAsync(item => item.ScreeningId == screeningId);

        if (screening == null)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Health screening record not found.",
                Data = null
            });
        }

        screening.ScreeningResult = isFailed ? "Failed" : "Passed";
        screening.RejectionReason = isFailed ? request.RejectionReason!.Trim() : null;
        screening.StaffId = staffId;

        await _db.SaveChangesAsync();

        var response = new ResponseHealthScreening
        {
            ScreeningId = screening.ScreeningId,
            VolunteerId = screening.VolunteerId,
            VolunteerName = $"{screening.Volunteer.FirstName} {screening.Volunteer.LastName}".Trim(),
            CampaignId = screening.CampaignId,
            CampaignTitle = screening.Campaign.Title,
            ScreeningResult = screening.ScreeningResult,
            RejectionReason = screening.RejectionReason,
            BloodVolume = screening.BloodVolume,
            StaffId = screening.StaffId,
            CreatedAt = screening.CreatedAt
        };

        return Ok(new WrapperData<ResponseHealthScreening>
        {
            Message = "Health screening updated successfully.",
            Data = response
        });
    }

    [HttpDelete("delete/{screeningId:int}")]
    public async Task<IActionResult> Delete(int screeningId)
    {
        var screening = await _db.HealthScreenings
            .Include(item => item.BloodTestResult)
            .FirstOrDefaultAsync(item => item.ScreeningId == screeningId);

        if (screening == null)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Health screening record not found.",
                Data = null
            });
        }

        if (screening.BloodTestResult != null)
        {
            return Conflict(new WrapperData<string>
            {
                Message = "The health screening cannot be deleted because a blood test result already exists.",
                Data = null
            });
        }

        _db.HealthScreenings.Remove(screening);
        await _db.SaveChangesAsync();

        return Ok(new WrapperData<string>
        {
            Message = "Health screening deleted successfully.",
            Data = null
        });
    }

    private int? GetStaffId()
    {
        var staffIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(staffIdClaim, out var staffId) ? staffId : null;
    }

    private static bool TryNormalizeScreeningResult(
        string? value,
        string? rejectionReason,
        out string screeningResult,
        out string? normalizedRejectionReason,
        out string validationMessage)
    {
        screeningResult = string.Empty;
        normalizedRejectionReason = null;
        validationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            validationMessage = "Screening result is required.";
            return false;
        }

        if (value.Trim().Equals("Passed", StringComparison.OrdinalIgnoreCase))
        {
            screeningResult = "Passed";
            return true;
        }

        if (value.Trim().Equals("Failed", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                validationMessage = "Rejection reason is required when the screening result is Failed.";
                return false;
            }

            screeningResult = "Failed";
            normalizedRejectionReason = rejectionReason.Trim();
            return true;
        }

        validationMessage = "Screening result must be Passed or Failed.";
        return false;
    }

    private static ResponseHealthScreening MapResponse(HealthScreening screening)
    {
        return new ResponseHealthScreening
        {
            ScreeningId = screening.ScreeningId,
            VolunteerId = screening.VolunteerId,
            VolunteerName = $"{screening.Volunteer.FirstName} {screening.Volunteer.LastName}".Trim(),
            CampaignId = screening.CampaignId,
            CampaignTitle = screening.Campaign.Title,
            ScreeningResult = screening.ScreeningResult,
            RejectionReason = screening.RejectionReason,
            BloodVolume = screening.BloodVolume,
            StaffId = screening.StaffId,
            CreatedAt = screening.CreatedAt
        };
    }
}
