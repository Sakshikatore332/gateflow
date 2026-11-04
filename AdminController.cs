using gateflow.Data;
using gateflow.Models;
using GateFlow.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GateFlow.Controllers
{
    [Authorize(Roles = "Admin")]
    [Authorize]
    public class AdminController : Controller
    {
        private readonly GateFlowDbContext db;

        public AdminController(GateFlowDbContext context)
        {
            db = context;
        }

        private int AdminId()
        {
            return int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);
        }

        // 1. ADMIN DASHBOARD
        public IActionResult Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalStudents = db.Users.Count(
                    u => u.Role == "Student"),

                TotalPasses = db.GatePasses.Count(),

                PendingPasses = db.GatePasses.Count(
                    p => p.Status == "Pending"),

                ApprovedPasses = db.GatePasses.Count(
                    p => p.Status == "Approved"),

                RejectedPasses = db.GatePasses.Count(
                    p => p.Status == "Rejected"),

                RecentPasses = db.GatePasses
                    .Include(p => p.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ThenByDescending(p => p.PassId)
                    .Take(5)
                    .AsNoTracking()
                    .ToList()
            };

            return View(model);
        }

        // 2. PENDING REQUESTS
        public IActionResult PendingRequests()
        {
            var passes = db.GatePasses
                .Include(p => p.User)
                .Where(p => p.Status == "Pending")
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToList();

            return View(passes);
        }

        // 3. ALL GATE PASSES
        public IActionResult AllPasses(string? status)
        {
            var passes = db.GatePasses
                .Include(p => p.User)
                .AsNoTracking()
                .AsQueryable();

            string[] allowed =
            {
                "Pending", "Approved",
                "Rejected", "Cancelled"
            };

            if (!string.IsNullOrEmpty(status) &&
                allowed.Contains(status))
            {
                passes = passes.Where(
                    p => p.Status == status);
            }

            ViewBag.SelectedStatus = status;

            return View(passes
                .OrderByDescending(p => p.CreatedAt)
                .ToList());
        }

        // 4. APPROVE REQUEST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            using var transaction =
                db.Database.BeginTransaction();

            var pass = db.GatePasses
                .FromSqlInterpolated(
                    $"SELECT * FROM GatePasses WITH (UPDLOCK, ROWLOCK) WHERE PassId = {id}")
                .SingleOrDefault();

            if (pass == null || pass.Status != "Pending")
                return NotFound();

            pass.Status = "Approved";

            var approval = new Approval
            {
                PassId = pass.PassId,
                AdminId = AdminId(),
                Decision = "Approved",
                Remarks = "Permission granted",
                ApprovalDate = DateTime.Now
            };

            db.Approvals.Add(approval);

            db.Notifications.Add(new Notification
            {
                UserId = pass.UserId,
                Message = "Your gate pass has been approved.",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            db.SaveChanges();
            transaction.Commit();

            TempData["Success"] =
                "Gate pass approved successfully!";

            return RedirectToAction("PendingRequests");
        }

        // 5. REJECT - SHOW FORM
        [HttpGet]
        public IActionResult Review(int id)
        {
            var pass = db.GatePasses
                .AsNoTracking()
                .FirstOrDefault(p =>
                    p.PassId == id &&
                    p.Status == "Pending");

            if (pass == null)
                return NotFound();

            return View(new AdminRejectViewModel
            {
                PassId = pass.PassId
            });
        }

        // 6. REJECT - SAVE DECISION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Review(AdminRejectViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var transaction =
                db.Database.BeginTransaction();

            var pass = db.GatePasses
                .FromSqlInterpolated(
                    $"SELECT * FROM GatePasses WITH (UPDLOCK, ROWLOCK) WHERE PassId = {model.PassId}")
                .SingleOrDefault();

            if (pass == null || pass.Status != "Pending")
                return NotFound();

            pass.Status = "Rejected";

            db.Approvals.Add(new Approval
            {
                PassId = pass.PassId,
                AdminId = AdminId(),
                Decision = "Rejected",
                Remarks = model.Remarks.Trim(),
                ApprovalDate = DateTime.Now
            });

            db.Notifications.Add(new Notification
            {
                UserId = pass.UserId,
                Message = "Your gate pass was rejected: " +
                          model.Remarks.Trim(),
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            db.SaveChanges();
            transaction.Commit();

            TempData["Success"] =
                "Gate pass rejected successfully!";

            return RedirectToAction("PendingRequests");
        }

        // 7. STUDENT LIST + SEARCH
        public IActionResult Students(string? search)
        {
            var students = db.Users
                .Where(u => u.Role == "Student");

            if (!string.IsNullOrWhiteSpace(search))
            {
                students = students.Where(u =>
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search));
            }

            ViewBag.Search = search;

            return View(students
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .ToList());
        }

        // 8. EDIT STUDENT - GET
        [HttpGet]
        public IActionResult EditStudent(int id)
        {
            var student = db.Users.FirstOrDefault(
                u => u.UserId == id &&
                     u.Role == "Student");

            if (student == null)
                return NotFound();

            return View(new AdminStudentEditViewModel
            {
                UserId = student.UserId,
                FullName = student.FullName,
                Email = student.Email,
                Department = student.Department ?? "",
                Phone = student.Phone
            });
        }

        // 9. EDIT STUDENT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStudent(
            int id, AdminStudentEditViewModel model)
        {
            if (id != model.UserId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var student = db.Users.FirstOrDefault(
                u => u.UserId == id &&
                     u.Role == "Student");

            if (student == null)
                return NotFound();

            string email = model.Email.Trim().ToLower();

            bool exists = db.Users.Any(u =>
                u.Email == email &&
                u.UserId != id);

            if (exists)
            {
                ModelState.AddModelError("Email",
                    "Email already exists");
                return View(model);
            }

            student.FullName = model.FullName.Trim();
            student.Email = email;
            student.Department = model.Department;
            student.Phone = model.Phone;

            db.SaveChanges();

            TempData["Success"] =
                "Student updated successfully!";

            return RedirectToAction("Students");
        }

        // 10. DELETE STUDENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStudent(int id)
        {
            var student = db.Users.FirstOrDefault(
                u => u.UserId == id &&
                     u.Role == "Student");

            if (student == null)
                return NotFound();

            bool hasPasses = db.GatePasses.Any(
                p => p.UserId == id);

            bool hasNotifications = db.Notifications.Any(
                n => n.UserId == id);

            if (hasPasses || hasNotifications)
            {
                TempData["Error"] =
                    "Cannot delete student with existing records.";
                return RedirectToAction("Students");
            }

            db.Users.Remove(student);
            db.SaveChanges();

            TempData["Success"] =
                "Student deleted successfully!";

            return RedirectToAction("Students");
        }

        // 11. ADMIN PROFILE
        public IActionResult Profile()
        {
            var admin = db.Users
                .AsNoTracking()
                .FirstOrDefault(u =>
                    u.UserId == AdminId() &&
                    u.Role == "Admin");

            if (admin == null)
                return NotFound();

            return View(admin);
        }
    }
}

