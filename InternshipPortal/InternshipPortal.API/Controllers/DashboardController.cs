using InternshipPortal.Application.DTOs;
using InternshipPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.API.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private static readonly object ApplicationSync = new();
    private static readonly Dictionary<int, Dictionary<string, string>> InternshipApplications = new();
    private static readonly Dictionary<int, HashSet<string>> DebarredStudents = new();

    static DashboardController()
    {
        SeedApplications(
            1,
            ("student", "Applied"),
            ("student01", "Hired"),
            ("student02", "Rejected"),
            ("student03", "Applied"));

        SeedApplications(
            2,
            ("student", "Rejected"),
            ("student01", "Applied"),
            ("student04", "Applied"),
            ("student05", "Hired"));

        SeedApplications(
            3,
            ("student02", "Applied"),
            ("student03", "Applied"),
            ("student06", "Rejected"),
            ("student07", "Hired"));

        SeedApplications(
            4,
            ("student01", "Applied"),
            ("student04", "Rejected"),
            ("student08", "Applied"),
            ("student09", "Hired"));

        SeedApplications(
            5,
            ("student02", "Applied"),
            ("student05", "Applied"),
            ("student10", "Rejected"));

        SeedApplications(
            6,
            ("student03", "Hired"),
            ("student06", "Applied"),
            ("student11", "Applied"),
            ("student12", "Rejected"));

        SeedApplications(
            7,
            ("student04", "Applied"),
            ("student08", "Applied"),
            ("student10", "Applied"));

        SeedApplications(
            8,
            ("student05", "Rejected"),
            ("student07", "Applied"),
            ("student09", "Applied"),
            ("student11", "Hired"));

        SeedApplications(
            10,
            ("student01", "Applied"),
            ("student06", "Applied"),
            ("student09", "Applied"),
            ("student12", "Applied"));

        DebarredStudents[5] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "student11"
        };

        DebarredStudents[8] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "student10"
        };
    }

    private readonly IInternshipService _internshipService;

    public DashboardController(IInternshipService internshipService)
    {
        _internshipService = internshipService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var internships = await _internshipService.GetAllAsync(cancellationToken);
        ViewBag.TotalCount = internships.Count;
        ViewBag.ActiveCount = internships.Count(item => item.IsActive);
        ViewBag.AverageStipend = internships.Count == 0 ? 0 : internships.Average(item => item.Stipend);

        lock (ApplicationSync)
        {
            var studentStatusByInternship = new Dictionary<int, string>();
            var appliedInternshipIds = new HashSet<int>();
            var debarredInternshipIds = new HashSet<int>();
            var applicantUsernamesByInternship = new Dictionary<int, List<string>>();
            var applicationCountsByInternship = new Dictionary<int, int>();
            var hiredCountsByInternship = new Dictionary<int, int>();
            var rejectedCountsByInternship = new Dictionary<int, int>();
            var companyByInternshipId = internships.ToDictionary(item => item.Id, item => item.CompanyName);
            var titleByInternshipId = internships.ToDictionary(item => item.Id, item => item.Title);

            var username = User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(username) && User.IsInRole("Student"))
            {
                appliedInternshipIds = InternshipApplications
                    .Where(item => item.Value.ContainsKey(username))
                    .Select(item => item.Key)
                    .ToHashSet();

                debarredInternshipIds = DebarredStudents
                    .Where(item => item.Value.Contains(username))
                    .Select(item => item.Key)
                    .ToHashSet();

                foreach (var internshipEntry in InternshipApplications)
                {
                    if (internshipEntry.Value.TryGetValue(username, out var status))
                    {
                        studentStatusByInternship[internshipEntry.Key] = status;
                    }
                }
            }

            foreach (var internshipEntry in InternshipApplications)
            {
                var applicants = internshipEntry.Value.Keys.OrderBy(item => item).ToList();
                applicantUsernamesByInternship[internshipEntry.Key] = applicants;
                applicationCountsByInternship[internshipEntry.Key] = internshipEntry.Value.Count;
                hiredCountsByInternship[internshipEntry.Key] = internshipEntry.Value.Count(item => item.Value == "Hired");
                rejectedCountsByInternship[internshipEntry.Key] = internshipEntry.Value.Count(item => item.Value == "Rejected");
            }

            ViewBag.AppliedInternshipIds = appliedInternshipIds;
            ViewBag.DebarredInternshipIds = debarredInternshipIds;
            ViewBag.StudentStatusByInternship = studentStatusByInternship;
            ViewBag.ApplicantUsernamesByInternship = applicantUsernamesByInternship;
            ViewBag.ApplicationCountsByInternship = applicationCountsByInternship;
            ViewBag.HiredCountsByInternship = hiredCountsByInternship;
            ViewBag.RejectedCountsByInternship = rejectedCountsByInternship;

            var allStatuses = InternshipApplications.SelectMany(item => item.Value.Values).ToList();
            ViewBag.TotalApplications = allStatuses.Count;
            ViewBag.TotalHired = allStatuses.Count(item => item == "Hired");
            ViewBag.TotalRejected = allStatuses.Count(item => item == "Rejected");
            ViewBag.TotalEmployedStudents = InternshipApplications
                .SelectMany(item => item.Value)
                .Where(item => item.Value == "Hired")
                .Select(item => item.Key)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            var applicationsPerStudent = InternshipApplications
                .SelectMany(item => item.Value.Keys.Select(username => username))
                .GroupBy(username => username, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

            var multiCompanyApplicants = applicationsPerStudent
                .Where(item => item.Value >= 2)
                .OrderByDescending(item => item.Value)
                .ThenBy(item => item.Key)
                .ToList();

            var applicationsByCompany = InternshipApplications
                .GroupBy(
                    item => companyByInternshipId.TryGetValue(item.Key, out var companyName) ? companyName : "Unknown",
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Sum(item => item.Value.Count), StringComparer.OrdinalIgnoreCase);

            var topCompanyByApplications = applicationsByCompany
                .OrderByDescending(item => item.Value)
                .ThenBy(item => item.Key)
                .FirstOrDefault();

            var topOpeningsByApplications = applicationCountsByInternship
                .OrderByDescending(item => item.Value)
                .ThenBy(item => item.Key)
                .Take(3)
                .Select(item =>
                {
                    var title = titleByInternshipId.TryGetValue(item.Key, out var internshipTitle)
                        ? internshipTitle
                        : $"Opening {item.Key}";
                    return $"{title} ({item.Value})";
                })
                .ToList();

            var studentsWithoutAnyOffer = applicationsPerStudent
                .Where(item => !InternshipApplications
                    .SelectMany(app => app.Value)
                    .Any(app => string.Equals(app.Key, item.Key, StringComparison.OrdinalIgnoreCase) && app.Value == "Hired"))
                .OrderByDescending(item => item.Value)
                .ThenBy(item => item.Key)
                .Select(item => item.Key)
                .ToList();

            ViewBag.MultiCompanyApplicants = multiCompanyApplicants;
            ViewBag.TopCompanyByApplications = topCompanyByApplications.Key is null
                ? "N/A"
                : $"{topCompanyByApplications.Key} ({topCompanyByApplications.Value})";
            ViewBag.TopOpeningsByApplications = topOpeningsByApplications;
            ViewBag.StudentsWithoutAnyOffer = studentsWithoutAnyOffer;
            ViewBag.TotalUniqueApplicants = applicationsPerStudent.Count;
        }

        return View(internships);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var internship = await _internshipService.GetByIdAsync(id, cancellationToken);
        if (internship is null)
        {
            return NotFound();
        }

        return View(internship);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Company")]
    public IActionResult Create()
    {
        return View(new CreateInternshipOpeningRequest
        {
            LastDate = DateTime.Today.AddDays(7)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> Create(CreateInternshipOpeningRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _internshipService.CreateAsync(request, cancellationToken);
            TempData["StatusMessage"] = "Internship created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(request);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var internship = await _internshipService.GetByIdAsync(id, cancellationToken);
        if (internship is null)
        {
            return NotFound();
        }

        var request = new UpdateInternshipOpeningRequest
        {
            Title = internship.Title,
            Description = internship.Description,
            CompanyName = internship.CompanyName,
            Location = internship.Location,
            Duration = internship.Duration,
            Stipend = internship.Stipend,
            LastDate = internship.LastDate,
            IsActive = internship.IsActive
        };

        ViewBag.InternshipId = id;
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> Edit(int id, UpdateInternshipOpeningRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _internshipService.UpdateAsync(id, request, cancellationToken);
            if (!updated)
            {
                return NotFound();
            }

            TempData["StatusMessage"] = "Internship updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException exception)
        {
            ViewBag.InternshipId = id;
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(request);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _internshipService.DeleteAsync(id, cancellationToken);
        TempData["StatusMessage"] = "Internship removed successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Apply(int id, CancellationToken cancellationToken)
    {
        var internship = await _internshipService.GetByIdAsync(id, cancellationToken);
        if (internship is null)
        {
            return NotFound();
        }

        if (!internship.IsActive)
        {
            TempData["StatusMessage"] = "Application failed: internship is not active.";
            return RedirectToAction(nameof(Index));
        }

        var username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            return Challenge();
        }

        lock (ApplicationSync)
        {
            if (DebarredStudents.TryGetValue(id, out var debarredSet) && debarredSet.Contains(username))
            {
                TempData["StatusMessage"] = "You are debarred from applying to this internship.";
                return RedirectToAction(nameof(Index));
            }

            if (!InternshipApplications.TryGetValue(id, out var applicationSet))
            {
                applicationSet = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                InternshipApplications[id] = applicationSet;
            }

            if (!applicationSet.TryAdd(username, "Applied"))
            {
                TempData["StatusMessage"] = "You have already applied to this internship.";
                return RedirectToAction(nameof(Index));
            }
        }

        TempData["StatusMessage"] = "Application submitted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Faculty")]
    public IActionResult DebarStudent(int id, string studentUsername)
    {
        if (string.IsNullOrWhiteSpace(studentUsername))
        {
            TempData["StatusMessage"] = "Student username is required.";
            return RedirectToAction(nameof(Index));
        }

        lock (ApplicationSync)
        {
            if (!InternshipApplications.TryGetValue(id, out var applicationSet) || !applicationSet.ContainsKey(studentUsername))
            {
                TempData["StatusMessage"] = "Cannot remove student: no application found for this internship.";
                return RedirectToAction(nameof(Index));
            }

            applicationSet.Remove(studentUsername);

            if (!DebarredStudents.TryGetValue(id, out var debarredSet))
            {
                debarredSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                DebarredStudents[id] = debarredSet;
            }

            debarredSet.Add(studentUsername);
        }

        TempData["StatusMessage"] = $"Student '{studentUsername}' removed and debarred for this internship.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Company")]
    public IActionResult HireStudent(int id, string studentUsername)
    {
        if (string.IsNullOrWhiteSpace(studentUsername))
        {
            TempData["StatusMessage"] = "Student username is required.";
            return RedirectToAction(nameof(Index));
        }

        lock (ApplicationSync)
        {
            if (!InternshipApplications.TryGetValue(id, out var applicationSet) || !applicationSet.ContainsKey(studentUsername))
            {
                TempData["StatusMessage"] = "Cannot hire student: no application found for this internship.";
                return RedirectToAction(nameof(Index));
            }

            applicationSet[studentUsername] = "Hired";
        }

        TempData["StatusMessage"] = $"Student '{studentUsername}' marked as Hired.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Company")]
    public IActionResult RejectStudent(int id, string studentUsername)
    {
        if (string.IsNullOrWhiteSpace(studentUsername))
        {
            TempData["StatusMessage"] = "Student username is required.";
            return RedirectToAction(nameof(Index));
        }

        lock (ApplicationSync)
        {
            if (!InternshipApplications.TryGetValue(id, out var applicationSet) || !applicationSet.ContainsKey(studentUsername))
            {
                TempData["StatusMessage"] = "Cannot reject student: no application found for this internship.";
                return RedirectToAction(nameof(Index));
            }

            applicationSet[studentUsername] = "Rejected";
        }

        TempData["StatusMessage"] = $"Student '{studentUsername}' marked as Rejected.";
        return RedirectToAction(nameof(Index));
    }

    private static void SeedApplications(int internshipId, params (string Username, string Status)[] applications)
    {
        var seededApplications = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (username, status) in applications)
        {
            seededApplications[username] = status;
        }

        InternshipApplications[internshipId] = seededApplications;
    }
}