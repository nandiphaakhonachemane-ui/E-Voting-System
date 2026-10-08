using E_Voting_System.Data;
using E_Voting_System.Models;
using E_Voting_System.Services;
using E_Voting_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Voting_System.Controllers
{
    [Authorize(Roles = "Voter")]
    public class VotingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVoteService _voteService;
        private readonly IOtpService _otpService;

        public VotingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IVoteService voteService, IOtpService otpService)
        {
            _context = context;
            _userManager = userManager;
            _voteService = voteService;
            _otpService = otpService;
        }

        // GET: /Voting
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsEligibleVoter)
            {
                return View("NotEligible");
            }

            // ===== OTP CHECK =====
            if (HttpContext.Session.GetString("OtpVerified") != "true")
            {
                return RedirectToAction(nameof(RequestOtp));
            }
            // ====================

            var openElection = await _context.Elections
                .Where(e => e.IsOpen)
                .OrderByDescending(e => e.StartDate)
                .FirstOrDefaultAsync();

            if (openElection == null)
            {
                return View("NoOpenElection");
            }

            // Check if already voted
            if (await _voteService.HasAlreadyVotedAsync(user.Id, openElection.Id))
            {
                return View("AlreadyVoted");
            }

            // Issue / retrieve token
            var token = await _voteService.IssueTokenAsync(user.Id, openElection.Id);
            if (token == null)
            {
                return View("AlreadyVoted");
            }

            var parties = await _context.Parties
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            var vm = new BallotViewModel
            {
                ElectionId = openElection.Id,
                Election = openElection,
                Parties = parties,
                Token = token.Token
            };

            return View(vm);
        }

        // GET: /Voting/Confirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(BallotViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Re-populate parties if validation fails
                model.Parties = await _context.Parties.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Show confirmation screen
            var nationalParty = await _context.Parties.FindAsync(model.SelectedNationalPartyId);
            Party? provincialParty = null;
            if (model.SelectedProvincialPartyId.HasValue)
            {
                provincialParty = await _context.Parties.FindAsync(model.SelectedProvincialPartyId.Value);
            }

            var confirmVm = new ConfirmVoteViewModel
            {
                ElectionId = model.ElectionId,
                Token = model.Token,
                NationalParty = nationalParty!,
                ProvincialParty = provincialParty,
                SelectedNationalPartyId = model.SelectedNationalPartyId,
                SelectedProvincialPartyId = model.SelectedProvincialPartyId
            };

            return View(confirmVm);
        }

        // POST: /Voting/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ConfirmVoteViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _voteService.CastVoteAsync(
                user.Id,
                model.Token,
                model.ElectionId,
                model.SelectedNationalPartyId,
                model.SelectedProvincialPartyId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Success));
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> RequestOtp()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsEligibleVoter)
                return RedirectToAction("NotEligible");

            try
            {
                await _otpService.GenerateAndSendOtpAsync(user.Id, user.Email!);
                ViewBag.SentTo = user.Email;
            }
            catch (Exception ex)
            {
                ViewBag.EmailError = ex.Message;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(string code)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            bool isValid = await _otpService.ValidateOtpAsync(user.Id, code);

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid or expired OTP. Please try again.");
                return View("RequestOtp");
            }

            // OTP is valid → allow access to the real ballot
            // You can store a temporary claim or session flag
            HttpContext.Session.SetString("OtpVerified", "true");

            return RedirectToAction(nameof(Index)); // Now they can vote
        }
        public IActionResult Success()
        {
            return View();
        } 
       
    }
}
