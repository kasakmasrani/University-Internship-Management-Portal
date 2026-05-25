using InternshipPortal.Application.DTOs;
using InternshipPortal.Application.Interfaces;
using InternshipPortal.Domain.Entities;

namespace InternshipPortal.Application.Services;

public class InternshipService : IInternshipService
{
    private readonly IInternshipOpeningRepository _repository;

    public InternshipService(IInternshipOpeningRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<InternshipOpeningDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var internships = await _repository.GetAllAsync(cancellationToken);

        return internships
            .OrderByDescending(item => item.IsActive)
            .ThenBy(item => item.LastDate)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<InternshipOpeningDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var internship = await _repository.GetByIdAsync(id, cancellationToken);
        return internship is null ? null : MapToDto(internship);
    }

    public async Task<InternshipOpeningDto> CreateAsync(CreateInternshipOpeningRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Title, request.CompanyName, request.Location, request.Duration, request.LastDate, request.Stipend);

        var internship = new InternshipOpening
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CompanyName = request.CompanyName.Trim(),
            Location = request.Location.Trim(),
            Duration = request.Duration.Trim(),
            Stipend = request.Stipend,
            LastDate = request.LastDate,
            IsActive = request.LastDate.Date >= DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(internship, cancellationToken);
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(int id, UpdateInternshipOpeningRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Title, request.CompanyName, request.Location, request.Duration, request.LastDate, request.Stipend);

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.Title = request.Title.Trim();
        existing.Description = request.Description.Trim();
        existing.CompanyName = request.CompanyName.Trim();
        existing.Location = request.Location.Trim();
        existing.Duration = request.Duration.Trim();
        existing.Stipend = request.Stipend;
        existing.LastDate = request.LastDate;
        existing.IsActive = request.IsActive && request.LastDate.Date >= DateTime.UtcNow.Date;

        return await _repository.UpdateAsync(existing, cancellationToken);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, cancellationToken);
    }

    private static InternshipOpeningDto MapToDto(InternshipOpening internshipOpening)
    {
        return new InternshipOpeningDto
        {
            Id = internshipOpening.Id,
            Title = internshipOpening.Title,
            Description = internshipOpening.Description,
            CompanyName = internshipOpening.CompanyName,
            Location = internshipOpening.Location,
            Duration = internshipOpening.Duration,
            Stipend = internshipOpening.Stipend,
            LastDate = internshipOpening.LastDate,
            IsActive = internshipOpening.IsActive,
            CreatedAt = internshipOpening.CreatedAt
        };
    }

    private static void ValidateRequest(
        string title,
        string companyName,
        string location,
        string duration,
        DateTime lastDate,
        decimal stipend)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new ArgumentException("Company name is required.");
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            throw new ArgumentException("Location is required.");
        }

        if (string.IsNullOrWhiteSpace(duration))
        {
            throw new ArgumentException("Duration is required.");
        }

        if (stipend < 0)
        {
            throw new ArgumentException("Stipend cannot be negative.");
        }

        if (lastDate.Date < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Last date must be today or later.");
        }
    }
}