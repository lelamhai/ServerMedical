using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerMedical.Entities;
using ServerMedical.Models.Request.Campaign;
using ServerMedical.Models.Response.Campaign;
using ServerMedical.Utils;

namespace ServerMedical.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class CampaignController : ControllerBase
{
    private readonly MedicalContext _db;

    public CampaignController(MedicalContext db)
    {
        _db = db;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(RequestCreateCampaign request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Title is required.",
                Data = null
            });
        }

        if (string.IsNullOrWhiteSpace(request.Location))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Location is required.",
                Data = null
            });
        }

        if (request.EventDate == default)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Event date is required.",
                Data = null
            });
        }

        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var adminId))
        {
            return Unauthorized(new WrapperData<string>
            {
                Message = "The authenticated user is invalid.",
                Data = null
            });
        }

        var isAdmin = await _db.Users.AnyAsync(user =>
            user.UserId == adminId && user.Role == "Admin");

        if (!isAdmin)
        {
            return Forbid();
        }

        var assignmentRequests = request.StaffAssignments ?? new List<RequestCampaignAssignment>();
        var staffIds = assignmentRequests.Select(assignment => assignment.StaffId).ToList();

        if (staffIds.Count != staffIds.Distinct().Count())
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "A staff member cannot be assigned more than once to the same campaign.",
                Data = null
            });
        }

        var staffMembers = await _db.Users
            .Where(user => staffIds.Contains(user.UserId) && user.Role == "Staff")
            .ToListAsync();

        var missingStaffIds = staffIds.Except(staffMembers.Select(user => user.UserId)).ToList();
        if (missingStaffIds.Count > 0)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = $"The following staff members do not exist or are not Staff: {string.Join(", ", missingStaffIds)}.",
                Data = null
            });
        }

        var campaign = new Campaign
        {
            Title = request.Title.Trim(),
            Location = request.Location.Trim(),
            EventDate = request.EventDate,
            AdminId = adminId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var assignmentRequest in assignmentRequests)
        {
            campaign.CampaignAssignments.Add(new CampaignAssignment
            {
                StaffId = assignmentRequest.StaffId,
                ShiftTime = string.IsNullOrWhiteSpace(assignmentRequest.ShiftTime)
                    ? null
                    : assignmentRequest.ShiftTime.Trim(),
                Staff = staffMembers.Single(staff => staff.UserId == assignmentRequest.StaffId)
            });
        }

        _db.Campaigns.Add(campaign);
        await _db.SaveChangesAsync();

        var response = MapCampaignResponse(campaign);

        return Ok(new WrapperData<ResponseCampaign>
            {
                Message = "Campaign created successfully.",
                Data = response
            });
    }

    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> Update(int id, RequestCreateCampaign request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Title is required.",
                Data = null
            });
        }

        if (string.IsNullOrWhiteSpace(request.Location))
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Location is required.",
                Data = null
            });
        }

        if (request.EventDate == default)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Event date is required.",
                Data = null
            });
        }

        var campaign = await _db.Campaigns
            .Include(c => c.CampaignAssignments)
            .FirstOrDefaultAsync(c => c.CampaignId == id);

        if (campaign == null)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Campaign not found.",
                Data = null
            });
        }

        var assignmentRequests = request.StaffAssignments ?? new List<RequestCampaignAssignment>();
        var staffIds = assignmentRequests.Select(assignment => assignment.StaffId).ToList();

        if (staffIds.Count != staffIds.Distinct().Count())
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "A staff member cannot be assigned more than once to the same campaign.",
                Data = null
            });
        }

        var staffMembers = await _db.Users
            .Where(user => staffIds.Contains(user.UserId) && user.Role == "Staff")
            .ToListAsync();

        var missingStaffIds = staffIds.Except(staffMembers.Select(user => user.UserId)).ToList();
        if (missingStaffIds.Count > 0)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = $"The following staff members do not exist or are not Staff: {string.Join(", ", missingStaffIds)}.",
                Data = null
            });
        }

        campaign.Title = request.Title.Trim();
        campaign.Location = request.Location.Trim();
        campaign.EventDate = request.EventDate;

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var existingAssignments = campaign.CampaignAssignments.ToList();
        _db.CampaignAssignments.RemoveRange(existingAssignments);
        campaign.CampaignAssignments.Clear();

        await _db.SaveChangesAsync();

        foreach (var assignmentRequest in assignmentRequests)
        {
            campaign.CampaignAssignments.Add(new CampaignAssignment
            {
                CampaignId = campaign.CampaignId,
                StaffId = assignmentRequest.StaffId,
                ShiftTime = string.IsNullOrWhiteSpace(assignmentRequest.ShiftTime)
                    ? null
                    : assignmentRequest.ShiftTime.Trim(),
                Staff = staffMembers.Single(staff => staff.UserId == assignmentRequest.StaffId)
            });
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new WrapperData<ResponseCampaign>
        {
            Message = "Campaign updated successfully.",
            Data = MapCampaignResponse(campaign)
        });
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var campaign = await _db.Campaigns
            .Include(c => c.CampaignAssignments)
            .FirstOrDefaultAsync(c => c.CampaignId == id);

        if (campaign == null)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Campaign not found.",
                Data = null
            });
        }

        var hasRegistrations = await _db.Registrations
            .AnyAsync(registration => registration.CampaignId == id);

        if (hasRegistrations)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Campaign cannot be deleted because volunteers have registered for it.",
                Data = null
            });
        }

        var hasHealthScreenings = await _db.HealthScreenings
            .AnyAsync(screening => screening.CampaignId == id);

        if (hasHealthScreenings)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Campaign cannot be deleted because it has health screening data.",
                Data = null
            });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        _db.CampaignAssignments.RemoveRange(campaign.CampaignAssignments);
        _db.Campaigns.Remove(campaign);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new WrapperData<string>
        {
            Message = "Campaign deleted successfully.",
            Data = null
        });
    }

    [HttpGet("detail/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var campaign = await _db.Campaigns
            .Include(c => c.CampaignAssignments)
                .ThenInclude(assignment => assignment.Staff)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CampaignId == id);

        if (campaign == null)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "Campaign not found.",
                Data = null
            });
        }

        var response = MapCampaignResponse(campaign);

        return Ok(new WrapperData<ResponseCampaign>
        {
            Message = "Campaign retrieved successfully.",
            Data = response
        });
    }

    private static ResponseCampaign MapCampaignResponse(Campaign campaign)
    {
        return new ResponseCampaign
        {
            CampaignId = campaign.CampaignId,
            Title = campaign.Title,
            Location = campaign.Location,
            EventDate = campaign.EventDate,
            AdminId = campaign.AdminId,
            CreatedAt = campaign.CreatedAt,
            StaffCount = campaign.CampaignAssignments.Count,
            StaffAssignments = campaign.CampaignAssignments.Select(assignment => new ResponseCampaignAssignment
            {
                StaffId = assignment.StaffId,
                StaffName = $"{assignment.Staff.FirstName} {assignment.Staff.LastName}".Trim(),
                Role = assignment.Staff.Role,
                FirstName = assignment.Staff.FirstName,
                LastName = assignment.Staff.LastName,
                Email = assignment.Staff.Email,
                ShiftTime = assignment.ShiftTime
            }).ToList()
        };
    }
}
