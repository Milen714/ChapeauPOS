using Microsoft.AspNetCore.Mvc;
using ChapeauPOS.Models; // Adjust namespace to match your project
using ChapeauPOS.Commons;
using Microsoft.AspNetCore.Mvc.Filters; // For session extensions

public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        // Retrieve the logged-in employee from session
        var loggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");

        // Pass it to the ViewBag
        ViewBag.LoggedInEmployee = loggedInEmployee;
    }
}
