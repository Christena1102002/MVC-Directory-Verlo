namespace mvc.ViewModels
{
    public class RoleViewModel
    {
        public string? Id { get; set; }
        public string RoleName { get; set; }
        public int UsersCount { get; set; }
    }

    public class RoleFormViewModel
    {
        public string? Id { get; set; }
        public string RoleName { get; set; }
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; } = false;
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsSelected { get; set; }
    }
}
