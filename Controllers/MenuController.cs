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

        //public IActionResult Index(string course, string category)
        //{
        //    var items = _service.FilterMenuItems(course, category);
        //    return View(items);
        //}

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Index(string course, string category)
        {
            //Explicitly include inactive items so that deactivated ones still show
            var filteredItems = _menuService.FilterMenuItems(course, category, includeInactive: true);

            var drinks = filteredItems.Where(i => i.Course == MenuCourse.Drink).ToList();

            var viewModel = new MenuViewModel(
                categoryName: category ?? "All Categories",
                category: filteredItems,
                drinks: drinks
            );

            return View(viewModel);
        }

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Create()
        {
            ViewBag.Categories = _menuService.GetMenuCategories();
            return View(); //  load Views/Menu/Create.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MenuItem item)
        {
            // Make sure Category object exists
            if (item.Category == null)
                item.Category = new MenuCategory();

            // Manually bind the CategoryID from the form if not bound automatically
            if (int.TryParse(Request.Form["Category.CategoryID"], out int catId))
                item.Category.CategoryID = catId;

            // Validate the model
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _menuService.GetMenuCategories();
                return View(item);
            }

            try
            {
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
            {
                return NotFound();
            }

            ViewBag.Categories = _menuService.GetMenuCategories();
            return View(item);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Edit(MenuItem item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure Category is constructed if null
                    if (item.Category == null)
                        item.Category = new MenuCategory();

                    // Manually bind CategoryID (if not bound automatically)
                    if (int.TryParse(Request.Form["Category.CategoryID"], out int catId))
                        item.Category.CategoryID = catId;
                    else
                        throw new Exception("Category ID was not selected.");

                    _menuService.UpdateMenuItem(item); // Update in DB
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Show error message in UI
                    ModelState.AddModelError("", "Update failed: " + ex.Message);
                }
            }

            // Repopulate dropdowns if ModelState is invalid
            ViewBag.Categories = _menuService.GetMenuCategories();
            return View(item);
        }

        //public IActionResult Toggle(int id, bool isActive)
        //{
        //    _service.ToggleMenuItemStatus(id, isActive);
        //    return RedirectToAction("Index");
        //}

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
                _menuService.ToggleMenuItemStatus(id, newStatus);
                TempData["SuccessMessage"] = newStatus ? "Menu item activated successfully!" : "Menu item deactivated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
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

        public IActionResult Deactivate(int id)
        {
            _menuService.ToggleMenuItemStatus(id, false);
            TempData["SuccessMessage"] = "Menu item deactivated successfully!";
            return RedirectToAction("Index");
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
