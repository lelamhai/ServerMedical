using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerMedical.Entities;
using ServerMedical.Models.Response.Campaign;
using ServerMedical.Utils;

namespace ServerMedical.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CampaignRegistrationController : ControllerBase
{
    private readonly MedicalContext _db;

    public CampaignRegistrationController(MedicalContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Volunteer")]
    [HttpGet("future")]
    public Task<IActionResult> GetFuture()
    {
        return GetByRegistrationDate(isFuture: true);
    }

    [Authorize(Roles = "Volunteer")]
    [HttpGet("history")]
    public Task<IActionResult> GetHistory()
    {
        return GetByRegistrationDate(isFuture: false);
    }

    private async Task<IActionResult> GetByRegistrationDate(bool isFuture)
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

        var currentDateTime = DateTime.Now;
        var query = _db.Registrations
            .AsNoTracking()
            .Where(registration => registration.VolunteerId == volunteerId);

        query = isFuture
            ? query.Where(registration => registration.RegistrationDate >= currentDateTime)
            : query.Where(registration => registration.RegistrationDate < currentDateTime);

        var orderedQuery = isFuture
            ? query.OrderBy(registration => registration.RegistrationDate)
                .ThenBy(registration => registration.CampaignId)
            : query.OrderByDescending(registration => registration.RegistrationDate)
                .ThenBy(registration => registration.CampaignId);

        var registrations = await orderedQuery
            .Select(registration => new ResponseRegisteredCampaign
            {
                CampaignId = registration.Campaign.CampaignId,
                Title = registration.Campaign.Title,
                Location = registration.Campaign.Location,
                EventDate = registration.Campaign.EventDate,
                AdminId = registration.Campaign.AdminId,
                CreatedAt = registration.Campaign.CreatedAt
            })
            .ToListAsync();

        return Ok(new WrapperData<List<ResponseRegisteredCampaign>>
        {
            Message = isFuture
                ? "Future registered campaigns retrieved successfully."
                : "Campaign registration history retrieved successfully.",
            Data = registrations
        });
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpGet("list/{campaignId:int}")]
    public async Task<IActionResult> GetByCampaignId(int campaignId)
    {
        var campaignExists = await _db.Campaigns
            .AsNoTracking()
            .AnyAsync(campaign => campaign.CampaignId == campaignId);

        if (!campaignExists)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Campaign not found.",
                Data = null
            });
        }

        var registrations = await _db.Registrations
            .AsNoTracking()
            .Where(registration => registration.CampaignId == campaignId)
            .Include(registration => registration.Volunteer)
            .OrderBy(registration => registration.RegistrationDate)
            .Select(registration => new ResponseCampaignRegistration
            {
                RegistrationId = registration.RegistrationId,
                CampaignId = registration.CampaignId,
                VolunteerId = registration.VolunteerId,
                VolunteerName = $"{registration.Volunteer.FirstName} {registration.Volunteer.LastName}".Trim(),
                Email = registration.Volunteer.Email,
                Phone = registration.Volunteer.Phone,
                RegistrationDate = registration.RegistrationDate
            })
            .ToListAsync();

        return Ok(new WrapperData<List<ResponseCampaignRegistration>>
        {
            Message = "Campaign registrations retrieved successfully.",
            Data = registrations
        });
    }
}
