using InternshipPortal.Domain.Entities;

namespace InternshipPortal.Application.Interfaces;

public interface IInternshipOpeningRepository
{
    Task<IReadOnlyList<InternshipOpening>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InternshipOpening?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InternshipOpening> AddAsync(InternshipOpening internshipOpening, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(InternshipOpening internshipOpening, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}