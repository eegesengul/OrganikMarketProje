using Microsoft.AspNetCore.Authorization;
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

        // ✅ Kayıt Olma (Register GET)
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
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


        // ✅ Giriş Yapma (Login GET)
        [HttpGet]
        public IActionResult Login() => View();

        // ✅ Giriş Yapma (Login POST)
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

        // ✅ Çıkış Yapma
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ✅ Profil Bilgilerini Getirme
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

        // ✅ Profil Bilgilerini Güncelleme
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MyProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

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

            // Şifre güncelleme işlemi
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
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

            // Sadece isim soyisim güncelleme
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError("", "Profil güncellenirken bir hata oluştu.");
                return View(model);
            }

            TempData["ProfileMessage"] = "Profil başarıyla güncellendi.";
            return RedirectToAction("MyProfile");
        }
    }
}
