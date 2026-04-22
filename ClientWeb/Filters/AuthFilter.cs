using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ClientWeb.Filters
{
    public class AuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            // Cho phép vào trang Login/Logout mà không cần đăng nhập
            if (controller == "Auth") return;

            if (userId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}
