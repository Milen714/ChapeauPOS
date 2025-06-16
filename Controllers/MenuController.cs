using ChapeauPOS.Models;
using ChapeauPOS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ChapeauPOS.Commons;
using ChapeauPOS.Models.ViewModels;
using ChapeauPOS.Services;

namespace ChapeauPOS.Controllers
{
    public class MenuController : BaseController
    {
        private readonly IMenuService _service;

        public MenuController(IMenuService service)
        {
            _service = service;
        }

        public IActionResult Index(string course, string category)
        {
            var items = _service.FilterMenuItems(course, category, includeInactive: true);

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
            var items = _service.FilterMenuItems(course, category, includeInactive: true);

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
            ViewBag.Categories = _service.GetMenuCategories();
            return View();
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
                ViewBag.Categories = _service.GetMenuCategories();
                return View(item);
            }

            try
            {
                _service.AddMenuItem(item);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong: " + ex.Message);
                ViewBag.Categories = _service.GetMenuCategories();
                return View(item);
            }
        }

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Edit(int id)
        {
            var item = _service.GetMenuItemById(id);
            if (item == null)
                return NotFound();

            ViewBag.Categories = _service.GetMenuCategories();
            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(MenuItem item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (item.Category == null)
                        item.Category = new MenuCategory();

                    if (int.TryParse(Request.Form["Category.CategoryID"], out int catId))
                        item.Category.CategoryID = catId;
                    else
                        throw new Exception("Category ID was not selected.");

                    _service.UpdateMenuItem(item);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Update failed: " + ex.Message);
                }
            }

            ViewBag.Categories = _service.GetMenuCategories();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(int id)
        {
            try
            {
                var item = _service.GetMenuItemById(id);
                if (item == null)
                {
                    TempData["ErrorMessage"] = "Menu item not found.";
                    return RedirectToAction("Index");
                }

                bool newStatus = !item.IsActive;
                _service.ToggleMenuItemStatus(id, newStatus);
                TempData["SuccessMessage"] = $"Menu item {(newStatus ? "activated" : "deactivated")} successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Toggle(int id, bool isActive)
        {
            _service.ToggleMenuItemStatus(id, isActive);
            TempData["SuccessMessage"] = "Menu item updated successfully!";
            return RedirectToAction("Index");
        }

        //  Secure POST version of Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Deactivate(int id)
        {
            _service.ToggleMenuItemStatus(id, false);
            TempData["SuccessMessage"] = "Menu item deactivated successfully!";
            return RedirectToAction("Manage");
        }

        //  Secure POST version of Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Activate(int id)
        {
            _service.ToggleMenuItemStatus(id, true);
            TempData["SuccessMessage"] = "Menu item activated successfully!";
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public IActionResult UpdateStock(int id, int stock)
        {
            _service.UpdateStock(id, stock);
            TempData["SuccessMessage"] = "Stock updated successfully!";
            return RedirectToAction("Index", new { course = Request.Query["course"], category = Request.Query["category"] });
        }
    }
}
