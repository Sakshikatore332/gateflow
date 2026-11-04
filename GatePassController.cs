
using gateflow.Data;
using gateflow.Models;
using GateFlow.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GateFlow.Controllers
{
    [Authorize(Roles = "Student")]
    [Authorize]
    public class GatePassController : Controller
    {
        private readonly GateFlowDbContext db;

        public GatePassController(GateFlowDbContext context)
        {
            db = context;
        }

        private int StudentId()
        {
            return int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);
        }

        private bool ValidTime(GatePassFormViewModel model)
        {
            if (!model.OutTime.HasValue ||
                !model.ExpectedReturnTime.HasValue)
                return false;

            DateTime exitDateTime = model.OutDate.Date
                .Add(model.OutTime.Value);

            if (exitDateTime <= DateTime.Now)
            {
                ModelState.AddModelError(
                    "OutDate",
                    "Exit date and time must be in the future");
            }

            if (model.ExpectedReturnTime.Value <= exitDateTime)
            {
                ModelState.AddModelError(
                    "ExpectedReturnTime",
                    "Return time must be after exit time");
            }

            return ModelState.IsValid;
        }

        // CREATE - Display form
        [HttpGet]
        public IActionResult Apply()
        {
            return View(new GatePassFormViewModel());
        }

        // CREATE - Save request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Apply(GatePassFormViewModel model)
        {
            ValidTime(model);

            if (!ModelState.IsValid)
                return View(model);

            var pass = new GatePass
            {
                UserId = StudentId(),
                Reason = model.Reason.Trim(),
                OutDate = DateOnly.FromDateTime(model.OutDate),
                OutTime = TimeOnly.FromTimeSpan(
                    model.OutTime!.Value),
                ExpectedReturnTime =
                    model.ExpectedReturnTime!.Value,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            db.GatePasses.Add(pass);
            db.SaveChanges();

            TempData["Success"] =
                "Gate pass request submitted successfully!";

            return RedirectToAction("MyPasses");
        }

        // READ - List student's requests
        [HttpGet]
        public IActionResult MyPasses(string? status)
        {
            var passes = db.GatePasses
                .AsNoTracking()
                .Where(p => p.UserId == StudentId());

            string[] allowedStatuses =
            {
                "Pending", "Approved",
                "Rejected", "Cancelled"
            };

            if (!string.IsNullOrEmpty(status) &&
                allowedStatuses.Contains(status))
            {
                passes = passes.Where(p => p.Status == status);
            }

            ViewBag.SelectedStatus = status;

            return View(passes
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.PassId)
                .ToList());
        }

        // READ - View details
        [HttpGet]
        public IActionResult Details(int id)
        {
            var pass = db.GatePasses
                .AsNoTracking()
                .FirstOrDefault(p =>
                    p.PassId == id &&
                    p.UserId == StudentId());

            if (pass == null)
                return NotFound();

            return View(pass);
        }

        // UPDATE - Display edit form
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var pass = db.GatePasses
                .FirstOrDefault(p =>
                    p.PassId == id &&
                    p.UserId == StudentId() &&
                    p.Status == "Pending");

            if (pass == null)
                return NotFound();

            var model = new GatePassFormViewModel
            {
                PassId = pass.PassId,
                Reason = pass.Reason,
                OutDate = pass.OutDate.ToDateTime(
                    TimeOnly.MinValue),
                OutTime = pass.OutTime.ToTimeSpan(),
                ExpectedReturnTime = pass.ExpectedReturnTime
            };

            return View(model);
        }

        // UPDATE - Save changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id, GatePassFormViewModel model)
        {
            if (id != model.PassId)
                return BadRequest();

            var pass = db.GatePasses
                .FirstOrDefault(p =>
                    p.PassId == id &&
                    p.UserId == StudentId() &&
                    p.Status == "Pending");

            if (pass == null)
                return NotFound();

            ValidTime(model);

            if (!ModelState.IsValid)
                return View(model);

            pass.Reason = model.Reason.Trim();
            pass.OutDate = DateOnly.FromDateTime(model.OutDate);
            pass.OutTime = TimeOnly.FromTimeSpan(
                model.OutTime!.Value);
            pass.ExpectedReturnTime =
                model.ExpectedReturnTime!.Value;

            db.SaveChanges();

            TempData["Success"] =
                "Gate pass updated successfully!";

            return RedirectToAction("MyPasses");
        }

        // UPDATE - Cancel pending request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var pass = db.GatePasses
                .FirstOrDefault(p =>
                    p.PassId == id &&
                    p.UserId == StudentId() &&
                    p.Status == "Pending");

            if (pass == null)
                return NotFound();

            pass.Status = "Cancelled";
            db.SaveChanges();

            TempData["Success"] =
                "Gate pass cancelled successfully!";

            return RedirectToAction("MyPasses");
        }

        // DELETE - Remove cancelled or rejected pass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var pass = db.GatePasses
                .FirstOrDefault(p =>
                    p.PassId == id &&
                    p.UserId == StudentId() &&
                    (p.Status == "Cancelled" ||
                     p.Status == "Rejected"));

            if (pass == null)
                return NotFound();

            using var transaction = db.Database.BeginTransaction();

            var approval = db.Approvals
                .FirstOrDefault(a => a.PassId == id);

            if (approval != null)
                db.Approvals.Remove(approval);

            db.GatePasses.Remove(pass);
            db.SaveChanges();

            transaction.Commit();

            TempData["Success"] =
                "Gate pass deleted successfully!";

            return RedirectToAction("MyPasses");
        }
    }
}

