
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuModern.Areas.Identity.Data;
using NeuModern.Models;
using NeuModern.Repository.IRepository;

namespace NeuModern.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =Role.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IUnitOfWork unitOfWork;

        public UserController(ApplicationDbContext context, IUnitOfWork unitOfWork)
        {
            this.context = context;
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(string id)
        {
            try
            {
                ApplicationUser application = unitOfWork.ApplicationUser.Get(u => u.Id == id);
                if (application == null)
                {
                    return NotFound();
                }
                return View(application);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching user details.";
                return RedirectToAction(nameof(Index));
            }
            
        }
        [HttpPost]
        public async Task<IActionResult> Block(string id)
        {
            try
            {
                var user = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return NotFound();
                }

                user.IsBlocked = true;
                await context.SaveChangesAsync();
                TempData["Success"] = "User Blocked successfully";


                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while blocking the user.";
                return RedirectToAction(nameof(Index));
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(string id)
        {
            try
            {
                var user = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return NotFound();
                }

                user.IsBlocked = false;
                await context.SaveChangesAsync();
                TempData["Success"] = "User Unblocked successfully";


                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                TempData["error"] = "An error occurred while unblocking the user.";
                return RedirectToAction(nameof(Index));
            }
           
        }


        #region API calls
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                List<ApplicationUser> UserList = context.ApplicationUsers.ToList();
                return Json(new { data = UserList });

            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "An error occurred while fetching users." });
            }

        }

        #endregion

    }
}
