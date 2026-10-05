using E_Voting_System.Data;
using E_Voting_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Voting_System.Controllers
{
    [Authorize(Roles = "Admin,ElectionOfficer")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ========== PARTIES ==========
        public async Task<IActionResult> Parties()
        {
            var parties = await _context.Parties.OrderBy(p => p.Name).ToListAsync();
            return View(parties);
        }

        [HttpGet]
        public IActionResult CreateParty() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateParty(Party model)
        {
            if (ModelState.IsValid)
            {
                _context.Parties.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Parties));
            }
            return View(model);
        }

        // ========== ELECTIONS ==========
        public async Task<IActionResult> Elections()
        {
            var elections = await _context.Elections.OrderByDescending(e => e.StartDate).ToListAsync();
            return View(elections);
        }

        [HttpGet]
        public IActionResult CreateElection() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateElection(Election model)
        {
            if (ModelState.IsValid)
            {
                _context.Elections.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Elections));
            }
            return View(model);
        }

        // ========== VOTER VERIFICATION ==========
        public async Task<IActionResult> PendingVoters()
        {
            var pending = await _userManager.Users
                .Where(u => !u.IsEligibleVoter)
                .OrderBy(u => u.RegisteredAt)
                .ToListAsync();

            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveVoter(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsEligibleVoter = true;
                await _userManager.UpdateAsync(user);

                // Optional: write audit log
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    Action = "VoterApproved",
                    Details = $"Approved voter {user.SouthAfricanId}",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(PendingVoters));
        }
    }
}
