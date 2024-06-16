using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuModern.Models;
using NeuModern.Repository.IRepository;

namespace NeuStyle.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CustomerCouponController : Controller
    {
        public IUnitOfWork _unitOfWork;

        public CustomerCouponController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [Authorize]
        public IActionResult Index()
        {
            try
            {
                List<Coupon> coupon = _unitOfWork.Coupon.GetAll().ToList();
                return View(coupon);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the coupons.";
                return RedirectToAction("Error", "Home");
            }
            
        }
    }
}
