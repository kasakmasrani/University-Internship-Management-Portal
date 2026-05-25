using InternshipPortal.Application.DTOs;
using InternshipPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InternshipOpeningsController : ControllerBase
{
    private readonly IInternshipService _internshipService;

    public InternshipOpeningsController(IInternshipService internshipService)
    {
        _internshipService = internshipService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InternshipOpeningDto>>> GetAll(CancellationToken cancellationToken)
    {
        var internships = await _internshipService.GetAllAsync(cancellationToken);
        return Ok(internships);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InternshipOpeningDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var internship = await _internshipService.GetByIdAsync(id, cancellationToken);
        if (internship is null)
        {
            return NotFound(new { message = "Internship not found." });
        }

        return Ok(internship);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Company")]
    public async Task<ActionResult<InternshipOpeningDto>> Create(
        [FromBody] CreateInternshipOpeningRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _internshipService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateInternshipOpeningRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _internshipService.UpdateAsync(id, request, cancellationToken);
            if (!updated)
            {
                return NotFound(new { message = "Internship not found." });
            }

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _internshipService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = "Internship not found." });
        }

        return NoContent();
    }
}