
using gateflow.Data;
using gateflow.Models;
using GateFlow.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GateFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GatePassApiController : ControllerBase
    {
        private readonly GateFlowDbContext _context;

        public GatePassApiController(GateFlowDbContext context)
        {
            _context = context;
        }

        // GET: api/GatePassApi
        [HttpGet]
        public async Task<IActionResult> GetAllGatePasses()
        {
            var passes = await _context.GatePasses
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.PassId,
                    p.UserId,
                    p.Reason,
                    p.OutDate,
                    p.OutTime,
                    p.ExpectedReturnTime,
                    p.Status,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(passes);
        }

        // GET: api/GatePassApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGatePassById(int id)
        {
            var pass = await _context.GatePasses
                .Where(p => p.PassId == id)
                .Select(p => new
                {
                    p.PassId,
                    p.UserId,
                    p.Reason,
                    p.OutDate,
                    p.OutTime,
                    p.ExpectedReturnTime,
                    p.Status,
                    p.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (pass == null)
            {
                return NotFound("Gate pass not found.");
            }

            return Ok(pass);
        }

        // POST: api/GatePassApi
        [HttpPost]
        public async Task<IActionResult> AddGatePass(
            GatePassFormViewModel model,
            [FromQuery] int userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == userId);

            if (!userExists)
            {
                return BadRequest("Invalid UserId. Student does not exist.");
            }

            if (!model.OutTime.HasValue ||
                !model.ExpectedReturnTime.HasValue)
            {
                return BadRequest("Select exit and return time.");
            }

            DateTime exitDateTime = model.OutDate.Date
                .Add(model.OutTime.Value);

            if (exitDateTime <= DateTime.Now)
            {
                return BadRequest(
                    "Exit date and time must be in the future.");
            }

            if (model.ExpectedReturnTime.Value <= exitDateTime)
            {
                return BadRequest(
                    "Return time must be after exit time.");
            }

            var pass = new GatePass
            {
                UserId = userId,
                Reason = model.Reason.Trim(),
                OutDate = DateOnly.FromDateTime(model.OutDate),
                OutTime = TimeOnly.FromTimeSpan(model.OutTime.Value),
                ExpectedReturnTime = model.ExpectedReturnTime.Value,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.GatePasses.Add(pass);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Gate pass added successfully!",
                passId = pass.PassId
            });
        }

        // PUT: api/GatePassApi/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGatePass(
            int id, GatePassFormViewModel model)
        {
            var existingPass = await _context.GatePasses
                .FirstOrDefaultAsync(p =>
                    p.PassId == id &&
                    p.Status == "Pending");

            if (existingPass == null)
            {
                return NotFound("Pending gate pass not found.");
            }

            if (!ModelState.IsValid ||
                !model.OutTime.HasValue ||
                !model.ExpectedReturnTime.HasValue)
            {
                return BadRequest(ModelState);
            }

            DateTime exitDateTime = model.OutDate.Date
                .Add(model.OutTime.Value);

            if (exitDateTime <= DateTime.Now)
            {
                return BadRequest(
                    "Exit date and time must be in the future.");
            }

            if (model.ExpectedReturnTime.Value <= exitDateTime)
            {
                return BadRequest(
                    "Return time must be after exit time.");
            }

            existingPass.Reason = model.Reason.Trim();
            existingPass.OutDate =
                DateOnly.FromDateTime(model.OutDate);
            existingPass.OutTime =
                TimeOnly.FromTimeSpan(model.OutTime.Value);
            existingPass.ExpectedReturnTime =
                model.ExpectedReturnTime.Value;

            await _context.SaveChangesAsync();

            return Ok("Gate pass updated successfully!");
        }

        // DELETE: api/GatePassApi/2
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGatePass(int id)
        {
            var pass = await _context.GatePasses
                .FirstOrDefaultAsync(p =>
                    p.PassId == id &&
                    (p.Status == "Cancelled" ||
                     p.Status == "Rejected"));

            if (pass == null)
            {
                return NotFound(
                    "Cancelled or rejected gate pass not found.");
            }

            bool hasGateLog = await _context.GateLogs
                .AnyAsync(g => g.PassId == id);

            if (hasGateLog)
            {
                return Conflict(
                    "Gate pass has gate movement history and cannot be deleted.");
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            var approval = await _context.Approvals
                .FirstOrDefaultAsync(a => a.PassId == id);

            if (approval != null)
            {
                _context.Approvals.Remove(approval);
            }

            _context.GatePasses.Remove(pass);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("Gate pass deleted successfully!");
        }
    }
}

