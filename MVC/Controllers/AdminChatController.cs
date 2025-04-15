using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mvc.Models;
using System.Linq;

namespace mvc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminChatController : Controller
    {
        private readonly ProjectContext _context;
        
        public AdminChatController(ProjectContext context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult GetUnreadCount()
        {
           
            
           
            int unreadCount = 3;
            
            return Json(unreadCount);
        }
        
        [HttpGet]
        public IActionResult GetConversations()
        {
          
            
            return Json(new object[0]);
        }
        
        [HttpGet]
        public IActionResult GetMessages(int conversationId)
        {
          
            
            return Json(new object[0]);
        }
        
        [HttpPost]
        public IActionResult SendMessage(int conversationId, string message)
        {
       
            
            return Json(new { success = true });
        }
        
        [HttpPost]
        public IActionResult MarkAsRead(int conversationId)
        {
      
            
            return Json(new { success = true });
        }
        
        [HttpPost]
        public IActionResult ArchiveConversation(int conversationId)
        {
     
            
            return Json(new { success = true });
        }
        
        [HttpPost]
        public IActionResult DeleteConversation(int conversationId)
        {
        
            
            return Json(new { success = true });
        }
    }
}
