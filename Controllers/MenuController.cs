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
        private readonly IMenuService _menuService;

        public MenuController(IMenuService service)
        {
            _menuService = service;
        }

        public IActionResult Index(string course, string category)
        {

            var items = _service.FilterMenuItems(course, category, includeInactive: true);

            //Explicitly include inactive items so that deactivated ones still show
            var filteredItems = _menuService.FilterMenuItems(course, category, includeInactive: true);

            var drinks = filteredItems.Where(i => i.Course == MenuCourse.Drink).ToList();


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

            ViewBag.Categories = _menuService.GetMenuCategories();
            return View(); //  load Views/Menu/Create.cshtml

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

                _service.AddMenuItem(item);

                _menuService.AddMenuItem(item); // Save to DB

                return RedirectToAction("Index");
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

                    _menuService.UpdateMenuItem(item); // Update in DB

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Update failed: " + ex.Message);
                }
            }


            ViewBag.Categories = _service.GetMenuCategories();

            // Repopulate dropdowns if ModelState is invalid
            ViewBag.Categories = _menuService.GetMenuCategories();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(int id)
        {
            try
            {
                var item = _menuService.GetMenuItemById(id);
                if (item == null)
                {
                    TempData["ErrorMessage"] = "Menu item not found.";
                    return RedirectToAction("Index");
                }

                bool newStatus = !item.IsActive;

                _service.ToggleMenuItemStatus(id, newStatus);
                TempData["SuccessMessage"] = $"Menu item {(newStatus ? "activated" : "deactivated")} successfully!";

                _menuService.ToggleMenuItemStatus(id, newStatus);
                TempData["SuccessMessage"] = newStatus ? "Menu item activated successfully!" : "Menu item deactivated successfully!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        //toggle menu item status (active/deactive) securely
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Toggle(int id, bool isActive)
        {
            _service.ToggleMenuItemStatus(id, isActive);
            TempData["SuccessMessage"] = "Menu item updated successfully!";
       TempData["ErrorMessage"] = "Error toggling menu item: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Toggle(int id, bool isActive)
        {
            _menuService.ToggleMenuItemStatus(id, true);
            TempData["SuccessMessage"] = "Menu item activated successfully!";

            return RedirectToAction("Index");
        }

        //  Secure POST version of Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Deactivate(int id)
        {
            _menuService.ToggleMenuItemStatus(id, false);
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
            _menuService.UpdateStock(id, stock);
            TempData["SuccessMessage"] = "Stock updated successfully!";
            return RedirectToAction("Index", new { course = Request.Query["course"], category = Request.Query["category"] });
        }
    }
}
