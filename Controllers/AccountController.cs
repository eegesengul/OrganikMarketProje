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

        // Profil Görüntüleme ve Düzenleme
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

        // Profil Güncelleme
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MyProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Profil bilgilerini güncelle
            user.Name = model.Name;
            user.Surname = model.Surname;

            // E-posta değişikliği yapılmışsa
            if (user.Email != model.Email)
            {
                user.Email = model.Email;
                user.UserName = model.Email;  // UserName de e-posta ile güncelleniyor
                var emailUpdateResult = await _userManager.UpdateAsync(user);
                if (!emailUpdateResult.Succeeded)
                {
                    foreach (var error in emailUpdateResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    return View(model);
                }

                // E-posta değişikliği sonrası mevcut oturumu sonlandır
                await _signInManager.SignOutAsync();

                // Kullanıcıyı çıkış yapmaya yönlendir
                TempData["ProfileMessage"] = "E-posta başarıyla değiştirildi. Lütfen yeni e-posta ile giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            // Şifre değişikliği yapılacaksa, mevcut şifreyi kontrol et
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("", "Mevcut şifreyi girmeniz gerekmektedir.");
                    return View(model);
                }

                // Şifre onayı yapılacak
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

                // Şifre değişikliği sonrası, kullanıcıyı yeniden oturum açtır
                await _signInManager.SignOutAsync(); // Çıkış işlemi
                TempData["ProfileMessage"] = "Şifreniz başarıyla değiştirildi. Lütfen yeni şifreniz ile giriş yapın.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                // Eğer şifre değişikliği yoksa, sadece kullanıcı bilgilerini güncelle
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    ModelState.AddModelError("", "Profil güncellenirken hata oluştu.");
                    return View(model);
                }
            }

            TempData["ProfileMessage"] = "Profil bilgileri başarıyla güncellendi.";
            return RedirectToAction("MyProfile");
        }

        // Kayıt Olma
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string name, string surname, string phone, string email, string password)
        {
            var user = new AppUser
            {
                UserName = email,
                Email = email,
                PhoneNumber = phone,
                Name = name,
                Surname = surname
            };

            var result = await _userManager.CreateAsync(user, password);

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
            return View();
        }

        // Giriş Yapma
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("AdminPanel", "AdminUser");  // Admin sayfasına yönlendir
                }

                return RedirectToAction("Index", "Home");  // Kullanıcıyı normal anasayfaya yönlendir
            }

            ModelState.AddModelError(string.Empty, "Geçersiz giriş.");
            return View();
        }

        // Çıkış Yapma
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
