using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuModern.Models;
using NeuModern.Models.ViewModel;
using NeuModern.Repository.IRepository;
using Stripe;
using Coupon = NeuModern.Models.Coupon;

namespace NeuModern.Areas.Admin.Controllers
{
    
    [Area("Admin")]
    public class CouponController : Controller
    {
        public IUnitOfWork _unitOfWork;
        [BindProperty]
        public CouponVM CouponVM { get; set; }
        public CouponController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            try
            {
                List<Coupon> coupon = _unitOfWork.Coupon.GetAll().ToList();

                return View(coupon);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching coupons.";
                return View(new List<Coupon>());
            }
           
        }
        public IActionResult AddCoupon()
        {
            return View();
        }
        [HttpPost]

        public IActionResult AddCoupon(Coupon coupon)
        {
            try
            {
                if (coupon != null)
                {
                    coupon.IsValid = Role.CouponValid;
                    _unitOfWork.Coupon.Add(coupon);
                    _unitOfWork.Save();
                    TempData["success"] = "  Coupon Created SuccessFully";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                
                TempData["error"] = "An error occurred while creating the coupon.";
                return View(coupon);
            }
           
        }
        public IActionResult Edit(int id)
        {
            try
            {
                Coupon coupon = _unitOfWork.Coupon.Get(u => u.Id == id);
                return View(coupon);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching the coupon details.";
                return RedirectToAction(nameof(Index));
            }
           
        }
        [HttpPost]
        public IActionResult Edit(Coupon coupon)
        {
            try
            {
                if (coupon != null)
                {
                    _unitOfWork.Coupon.Update(coupon);
                    _unitOfWork.Save();
                    TempData["success"] = "  Coupon Edited SuccessFully";
                    return RedirectToAction(nameof(Index));
                }
                return View(coupon);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while editing the coupon";
                return View(coupon);
            }
        }
        public IActionResult Delete(int id)
        {
            try
            {
                Coupon coupon = _unitOfWork.Coupon.Get(u => u.Id == id);
                return View(coupon);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching the coupon details.";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        public IActionResult Delete(Coupon coupon)
        {
            try
            {
                if (coupon == null)
                {
                    NotFound();
                }
                _unitOfWork.Coupon.Remove(coupon);
                _unitOfWork.Save();
                TempData["error"] = "  Coupon Deleted SuccessFully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                
                TempData["error"] = "An error occurred while deleting the coupon.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

