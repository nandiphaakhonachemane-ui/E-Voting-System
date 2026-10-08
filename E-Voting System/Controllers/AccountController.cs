using E_Voting_System.Models;
using E_Voting_System.Services;
using E_Voting_System.ViewModels;
using Microsoft.AspNetCore.Authorization;          // ← add this
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Voting_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Validate SA ID
            if (!SouthAfricanIdValidator.IsValid(model.SouthAfricanId))
            {
                ModelState.AddModelError("SouthAfricanId", "Invalid South African ID number.");
                return View(model);
            }

            int age = SouthAfricanIdValidator.ExtractAge(model.SouthAfricanId);
            if (age < 18)
            {
                ModelState.AddModelError("SouthAfricanId", "You must be 18 years or older to register.");
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            var idAlreadyUsed = await _userManager.Users
                .AnyAsync(u => u.SouthAfricanId == model.SouthAfricanId);

            if (idAlreadyUsed)
            {
                ModelState.AddModelError("SouthAfricanId", "This South African ID is already registered.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                SouthAfricanId = model.SouthAfricanId,
                FullName = model.FullName,
                DateOfBirth = SouthAfricanIdValidator.ExtractDateOfBirth(model.SouthAfricanId) ?? DateTime.Now.AddYears(-20),
                Address = model.Address,
                Ward = model.Ward,
                IsEligibleVoter = false, // Admin must approve
                RegisteredAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Voter");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ===================== CHANGE EMAIL =====================
        [HttpGet]
        [Authorize]
        public IActionResult ChangeEmail()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(string newEmail, string currentPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            if (string.IsNullOrWhiteSpace(newEmail) || string.IsNullOrWhiteSpace(currentPassword))
            {
                ModelState.AddModelError(string.Empty, "Enter the new email and your current password.");
                return View();
            }

            var passwordOk = await _userManager.CheckPasswordAsync(user, currentPassword);
            if (!passwordOk)
            {
                ModelState.AddModelError(string.Empty, "Current password is incorrect.");
                return View();
            }

            var existing = await _userManager.FindByEmailAsync(newEmail);
            if (existing != null && existing.Id != user.Id)
            {
                ModelState.AddModelError(string.Empty, "That email is already registered.");
                return View();
            }

            var emailResult = await _userManager.SetEmailAsync(user, newEmail);
            if (!emailResult.Succeeded)
            {
                foreach (var error in emailResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View();
            }

            var nameResult = await _userManager.SetUserNameAsync(user, newEmail);
            if (!nameResult.Succeeded)
            {
                foreach (var error in nameResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View();
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Email updated successfully. Please log in with the new email next time.";
            return RedirectToAction("Index", "Home");
        }
        // ========================================================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}