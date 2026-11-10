using LibraryMvcApp.Models;
using LibraryMvcApp.Services.Interfaces;
using System.Security.Claims;

namespace LibraryMvcApp.Services
{
    public class FolderPermissionService : IFolderPermissionService
    {
        public bool CanRead(Folder folder, ClaimsPrincipal user)
        {
            return true;
        }

        public bool CanManage(Folder folder, ClaimsPrincipal user)
        {
            // read-only accounts never manage anything
            if (LibraryMvcApp.Helpers.ReadOnlyRoleFilter.IsReadOnly(user))
                return false;

            if (user.IsInRole("Admin"))
                return true;

            if (!string.IsNullOrEmpty(folder.AllowedRole) &&
                user.IsInRole(folder.AllowedRole))
                return true;

            return false;
        }
    }

}
