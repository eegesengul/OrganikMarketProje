using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrganikMarketProje.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganikMarketProje.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminUserController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // Admin Paneli - Ana Yönetim Sayfası
        public IActionResult AdminPanel()
        {
            return View();
        }

        // Kullanıcı Listesi
        public async Task<IActionResult> UserList()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = string.Join(", ", roles)
                });
            }

            return View(model);
        }

        // Kullanıcı Rolünü Değiştirme
        [HttpGet]
        public async Task<IActionResult> ChangeRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            // Role değiştirme sonrası başarılı mesajı
            TempData["SuccessMessage"] = "Kullanıcının rolü başarıyla değiştirildi.";
            return RedirectToAction("UserList");
        }

        // Kullanıcı Silme
        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
            return RedirectToAction("UserList");
        }

        // Kullanıcı Düzenleme Sayfası (GET)
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                UserId = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        // Kullanıcı Düzenleme İşlemi (POST)
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            user.Name = model.Name;
            user.Surname = model.Surname;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;

            // E-posta değiştiğinde kullanıcı adı da e-posta ile güncellenir
            if (user.UserName != model.Email)
            {
                user.UserName = model.Email;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // Kullanıcı başarıyla güncellendikten sonra mesaj gösterilir ve UserList sayfasına yönlendirilir
            TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
            return RedirectToAction("UserList");
        }
    }
}
