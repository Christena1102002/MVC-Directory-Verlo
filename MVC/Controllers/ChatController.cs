using Microsoft.AspNetCore.Mvc;
using mvc.Models;
using System;

namespace mvc.Controllers
{
    public class ChatController : Controller
    {

        public IActionResult Index(Chat model)
        {

            return View("Index", model);
        }

    }
}
