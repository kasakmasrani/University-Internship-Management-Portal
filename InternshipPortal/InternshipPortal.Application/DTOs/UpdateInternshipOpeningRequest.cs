namespace InternshipPortal.Application.DTOs;

public class UpdateInternshipOpeningRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public decimal Stipend { get; set; }
    public DateTime LastDate { get; set; }
    public bool IsActive { get; set; }
}