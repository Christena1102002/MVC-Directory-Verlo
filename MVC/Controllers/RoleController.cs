using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mvc.ViewModels;

namespace mvc.Controllers
{
    //[Authorize(Roles ="Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManger;

        public RoleController(RoleManager<IdentityRole>roleManger) {
            this.roleManger = roleManger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task< IActionResult> newRole(RoleViewModel role)
        {
            if (ModelState.IsValid)
            {
                IdentityRole roleModel = new IdentityRole();
                roleModel.Name = role.RoleName;
              IdentityResult result=await  roleManger.CreateAsync(roleModel);
                if (result != null)
                {
                    return Content("succcess");
                }
                else
                {
                    foreach(var item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                }
            }
            return View(role);
        }
    }
}
