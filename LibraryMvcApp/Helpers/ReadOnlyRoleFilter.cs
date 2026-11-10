using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryMvcApp.Helpers
{
    /// <summary>
    /// "Viewer" accounts are read-only: they can browse, search, open and print,
    /// but anything that adds, edits or deletes is refused on the server
    /// (hiding the buttons alone would not stop a hand-crafted request).
    /// Admin always wins, so a user who is both Admin and Viewer is not restricted.
    /// </summary>
    public class ReadOnlyRoleFilter : IAsyncActionFilter
    {
        public const string ViewerRole = "Viewer";

        private static readonly string[] WritePrefixes =
        {
            "Create", "Add", "Edit", "Update", "Delete", "Remove", "Rename",
            "Flag", "Upload", "Import", "Save", "Move", "Reset", "Assign"
        };

        public static bool IsReadOnly(System.Security.Claims.ClaimsPrincipal user) =>
            user.Identity?.IsAuthenticated == true
            && user.IsInRole(ViewerRole)
            && !user.IsInRole("Admin");

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;

            if (IsReadOnly(http.User))
            {
                var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
                var action = context.RouteData.Values["action"]?.ToString() ?? "";

                // login / logout / access-denied must keep working
                bool exempt = controller.Equals("Account", StringComparison.OrdinalIgnoreCase)
                           || controller.Equals("Error", StringComparison.OrdinalIgnoreCase);

                bool safeMethod = HttpMethods.IsGet(http.Request.Method) || HttpMethods.IsHead(http.Request.Method);
                bool writePage = WritePrefixes.Any(p => action.StartsWith(p, StringComparison.OrdinalIgnoreCase));

                if (!exempt && (!safeMethod || writePage))
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                    return;
                }
            }

            await next();
        }
    }
}
