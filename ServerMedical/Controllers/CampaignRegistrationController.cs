using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerMedical.Entities;
using ServerMedical.Models.Response.Campaign;
using ServerMedical.Utils;

namespace ServerMedical.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin, Staff")]
public class CampaignRegistrationController : ControllerBase
{
    private readonly MedicalContext _db;

    public CampaignRegistrationController(MedicalContext db)
    {
        _db = db;
    }

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
