using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuModern.Models;
using NeuModern.Models.ViewModel;
using NeuModern.Repository;
using NeuModern.Repository.IRepository;
using System.Security.Claims;

namespace NeuModern.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class WishListController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public WishListVM WishList { get; set; }
        public WishListController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                var WishlistItems = _unitOfWork.WishList.GetAll(
                   u => u.ApplicationUserId == userId,
                   includeProperties: "Product.ProductImages"
               );

                return View(WishlistItems);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching your wishlist.";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public ActionResult AddToWishlist(int productId)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                var existingWishlistItem = _unitOfWork.WishList.Get(
                    u => u.ApplicationUserId == userId && u.ProductId == productId);

                if (existingWishlistItem == null)
                {
                    var newWishlistItem = new WishList
                    {
                        ApplicationUserId = userId,
                        ProductId = productId
                    };

                    _unitOfWork.WishList.Add(newWishlistItem);
                    _unitOfWork.Save();

                    TempData["success"] = "Product added to Wishlist successfully.";
                }
                else
                {
                    TempData["error"] = "Product is already in the Wishlist.";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while adding the product to the wishlist.";
            }
            return RedirectToAction(nameof(Index));
 
        }

        public ActionResult RemoveFromWishlist(int id)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


                var wishlistItem = _unitOfWork.WishList.Get(u => u.Id == id && u.ApplicationUserId == userId);

                if (wishlistItem != null)
                {
                    _unitOfWork.WishList.Remove(wishlistItem);
                    _unitOfWork.Save();

                    TempData["success"] = "Product removed from Wishlist successfully.";
                }
                else
                {
                    TempData["error"] = "Wishlist item not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while removing the product from the wishlist.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
