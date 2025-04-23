using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mvc.Models.Authorize;
using mvc.ViewModels;


namespace mvc.Controllers
{
    public class AccountController : Controller
    {
        UserManager<ApplicationUser> _userManger; 
        SignInManager<ApplicationUser> _sinInManger;
        public AccountController(UserManager<ApplicationUser>userManger,SignInManager<ApplicationUser>sinInManger)
        {
            _userManger = userManger;
            _sinInManger=sinInManger;
        }

        public IActionResult Login()
        {

            return View("Login");
        }
        public IActionResult LoginAdmin()
        {

            return View("LoginAdmin");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Login(LoginViewModel userDataReq)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser userfromDb = await _userManger.FindByNameAsync(userDataReq.Name);
                if (userfromDb != null)
                {
                    bool isfound=
                        await _userManger.CheckPasswordAsync(userfromDb, userDataReq.Password);
                    if (isfound)
                    {

                        bool isInRole = await _userManger.IsInRoleAsync(userfromDb, "User");
                        if (!isInRole)
                        {
                            ModelState.AddModelError("", "You are not authorized to login from this page");
                            return View("Login", userDataReq);
                        }

                        await _sinInManger.SignInAsync(userfromDb, userDataReq.RememberMe);
                        Response.Cookies.Append("UserName", userfromDb.UserName);
                        Response.Cookies.Append("UserEmail", userfromDb.Email);
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError("Password", "Incorrect password Or User ❌");

            }
            return View("Login", userDataReq);
        
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> LoginAdmin(LoginViewModel userDataReq)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser userfromDb = await _userManger.FindByNameAsync(userDataReq.Name);
                if (userfromDb != null)
                {
                    bool isfound =
                        await _userManger.CheckPasswordAsync(userfromDb, userDataReq.Password);
                    if (isfound)
                    {


                        bool isInRole = await _userManger.IsInRoleAsync(userfromDb, "Admin");
                        if (!isInRole)
                        {
                            ModelState.AddModelError("", "You are not authorized to login from this page");
                            return View("LoginAdmin", userDataReq);
                        }
                        await _sinInManger.SignInAsync(userfromDb, userDataReq.RememberMe);
                        Response.Cookies.Append("UserName", userfromDb.UserName);
                        Response.Cookies.Append("UserEmail", userfromDb.Email);
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
                ModelState.AddModelError("Password", "Incorrect password Or User ❌");

            }
            return View("LoginAdmin", userDataReq);

        }

        
        public IActionResult Register()
        {
            return View("Register");
        }


        [Authorize(Roles = "Admin")]
        public IActionResult RegisterAdmin()
        {
            return View("RegisterAdmin");
        }
        #region Register_Onclick
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel? UserFromReq)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser userApp=new ApplicationUser();
                userApp.Address=UserFromReq.Address;
                userApp.UserName = UserFromReq.Name;
                userApp.Email = UserFromReq.Email;

              IdentityResult result= await _userManger.CreateAsync(userApp,UserFromReq.Password);
                if (result.Succeeded)
                {


                 // Add role
                    await _userManger.AddToRoleAsync(userApp,"User");

                    await _sinInManger.SignInAsync(userApp, isPersistent: false);
                    
                    return RedirectToAction("Index", "Home");

                }
                foreach (var error in result.Errors)
                {
                   
                        ModelState.AddModelError("", error.Description); // general error
                }
            }

            return View("Register", UserFromReq);
        }

        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAdmin(RegisterViewModel? UserFromReq)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser userApp = new ApplicationUser();
                userApp.Address = UserFromReq.Address;
                userApp.UserName = UserFromReq.Name;
                userApp.Email = UserFromReq.Email;

                IdentityResult result = await _userManger.CreateAsync(userApp, UserFromReq.Password);
                if (result.Succeeded)
                {


                    await _userManger.AddToRoleAsync(userApp, "Admin");

                    await _sinInManger.SignInAsync(userApp, isPersistent: false);

                    return RedirectToAction("Index", "Dashboard");

                }
                foreach (var error in result.Errors)
                {

                    ModelState.AddModelError("", error.Description); 
                }
            }

            return View("RegisterAdmin", UserFromReq);
        }


        #region LogOut
        public async Task<IActionResult> Logout()
        {
           await _sinInManger.SignOutAsync();
            return RedirectToAction("Login");
        }
        #endregion
    }
}
