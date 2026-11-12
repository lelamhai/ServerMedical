using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerMedical.Entities;
using ServerMedical.Models.Response.RegisterCampaign;
using ServerMedical.Utils;

namespace ServerMedical.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Volunteer")]
public class RegistrationController : ControllerBase
{
    private readonly MedicalContext _db;

    public RegistrationController(MedicalContext db)
    {
        _db = db;
    }

    [HttpPost("register/{campaignId:int}")]
    public async Task<IActionResult> RegisterCampaign(int campaignId)
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var volunteerId))
        {
            return Unauthorized(new WrapperData<string>
            {
                Message = "The authenticated user is invalid.",
                Data = null
            });
        }

        var campaign = await _db.Campaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CampaignId == campaignId);

        if (campaign == null)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Campaign not found.",
                Data = null
            });
        }

        if (campaign.EventDate <= DateTime.Now)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Registration is closed because the campaign event date has passed.",
                Data = null
            });
        }

        var lastDonationDate = await _db.HealthScreenings
            .Where(screening =>
                screening.VolunteerId == volunteerId &&
                screening.CreatedAt.HasValue &&
                screening.BloodVolume.HasValue &&
                screening.BloodVolume > 0)
            .MaxAsync(screening => screening.CreatedAt);

        if (lastDonationDate.HasValue)
        {
            var nextEligibleDate = lastDonationDate.Value.AddDays(84);

            if (DateTime.Now < nextEligibleDate)
            {
                return BadRequest(new WrapperData<string>
                {
                    Message = $"The volunteer can register for another blood donation campaign from {nextEligibleDate:yyyy-MM-dd HH:mm}.",
                    Data = null
                });
            }
        }

        var alreadyRegistered = await _db.Registrations.AnyAsync(registration =>
            registration.VolunteerId == volunteerId && registration.CampaignId == campaignId);

        if (alreadyRegistered)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "The volunteer has already registered for this campaign.",
                Data = null
            });
        }

        var registration = new Registration
        {
            VolunteerId = volunteerId,
            CampaignId = campaignId,
            RegistrationDate = DateTime.Now
        };

        _db.Registrations.Add(registration);
        await _db.SaveChangesAsync();

        return Ok(new WrapperData<ResponseRegisterCampaign>
        {
            Message = "Campaign registration successful.",
            Data = new ResponseRegisterCampaign
            {
                RegistrationId = registration.RegistrationId,
                VolunteerId = registration.VolunteerId,
                CampaignId = registration.CampaignId,
                RegistrationDate = registration.RegistrationDate,
                EventDate = campaign.EventDate
            }
        });
    }

    [HttpDelete("cancel/{campaignId:int}")]
    public async Task<IActionResult> CancelRegistration(int campaignId)
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var volunteerId))
        {
            return Unauthorized(new WrapperData<string>
            {
                Message = "The authenticated user is invalid.",
                Data = null
            });
        }

        var registration = await _db.Registrations
            .Include(item => item.Campaign)
            .FirstOrDefaultAsync(item =>
                item.CampaignId == campaignId &&
                item.VolunteerId == volunteerId);

        if (registration == null)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "The volunteer has not registered for this campaign.",
                Data = null
            });
        }

        if (registration.Campaign.EventDate <= DateTime.Now)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Registration cannot be cancelled because the campaign event has started or ended.",
                Data = null
            });
        }

        _db.Registrations.Remove(registration);
        await _db.SaveChangesAsync();

        return Ok(new WrapperData<string>
        {
            Message = "Campaign registration cancelled successfully.",
            Data = null
        });
    }
}
