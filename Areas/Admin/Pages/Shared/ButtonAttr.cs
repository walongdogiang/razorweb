namespace EFWeb.Areas.Admin.Pages.Shared
{
    public class ButtonAttr
    {
        public string? PageUrl { get; set; }
        public string? RoleId { get; set; }
        public string? Backext { get; set; } = "Hủy bỏ";
        public string? UserId { get; set; }
        public string? ParrentClass { get; set; } = "mt-3 float-end";
        public string? Title { get; set; } = "Quay lại trang chủ";
        public string? Handler { get; set; }
    }
}
