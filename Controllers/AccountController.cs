﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrganikMarketProje.Models;
using System.Threading.Tasks;

namespace OrganikMarketProje.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Name = model.Name,
                Surname = model.Surname
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                    return RedirectToAction("AdminPanel", "AdminUser");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Geçersiz giriş bilgileri.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MyProfile(ProfileViewModel model, string FormType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // 🔁 Profil Güncelleme Formu
            if (FormType == "Profile")
            {
                // Şifre alanlarını doğrulamadan çıkar
                ModelState.Remove(nameof(model.CurrentPassword));
                ModelState.Remove(nameof(model.NewPassword));
                ModelState.Remove(nameof(model.ConfirmNewPassword));

                if (!ModelState.IsValid)
                    return View(model);

                user.Name = model.Name;
                user.Surname = model.Surname;

                if (user.Email != model.Email)
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;

                    var emailUpdateResult = await _userManager.UpdateAsync(user);
                    if (!emailUpdateResult.Succeeded)
                    {
                        foreach (var error in emailUpdateResult.Errors)
                            ModelState.AddModelError("", error.Description);

                        return View(model);
                    }

                    await _signInManager.SignOutAsync();
                    TempData["ProfileMessage"] = "E-posta güncellendi. Lütfen yeniden giriş yapın.";
                    return RedirectToAction("Login", "Account");
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    ModelState.AddModelError("", "Profil güncellenirken bir hata oluştu.");
                    return View(model);
                }

                TempData["ProfileMessage"] = "Profil başarıyla güncellendi.";
                return RedirectToAction("MyProfile");
            }

            // 🔁 Şifre Güncelleme Formu
            else if (FormType == "Password")
            {
                // Profil alanlarını doğrulamadan çıkar
                ModelState.Remove(nameof(model.Name));
                ModelState.Remove(nameof(model.Surname));
                ModelState.Remove(nameof(model.Email));

                if (!ModelState.IsValid)
                    return View(model);

                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("", "Mevcut şifreyi girmeniz gerekmektedir.");
                    return View(model);
                }

                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    ModelState.AddModelError("", "Yeni şifreler uyuşmuyor.");
                    return View(model);
                }

                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!passwordChangeResult.Succeeded)
                {
                    foreach (var error in passwordChangeResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    return View(model);
                }

                await _signInManager.SignOutAsync();
                TempData["ProfileMessage"] = "Şifreniz başarıyla değiştirildi. Lütfen giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }
    }
}
