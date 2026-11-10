using LibraryMvcApp.Models;
using System.Security.Claims;

namespace LibraryMvcApp.Services.Interfaces
{
    public interface IFolderPermissionService
    {
        bool CanRead(Folder folder, ClaimsPrincipal user);
        bool CanManage(Folder folder, ClaimsPrincipal user);
    }
}
