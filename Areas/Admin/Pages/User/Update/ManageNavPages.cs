
#nullable disable

using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EFWeb.Areas.Admin.Pages.User.Update
{
    public static class ManageNavPages
    {
        public static string Index => "Index";
        public static string Update => "Update";
        public static string ChangePassword => "ChangePassword";
        public static string AddRole => "AddRole";

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        public static string UpdateNavClass(ViewContext viewContext) => PageNavClass(viewContext, Update);

        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

        public static string AddRoleNavClass(ViewContext viewContext) => PageNavClass(viewContext, AddRole);

        public static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
