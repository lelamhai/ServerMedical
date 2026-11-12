using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerMedical.Entities;
using ServerMedical.Models.Request.BloodTestResult;
using ServerMedical.Models.Response.BloodTestResult;
using ServerMedical.Utils;

namespace ServerMedical.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Staff")]
public class BloodTestResultController : ControllerBase
{
    private readonly MedicalContext _db;

    public BloodTestResultController(MedicalContext db)
    {
        _db = db;
    }

    [HttpPost("create/{screeningId:int}")]
    public async Task<IActionResult> Create(int screeningId, RequestUpdateBloodTestResult request)
    {
        if (string.IsNullOrWhiteSpace(request.BloodType))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Blood type is required.",
                Data = null
            });
        }

        if (request.BloodVolume <= 0)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Blood volume must be greater than zero.",
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
                Message = "A blood test result already exists for this screening.",
                Data = null
            });
        }

        if (!screening.ScreeningResult.Equals("Passed", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Blood test results can only be recorded for a volunteer who passed clinical screening.",
                Data = null
            });
        }

        var bloodTestResult = new BloodTestResult
        {
            ScreeningId = screeningId,
            BloodType = request.BloodType.Trim().ToUpperInvariant(),
            DiseaseNotes = string.IsNullOrWhiteSpace(request.DiseaseNotes)
                ? null
                : request.DiseaseNotes.Trim(),
            ResultDate = DateTime.Now,
            StaffId = staffId
        };

        screening.BloodVolume = request.BloodVolume;
        _db.BloodTestResults.Add(bloodTestResult);
        await _db.SaveChangesAsync();

        return Ok(new WrapperData<ResponseBloodTestResult>
        {
            Message = "Blood test result created successfully.",
            Data = MapResponse(screening, bloodTestResult)
        });
    }

    [HttpGet("detail/{screeningId:int}")]
    public async Task<IActionResult> GetByScreeningId(int screeningId)
    {
        var screening = await _db.HealthScreenings
            .AsNoTracking()
            .Include(item => item.Campaign)
            .Include(item => item.Volunteer)
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

        if (screening.BloodTestResult == null)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Blood test result not found for this screening.",
                Data = null
            });
        }

        return Ok(new WrapperData<ResponseBloodTestResult>
        {
            Message = "Blood test result retrieved successfully.",
            Data = MapResponse(screening, screening.BloodTestResult)
        });
    }

    [HttpPut("update/{screeningId:int}")]
    public async Task<IActionResult> Update(int screeningId, RequestUpdateBloodTestResult request)
    {
        if (string.IsNullOrWhiteSpace(request.BloodType))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Blood type is required.",
                Data = null
            });
        }

        if (request.BloodVolume <= 0)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Blood volume must be greater than zero.",
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

        if (!screening.ScreeningResult.Equals("Passed", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Blood test results can only be recorded for a volunteer who passed clinical screening.",
                Data = null
            });
        }

        var bloodTestResult = screening.BloodTestResult ?? new BloodTestResult
        {
            ScreeningId = screeningId
        };

        bloodTestResult.BloodType = request.BloodType.Trim().ToUpperInvariant();
        bloodTestResult.DiseaseNotes = string.IsNullOrWhiteSpace(request.DiseaseNotes)
            ? null
            : request.DiseaseNotes.Trim();
        bloodTestResult.ResultDate = DateTime.Now;
        bloodTestResult.StaffId = staffId;
        screening.BloodVolume = request.BloodVolume;

        if (screening.BloodTestResult == null)
        {
            _db.BloodTestResults.Add(bloodTestResult);
        }

        await _db.SaveChangesAsync();

        var response = MapResponse(screening, bloodTestResult);

        return Ok(new WrapperData<ResponseBloodTestResult>
        {
            Message = "Blood test result updated successfully.",
            Data = response
        });
    }

    private static ResponseBloodTestResult MapResponse(
        HealthScreening screening,
        BloodTestResult bloodTestResult)
    {
        return new ResponseBloodTestResult
        {
            TestResultId = bloodTestResult.TestResultId,
            ScreeningId = screening.ScreeningId,
            VolunteerId = screening.VolunteerId,
            VolunteerName = $"{screening.Volunteer.FirstName} {screening.Volunteer.LastName}".Trim(),
            CampaignId = screening.CampaignId,
            CampaignTitle = screening.Campaign.Title,
            DiseaseNotes = bloodTestResult.DiseaseNotes,
            BloodType = bloodTestResult.BloodType,
            BloodVolume = screening.BloodVolume ?? 0,
            ResultDate = bloodTestResult.ResultDate,
            StaffId = bloodTestResult.StaffId
        };
    }
}
