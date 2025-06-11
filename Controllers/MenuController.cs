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

        //public IActionResult Index(string course, string category)
        //{
        //    var items = _service.FilterMenuItems(course, category);
        //    return View(items);
        //}

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Index(string course, string category)
        {
            //Explicitly include inactive items so that deactivated ones still show
            var filteredItems = _service.FilterMenuItems(course, category, includeInactive: true);

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
            ViewBag.Categories = _service.GetMenuCategories();
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
                ViewBag.Categories = _service.GetMenuCategories();
                return View(item);
            }

            try
            {
                _service.AddMenuItem(item); // Save to DB
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
            {
                return NotFound();
            }

            ViewBag.Categories = _service.GetMenuCategories();
            return View(item);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Edit(MenuItem item)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        //  Ensure nested Category is reconstructed
        //        if (item.Category == null)
        //        {
        //            item.Category = new MenuCategory();
        //        }

        //        //  Get CategoryID from form (it may not bind into item.Category)
        //        if (int.TryParse(Request.Form["Category.CategoryID"], out int catId))
        //        {
        //            item.Category.CategoryID = catId;
        //        }

        //        _service.UpdateMenuItem(item); // this saves to DB
        //        return RedirectToAction("Index");
        //    }

        //    // Re-populate dropdown if model state is invalid
        //    ViewBag.Categories = _service.GetMenuCategories();
        //    return View(item);
        //}

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

                    _service.UpdateMenuItem(item); // Update in DB
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Show error message in UI
                    ModelState.AddModelError("", "Update failed: " + ex.Message);
                }
            }

            // Repopulate dropdowns if ModelState is invalid
            ViewBag.Categories = _service.GetMenuCategories();
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
                var item = _service.GetMenuItemById(id);
                if (item == null)
                {
                    TempData["ErrorMessage"] = "Menu item not found.";
                    return RedirectToAction("Index");
                }

                bool newStatus = !item.IsActive;
                _service.ToggleMenuItemStatus(id, newStatus);
                TempData["SuccessMessage"] = $"Menu item {(newStatus ? "activated" : "deactivated")} successfully!";
                return RedirectToAction("Index"); // ✅ ADDED missing return
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

        public IActionResult Deactivate(int id)
        {
            _service.ToggleMenuItemStatus(id, false);
            TempData["SuccessMessage"] = "Menu item deactivated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateStock(int id, int stock)
        {
            _service.UpdateStock(id, stock);
            TempData["SuccessMessage"] = "Stock updated successfully!";
            return RedirectToAction("Index", new { course = Request.Query["course"], category = Request.Query["category"] });
        }

        //[HttpPost]
        //public IActionResult Toggle(int id, bool isActive)
        //{
        //    _service.ToggleMenuItemStatus(id, isActive);
        //    return Ok(); // important for JS .then() to succeed
        //}
    }
}
