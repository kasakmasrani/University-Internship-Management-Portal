using InternshipPortal.Application.Interfaces;
using InternshipPortal.Domain.Entities;

namespace InternshipPortal.Infrastructure.Repositories;

public class InMemoryInternshipOpeningRepository : IInternshipOpeningRepository
{
    private readonly List<InternshipOpening> _internshipOpenings;
    private int _nextId;

    public InMemoryInternshipOpeningRepository()
    {
        _internshipOpenings = new List<InternshipOpening>
        {
            new()
            {
                Id = 1,
                Title = "Backend Developer Intern",
                Description = "Work on ASP.NET Core APIs and basic database tasks.",
                CompanyName = "CodeSphere",
                Location = "Dhaka",
                Duration = "3 Months",
                Stipend = 8000,
                LastDate = DateTime.UtcNow.Date.AddDays(10),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = 2,
                Title = "Frontend Intern",
                Description = "Support dashboard UI development and bug fixing.",
                CompanyName = "PixelCraft",
                Location = "Remote",
                Duration = "4 Months",
                Stipend = 7000,
                LastDate = DateTime.UtcNow.Date.AddDays(14),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = 3,
                Title = "QA Intern",
                Description = "Assist with test case preparation and regression testing.",
                CompanyName = "NextBridge",
                Location = "Chattogram",
                Duration = "2 Months",
                Stipend = 6000,
                LastDate = DateTime.UtcNow.Date.AddDays(7),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = 4,
                Title = "Data Analyst Intern",
                Description = "Build dashboards, prepare reports, and support data cleaning.",
                CompanyName = "InsightLabs",
                Location = "Dhaka",
                Duration = "3 Months",
                Stipend = 9000,
                LastDate = DateTime.UtcNow.Date.AddDays(12),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new()
            {
                Id = 5,
                Title = "Mobile App Intern",
                Description = "Assist in Flutter app development and testing.",
                CompanyName = "AppNova",
                Location = "Sylhet",
                Duration = "4 Months",
                Stipend = 8500,
                LastDate = DateTime.UtcNow.Date.AddDays(9),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new()
            {
                Id = 6,
                Title = "DevOps Intern",
                Description = "Support CI/CD pipelines, containerization, and monitoring setup.",
                CompanyName = "CloudNest",
                Location = "Remote",
                Duration = "3 Months",
                Stipend = 10000,
                LastDate = DateTime.UtcNow.Date.AddDays(15),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = 7,
                Title = "UI/UX Design Intern",
                Description = "Create wireframes and improve student dashboard experience.",
                CompanyName = "PixelCraft",
                Location = "Dhaka",
                Duration = "2 Months",
                Stipend = 6500,
                LastDate = DateTime.UtcNow.Date.AddDays(6),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                Id = 8,
                Title = "Cybersecurity Intern",
                Description = "Support vulnerability assessments and access policy audits.",
                CompanyName = "SafeNetics",
                Location = "Khulna",
                Duration = "3 Months",
                Stipend = 11000,
                LastDate = DateTime.UtcNow.Date.AddDays(18),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = 9,
                Title = "Business Operations Intern",
                Description = "Assist operations team with process documentation and KPI tracking.",
                CompanyName = "PrimeLogix",
                Location = "Rajshahi",
                Duration = "3 Months",
                Stipend = 5500,
                LastDate = DateTime.UtcNow.Date.AddDays(-1),
                IsActive = false,
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new()
            {
                Id = 10,
                Title = "AI/ML Intern",
                Description = "Train baseline models and evaluate student recommendation quality.",
                CompanyName = "NeuroMatrix",
                Location = "Dhaka",
                Duration = "6 Months",
                Stipend = 13000,
                LastDate = DateTime.UtcNow.Date.AddDays(20),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = 11,
                Title = "Database Intern",
                Description = "Write optimized SQL queries and support data migration tasks.",
                CompanyName = "DataForge",
                Location = "Narayanganj",
                Duration = "3 Months",
                Stipend = 7800,
                LastDate = DateTime.UtcNow.Date.AddDays(11),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-9)
            },
            new()
            {
                Id = 12,
                Title = "Product Management Intern",
                Description = "Participate in backlog grooming and feature prioritization.",
                CompanyName = "NextBridge",
                Location = "Remote",
                Duration = "3 Months",
                Stipend = 9200,
                LastDate = DateTime.UtcNow.Date.AddDays(5),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        _nextId = _internshipOpenings.Max(item => item.Id) + 1;
    }

    public Task<IReadOnlyList<InternshipOpening>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<InternshipOpening> result = _internshipOpenings
            .Select(Clone)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<InternshipOpening?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var internship = _internshipOpenings.FirstOrDefault(item => item.Id == id);
        return Task.FromResult(internship is null ? null : Clone(internship));
    }

    public Task<InternshipOpening> AddAsync(InternshipOpening internshipOpening, CancellationToken cancellationToken = default)
    {
        internshipOpening.Id = _nextId++;
        _internshipOpenings.Add(Clone(internshipOpening));
        return Task.FromResult(Clone(internshipOpening));
    }

    public Task<bool> UpdateAsync(InternshipOpening internshipOpening, CancellationToken cancellationToken = default)
    {
        var index = _internshipOpenings.FindIndex(item => item.Id == internshipOpening.Id);
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        _internshipOpenings[index] = Clone(internshipOpening);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var removed = _internshipOpenings.RemoveAll(item => item.Id == id) > 0;
        return Task.FromResult(removed);
    }

    private static InternshipOpening Clone(InternshipOpening item)
    {
        return new InternshipOpening
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            CompanyName = item.CompanyName,
            Location = item.Location,
            Duration = item.Duration,
            Stipend = item.Stipend,
            LastDate = item.LastDate,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt
        };
    }
}