using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NeuModern.Models;
using NeuModern.Models.ViewModel;
using NeuModern.Repository;
using NeuModern.Repository.IRepository;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace NeuModern.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public static bool WalletChecked {  get; set; }
        public static bool CouponChecked { get; set; }
        public static decimal CouponDiscountAmount { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
        }
        public IActionResult Index()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                ShoppingCartVM = new()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                    includeProperties: "Product"),
                    OrderHeader = new()
                };
                IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();
                foreach (var price in ShoppingCartVM.ShoppingCartList)
                {
                    price.Product.ProductImages = productImages.Where(u => u.ProductId == price.ProductId).ToList();
                    price.OfferPrice = GetPrice(price);
                    ShoppingCartVM.OrderHeader.OrderTotal += price.OfferPrice * price.Count;

                }

                return View(ShoppingCartVM);
            }
            catch (Exception ex)
            {
              
                TempData["Error"] = "An error occurred while loading the cart.";
                return RedirectToAction("Error", "Home");
            }
            
        }
       
        public IActionResult Summary()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                var shoppingCartItems = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");
                //var coupons = _unitOfWork.Coupon.GetAll();
                var validShoppingCartItems = new List<ShoppingCart>();
                foreach (var cartItem in shoppingCartItems)
                {
                    if (cartItem.Product.StockQuantity >= cartItem.Count)
                    {
                        validShoppingCartItems.Add(cartItem);
                    }
                }

                var previousAddresses = _unitOfWork.OrderHeader
                   .GetAll(o => o.ApplicationUserId == userId)
                   .Select(o => new SelectListItem
                   {
                       Text = $"{o.Name} , {o.StreetAddress}, {o.City}, {o.State}, {o.PostalCode} , {o.PhoneNumber} ",
                       Value = o.StreetAddress
                   })
                   .Distinct()
                   .ToList();

                ViewBag.PreviousAddresses = previousAddresses;
                var coupon = _unitOfWork.Coupon.GetAll();
                var shoppingCartViewModel = new ShoppingCartVM
                {
                    ShoppingCartList = validShoppingCartItems,
                    OrderHeader = new OrderHeader(),
                    CouponList = coupon.ToList(),

                };


                shoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
                shoppingCartViewModel.OrderHeader.Name = shoppingCartViewModel.OrderHeader.ApplicationUser.Name;
                shoppingCartViewModel.OrderHeader.PhoneNumber = shoppingCartViewModel.OrderHeader.ApplicationUser.PhoneNumber;
                shoppingCartViewModel.OrderHeader.StreetAddress = shoppingCartViewModel.OrderHeader.ApplicationUser.StreetAddress;
                shoppingCartViewModel.OrderHeader.City = shoppingCartViewModel.OrderHeader.ApplicationUser.City;
                shoppingCartViewModel.OrderHeader.State = shoppingCartViewModel.OrderHeader.ApplicationUser.State;
                shoppingCartViewModel.OrderHeader.PostalCode = shoppingCartViewModel.OrderHeader.ApplicationUser.PostalCode;

                foreach (var pric in shoppingCartViewModel.ShoppingCartList)
                {
                    if (pric.Product.StockQuantity > 0)
                    {
                        pric.OfferPrice = GetPrice(pric);
                        shoppingCartViewModel.OrderHeader.OrderTotal += (pric.OfferPrice * pric.Count);
                    }
                }

                return View(shoppingCartViewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the summary.";
                return RedirectToAction("Error", "Home");
            }

           
        }




		[HttpPost]
		[ActionName("setAddress")]
		public IActionResult SummaryPost()
		{
            try
            {
                var UserIdentity = (ClaimsIdentity)User.Identity;
                var UserId = UserIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == UserId, includeProperties: "Product");

                ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
                ShoppingCartVM.OrderHeader.ApplicationUserId = UserId;

                ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);
                foreach (var cartItem in ShoppingCartVM.ShoppingCartList)
                {
                    cartItem.OfferPrice = GetPrice(cartItem);
                    ShoppingCartVM.OrderHeader.OrderTotal += (cartItem.OfferPrice * cartItem.Count);
                }

                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    if (cart.Product != null && cart.Product.StockQuantity > 0)
                    {
                        cart.Product.StockQuantity -= cart.Count;
                        if (cart.Product.StockQuantity < 0)
                        {
                            cart.Product.StockQuantity = 0;
                        }
                    }
                }
                if (ShoppingCartVM.OrderHeader.PaymentMethod == Role.PaymentMethodWallet.ToString())
                {
                    if (!string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.CouponCode))
                    {
                        TempData["error"] = "Coupons are not allowed for wallet payments.";
                        return RedirectToAction(nameof(setAddress));
                    }
                    if (ShoppingCartVM.OrderHeader.OrderTotal > 1000)
                    {
                        TempData["error"] = "Wallet payment is not allowed for orders above Rs 1000.";
                        return RedirectToAction(nameof(setAddress));
                    }

                    if (applicationUser.Wallet >= ShoppingCartVM.OrderHeader.OrderTotal)
                    {
                        //applicationUser.Wallet -= ShoppingCartVM.OrderHeader.OrderTotal;
                        applicationUser.Wallet -= (int?)ShoppingCartVM.OrderHeader.OrderTotal;
                        ShoppingCartVM.OrderHeader.PaymentStatus = Role.PaymentStatusApproved;
                        ShoppingCartVM.OrderHeader.OrderStatus = Role.StatusApproved;
                    }
                    else
                    {
                        TempData["error"] = "Insufficient wallet balance.";
                        return RedirectToAction(nameof(setAddress));
                    }

                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
                    _unitOfWork.Save();

                    foreach (var cart in ShoppingCartVM.ShoppingCartList)
                    {
                        OrderDetail orderDetail = new()
                        {
                            Count = cart.Count,
                            Price = cart.OfferPrice,
                            ProductId = cart.ProductId,
                            OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                        };
                        _unitOfWork.OrderDetail.Add(orderDetail);
                        _unitOfWork.Save();
                    }
                }

                else if (ShoppingCartVM.OrderHeader.PaymentMethod == Role.PaymentMethodCOD.ToString())
                {
                    if (!string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.CouponCode))
                    {
                        TempData["error"] = "Coupons are not allowed for COD payments.";
                        return RedirectToAction(nameof(setAddress));
                    }
                    if (ShoppingCartVM.OrderHeader.OrderTotal > 1000)
                    {
                        TempData["error"] = "COD is not allowed for orders above Rs 1000.";
                        return RedirectToAction(nameof(setAddress));
                    }
                    ShoppingCartVM.OrderHeader.PaymentStatus = Role.PaymentMethodCODPending;
                    ShoppingCartVM.OrderHeader.OrderStatus = Role.StatusApproved;
                    _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
                    _unitOfWork.Save();
                    foreach (var cart in ShoppingCartVM.ShoppingCartList)
                    {
                        OrderDetail orderDetail = new()
                        {
                            Count = cart.Count,
                            Price = cart.OfferPrice,
                            ProductId = cart.ProductId,
                            OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                        };
                        _unitOfWork.OrderDetail.Add(orderDetail);
                        _unitOfWork.Save();
                    }
                }

                else if (ShoppingCartVM.OrderHeader.PaymentMethod == Role.PaymentMethodOnline.ToString())
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = Role.PaymentStatusDelayedPayment;
                    ShoppingCartVM.OrderHeader.OrderStatus = Role.StatusPending;

                    _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
                    _unitOfWork.Save();
                    foreach (var cart in ShoppingCartVM.ShoppingCartList)
                    {
                        OrderDetail orderDetail = new()
                        {
                            Count = cart.Count,
                            Price = cart.OfferPrice,
                            ProductId = cart.ProductId,
                            OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                        };
                        _unitOfWork.OrderDetail.Add(orderDetail);
                        _unitOfWork.Save();
                    }

                    if (applicationUser != null)
                    {
                        decimal totalAmount = (decimal)ShoppingCartVM.OrderHeader.OrderTotal;
                        if (!string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.CouponCode))
                        {
                            var coupon = _unitOfWork.Coupon.Get(u => u.CouponCode == ShoppingCartVM.OrderHeader.CouponCode);
                            totalAmount = CouponCheckOut(ShoppingCartVM.OrderHeader.CouponCode, (int)ShoppingCartVM.OrderHeader.OrderTotal);
                            if (coupon != null)
                            {
                                ShoppingCartVM.OrderHeader.OrderTotal = (decimal)totalAmount;
                                coupon.IsValid = Role.CouponInValid;
                                _unitOfWork.Coupon.Update(coupon);
                                _unitOfWork.Save();
                            }
                        }



                        var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                        var options = new SessionCreateOptions
                        {
                            SuccessUrl = domain + $"Customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                            CancelUrl = domain + "Customer/cart/Index",
                            LineItems = new List<SessionLineItemOptions>(),
                            Mode = "payment",
                        };

                        var sessionLineItem = new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions()
                            {
                                UnitAmount = (long?)(totalAmount * 100),
                                Currency = "INR",
                                ProductData = new SessionLineItemPriceDataProductDataOptions()
                                {
                                    Name = "Neu Modern",
                                }
                            },
                            Quantity = 1,
                        };
                        options.LineItems.Add(sessionLineItem);

                        var service = new SessionService();
                        Session session = service.Create(options);
                        _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                        _unitOfWork.Save();
                        Response.Headers.Add("Location", session.Url);
                        return new StatusCodeResult(303);
                    }
                }

                _unitOfWork.ShoppingCart.ClearCart(UserId);
                _unitOfWork.Save();

                //return RedirectToAction(nameof(OrderConfirmation), new { ShoppingCartVM.OrderHeader.Id });
                return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
            }
            catch (Exception ex)
            {
                
                TempData["error"] = "Something went Wrong. Please try again.";
                return RedirectToAction(nameof(Summary));
            }
        }

        
		public IActionResult setAddress(int? id)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			ShoppingCartVM = new()
			{
				ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == UserId, includeProperties: "Product"),
				OrderHeader = new(),
				ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == UserId)
			};
			if (id == null)
			{
				ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);

				ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
				ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
				ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
				ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
				ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
				ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			}
			else
			{
				ShoppingCartVM.OrderHeader.MultipleAddress = _unitOfWork.MultipleAddress.Get(u => u.Id == id);

				ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.MultipleAddress.Name;
				ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.MultipleAddress.State;
				ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.MultipleAddress.StreetAddress;
				ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.MultipleAddress.PostalCode;
				ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.MultipleAddress.City;
				ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.MultipleAddress.PhoneNumber;
			}


			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.OfferPrice = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.OfferPrice * cart.Count);
			}

			return View(ShoppingCartVM);

		}


		public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader =
                _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentMethod == Role.PaymentMethodOnline)
            {
                if (orderHeader.PaymentStatus == Role.PaymentStatusDelayedPayment)
                {
                    try
                    {
                        var services = new SessionService();
                        Session session = services.Get(orderHeader.SessionId);
                        if (session.PaymentStatus.ToLower() == "paid")
                        {
                            _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                            _unitOfWork.OrderHeader.UpdateStatus(id, Role.StatusApproved, Role.PaymentStatusApproved);
                            _unitOfWork.Save();
                        }
                        HttpContext.Session.Clear();
                    }
                    catch (StripeException ex)
                    {

                        Console.WriteLine($"Stripe exception: {ex.Message}");
                        return StatusCode(500, "Internal server error while processing the payment.");
                    }

                }
            }


            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);


        }



        public async Task<IActionResult> Coupon(string coupon, int? OrderTotal)
        {
            if (string.IsNullOrEmpty(coupon) || OrderTotal == null)
            {
                return BadRequest();
            }
            var couponobj = _unitOfWork.Coupon.Get(u => u.CouponCode == coupon);

            if (couponobj != null)
            {
                if (couponobj.IsValid == Role.CouponInValid)
                {
                    TempData["error"] = "The Coupon is Invalid.";
                    var responce = new
                    {
                        success = false,
                        errorMessage = "The Coupon is Invalid."

                    };
                    return Json(responce);
                }
                else
                {
                    if (couponobj.DiscountAmount < OrderTotal)
                    {
                        decimal discountPrice = (decimal)couponobj.DiscountAmount;
                        decimal cartTotal = (decimal)OrderTotal;
                        if (couponobj.DiscountAmount > 0)
                        {
                            discountPrice = (decimal)(cartTotal - couponobj.DiscountAmount);

                        }
                        else
                        {
                            int discountPositive = (int)Math.Abs((decimal)couponobj.DiscountAmount);
                            discountPrice = (decimal)(cartTotal * (100 - discountPositive) / 100);
                            discountPrice = (decimal)(cartTotal - (cartTotal) * (couponobj.DiscountAmount / 100));
                        }
                        decimal newTotal = (decimal)(OrderTotal - discountPrice);

                        var responce = new
                        {
                            success = true,
                            discountPrice,
                            newTotal
                        };
                        CouponChecked = true;
                        CouponDiscountAmount = (decimal)discountPrice;
                        return Json(responce);
                    }

                    else
                    {
                        TempData["error"] = "Order total is below the minimum purchase amount.";
                        var responce = new
                        {
                            success = false,
                            errorMessage = "Order total is below the minimum purchase amount."

                        };
                        return Json(responce);
                    }
                }

            }
            TempData["error"] = "Coupon not found.";
            var responsed = new
            {
                success = false,
                errorMessage = "Coupon not found"
            };
            return Json(responsed);
        }

        private decimal CouponCheckOut(string couponCode, int orderTotal)
        {
            var couponobj = _unitOfWork.Coupon.Get(u => u.CouponCode == couponCode);
            decimal newTotal = (decimal)orderTotal;
            if (couponobj != null)
            {
                if (couponobj.MinAmount < orderTotal)
                {
                    if (couponobj.DiscountAmount > 0)
                    {
                        newTotal = (decimal)(orderTotal - couponobj.DiscountAmount);
                    }
                    else
                    {
                        newTotal = (decimal)(orderTotal - (orderTotal) * (couponobj.DiscountAmount / 100));
                    }
                }
            }
            return newTotal;
        }



        public IActionResult CheckWallet(int? totalAmount, string? userId)
        {
            var userobj = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            string message = "";

            if (userobj.Wallet > totalAmount)
            {
                if (CouponChecked)
                {
                    totalAmount = (int)CouponDiscountAmount;
                }
                var newwalletAmount = userobj.Wallet - totalAmount;
                message = "You may proceed with Wallet Payment";

                var response = new
                {
                    success = true,
                    newWalletAmount = newwalletAmount,
                    message = message,

                };
                WalletChecked = true;
                return Json(response);

            }
            else
            {
                var response = new
                {
                    success = false,
                    newWalletAmount = userobj.Wallet,
                    message = "Insufficient amount in your wallet"

                };

                return Json(response);
            }

        }
        public IActionResult IsNotCheckWallet(int? totalAmount, string? userId)
        {
            var userobj = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            WalletChecked = false;
            string message = "";
            var response = new
            {
                success = true,
                message = message,

            };

            return Json(response);
        }

       
        public IActionResult Plus(int cartid)
        {
            var cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartid);
            cart.Count += 1;
            _unitOfWork.ShoppingCart.Update(cart);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartid)
        {
            var cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartid);
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
            }
            else
            {
                cart.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cart);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartid)
        {
            var cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartid);

            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }
        private decimal GetPrice(ShoppingCart shopping)
        {
            return shopping.Product.OfferPrice;
        }
        private decimal GetPriceBasedOnQuantity(ShoppingCart shopping)
        {
            return shopping.Product.OfferPrice;
        }

    }
}
