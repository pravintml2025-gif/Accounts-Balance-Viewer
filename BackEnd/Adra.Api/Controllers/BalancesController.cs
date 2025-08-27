using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Asp.Versioning;
using Adra.Application.DTOs;
using Adra.Application.Interfaces;
using Adra.Core.Entities;

namespace Adra.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class BalancesController : ControllerBase
{
    private readonly IGetBalancesService _getBalancesService;
    private readonly IProcessBalanceUploadService _processBalanceUploadService;

    public BalancesController(
        IGetBalancesService getBalancesService,
        IProcessBalanceUploadService processBalanceUploadService)
    {
        _getBalancesService = getBalancesService;
        _processBalanceUploadService = processBalanceUploadService;
    }


    [HttpGet("latest")]
    public async Task<ActionResult<List<BalanceDto>>> GetLatest(CancellationToken ct)
    {
        var balances = await _getBalancesService.GetLatestAsync(ct);
        return Ok(balances);
    }

    [HttpGet("by-period")]
    [Authorize(Roles = Roles.Admin)]

    public async Task<ActionResult<List<BalanceDto>>> GetByPeriod(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct)
    {
        if (year < 2000 || year > DateTime.Now.Year + 1)
            return BadRequest(new { message = "Invalid year" });

        if (month < 1 || month > 12)
            return BadRequest(new { message = "Invalid month" });

        var balances = await _getBalancesService.GetByPeriodAsync(year, month, ct);

        return Ok(balances);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<List<BalanceSummaryDto>>> GetSummary(CancellationToken ct)
    {
        var summaries = await _getBalancesService.GetSummaryAsync(ct);
        return Ok(summaries);
    }

    [HttpGet("summary/by-period")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<List<BalanceSummaryDto>>> GetSummaryByPeriod(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct)
    {
        if (year < 2000 || year > DateTime.Now.Year + 1)
            return BadRequest(new { message = "Invalid year" });

        if (month < 1 || month > 12)
            return BadRequest(new { message = "Invalid month" });

        var summaries = await _getBalancesService.GetSummaryByPeriodAsync(year, month, ct);
        return Ok(summaries);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadBalanceResponse>> Upload(
        IFormFile file,
        [FromForm] int year,
        [FromForm] int month,
        CancellationToken ct)
    {
        // Validate parameters
        var currentDate = DateTime.Now;

        if (year < 2000)
            return BadRequest(new { message = "Year cannot be before 2000" });

        if (year > currentDate.Year)
            return BadRequest(new { message = "Cannot upload balance data for future years" });

        // If it's the current year, check the month is not in the future
        if (year == currentDate.Year && month > currentDate.Month)
            return BadRequest(new { message = "Cannot upload balance data for future months" });

        if (month < 1 || month > 12)
            return BadRequest(new { message = "Invalid month" });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File is required" });

        // Get current user ID
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        try
        {
            // Process the upload using the service
            using var fileStream = file.OpenReadStream();
            var result = await _processBalanceUploadService.ExecuteAsync(
                fileStream,
                file.FileName,
                file.Length,
                year,
                month,
                userId,
                ct);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
