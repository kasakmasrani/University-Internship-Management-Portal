using InternshipPortal.Application.DTOs;

namespace InternshipPortal.Application.Interfaces;

public interface IInternshipService
{
    Task<IReadOnlyList<InternshipOpeningDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InternshipOpeningDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InternshipOpeningDto> CreateAsync(CreateInternshipOpeningRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpdateInternshipOpeningRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}