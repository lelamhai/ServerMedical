using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    private readonly IWebHostEnvironment _environment;

    public UserController(MedicalContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
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

    [HttpGet("medical-info")]
    public async Task<IActionResult> GetMedicalInfo()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
        {
            return Unauthorized(new WrapperData<string>
            {
                Message = "The authenticated user is invalid.",
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

        var latestBloodTest = await _db.BloodTestResults
            .AsNoTracking()
            .Where(result => result.Screening.VolunteerId == userId)
            .OrderByDescending(result => result.ResultDate)
            .ThenByDescending(result => result.TestResultId)
            .Select(result => new
            {
                result.BloodType,
                result.DiseaseNotes,
                result.ResultDate
            })
            .FirstOrDefaultAsync();

        var response = new ResponseUserMedicalInfo
        {
            User = user,
            BloodType = latestBloodTest?.BloodType,
            DiseaseNotes = latestBloodTest?.DiseaseNotes,
            ResultDate = latestBloodTest?.ResultDate
        };

        return Ok(new WrapperData<ResponseUserMedicalInfo>
        {
            Message = "User medical information retrieved successfully.",
            Data = response
        });
    }

    [HttpPut("avatar/upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Avatar file is required.",
                Data = null
            });
        }

        const long maxFileSize = 5 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Avatar file must be 5 MB or smaller.",
                Data = null
            });
        }

        var extension = await GetAvatarExtensionAsync(file);
        if (extension == null)
        {
            return BadRequest(new WrapperData<string>
            {
                Message = "Avatar must be a JPEG, PNG, GIF, or WebP image.",
                Data = null
            });
        }

        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
        {
            return Unauthorized(new WrapperData<string>
            {
                Message = "The authenticated user is invalid.",
                Data = null
            });
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            return NotFound(new WrapperData<string>
            {
                Message = "User not found.",
                Data = null
            });
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var uploadPath = Path.Combine(webRootPath, "avatars");
        Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);
        var avatarUrl = $"/avatars/{fileName}";

        await using (var stream = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true))
        {
            await file.CopyToAsync(stream);
        }

        user.Avatar = avatarUrl;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch
            {
                // Preserve the original database exception.
            }

            throw;
        }

        return Ok(new WrapperData<string>
        {
            Message = "Avatar uploaded successfully.",
            Data = avatarUrl
        });
    }

    private static async Task<string?> GetAvatarExtensionAsync(IFormFile file)
    {
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header.AsMemory());

        if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return ".jpg";
        }

        if (bytesRead >= 8 &&
            header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
            header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
        {
            return ".png";
        }

        if (bytesRead >= 6 &&
            header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 &&
            header[3] == 0x38 && (header[4] == 0x37 || header[4] == 0x39) && header[5] == 0x61)
        {
            return ".gif";
        }

        if (bytesRead >= 12 &&
            header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
            header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
        {
            return ".webp";
        }

        return null;
    }
}
