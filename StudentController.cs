
using gateflow.Data;
using GateFlow.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GateFlow.Controllers
{
    [Authorize(Roles = "Student")]
    [Authorize]
    public class StudentController : Controller
    {
        private readonly GateFlowDbContext db;

        public StudentController(GateFlowDbContext context)
        {
            db = context;
        }
        [Authorize]
        public IActionResult Dashboard()
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var passes = db.GatePasses
                .Where(p => p.UserId == userId);

            var model = new StudentDashboardViewModel
            {
                StudentName = User.Identity?.Name ?? "Student",

                TotalPasses = passes.Count(),

                PendingPasses = passes.Count(
                    p => p.Status == "Pending"),

                ApprovedPasses = passes.Count(
                    p => p.Status == "Approved"),

                RejectedPasses = passes.Count(
                    p => p.Status == "Rejected"),

                RecentPasses = passes
                    .OrderByDescending(p => p.CreatedAt)
                    .ThenByDescending(p => p.PassId)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }

        public IActionResult Profile()
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var student = db.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.UserId == userId);

            if (student == null)
                return NotFound();

            return View(student);
        }
    }
}


