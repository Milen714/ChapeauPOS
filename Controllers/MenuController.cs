using ChapeauPOS.Models;
using ChapeauPOS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ChapeauPOS.Commons;
using ChapeauPOS.Models.ViewModels;

namespace ChapeauPOS.Controllers
{
    public class MenuController : BaseController
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public IActionResult Index(string course, string category)
        {
            var items = _menuService.FilterMenuItems(course, category, includeInactive: true);

            var viewModel = new MenuViewModel(
                categoryName: category ?? "All Categories",
                category: items,
                drinks: items.Where(i => i.Course == MenuCourse.Drink).ToList()
            );

            ViewBag.LoggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");
            return View(viewModel);
        }

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Manage(string course, string category)
        {
            var items = _menuService.FilterMenuItems(course, category, includeInactive: true);

            var viewModel = new MenuViewModel(
                categoryName: category ?? "All Categories",
                category: items,
                drinks: items.Where(i => i.Course == MenuCourse.Drink).ToList()
            );

            ViewBag.LoggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");
            return View("Manage", viewModel);
        }

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Create()
        {
            ViewBag.Categories = _menuService.GetMenuCategories();
            return View(); // Loads Views/Menu/Create.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MenuItem item)
        {
            if (item.Category == null)
                item.Category = new MenuCategory();

            if (int.TryParse(Request.Form["Category.CategoryID"], out int catId))
                item.Category.CategoryID = catId;

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _menuService.GetMenuCategories();
                return View(item);
            }

            try
            {
                _menuService.AddMenuItem(item);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong: " + ex.Message);
                ViewBag.Categories = _menuService.GetMenuCategories();
                return View(item);
            }
        }

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Edit(int id)
        {
            var item = _menuService.GetMenuItemById(id);
            if (item == null)
                return NotFound();

            ViewBag.Categories = _menuService.GetMenuCategories();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MenuItem item)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _menuService.GetMenuCategories();
                return View(item);
            }

            try
            {
                if (item.Category == null)
                    item.Category = new MenuCategory();

                if (int.TryParse(Request.Form["Category.CategoryID"], out int catId))
                    item.Category.CategoryID = catId;
                else
                    throw new Exception("Category ID was not selected.");

                _menuService.UpdateMenuItem(item);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Update failed: " + ex.Message);
                ViewBag.Categories = _menuService.GetMenuCategories();
                return View(item);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Toggle(int id)
        {
            try
            {
                var item = _menuService.GetMenuItemById(id);
                if (item == null)
                {
                    TempData["ErrorMessage"] = "Menu item not found.";
                    return RedirectToAction(nameof(Index));
                }

                bool newStatus = !item.IsActive;
                _menuService.ToggleMenuItemStatus(id, newStatus);

                TempData["SuccessMessage"] = newStatus
                    ? "Menu item activated successfully!"
                    : "Menu item deactivated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Deactivate(int id)
        {
            _menuService.ToggleMenuItemStatus(id, false);
            TempData["SuccessMessage"] = "Menu item deactivated successfully!";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Activate(int id)
        {
            _menuService.ToggleMenuItemStatus(id, true);
            TempData["SuccessMessage"] = "Menu item activated successfully!";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStock(int id, int stock)
        {
            _menuService.UpdateStock(id, stock);
            TempData["SuccessMessage"] = "Stock updated successfully!";
            return RedirectToAction(nameof(Index), new { course = Request.Query["course"], category = Request.Query["category"] });
        }
    }
}
