using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuModern.Models;
using NeuModern.Repository.IRepository;
using NeuModern.Repository;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace NeuModern.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private static int walletAmount;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            try
            {

                if (HttpContext.User.Identity.IsAuthenticated)
                {

                    if (User.IsInRole("Admin"))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }

                }
                IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
                return View(productList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Index action: {ex.Message}");
                return RedirectToAction("Error");
            }
        }

        public IActionResult Details(int productId)
        {
            try
            {
                ShoppingCart cart = new()
                {
                    Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages"),
                    Count = 1,
                    ProductId = productId
                };

                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the Details page for product ID {ProductId}.", productId);
                TempData["Error"] = "An error occurred while loading the product details.";
                return RedirectToAction("Error");
            }


        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {

            try
            {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                shoppingCart.ApplicationUserId = userId;

                ShoppingCart shoppingfromDB = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId &&
                u.ProductId == shoppingCart.ProductId);


                var product = _unitOfWork.Product.Get(p => p.Id == shoppingCart.ProductId);



                if (shoppingCart.Count <= 0)
                {
                    TempData["Error"] = "Minimum should be one!!!";

                    return RedirectToAction("Details", "Home", new { shoppingCart.ProductId });
                }


                if (shoppingCart.Count > product.StockQuantity)
                {
                    TempData["Error"] = $"Stock over: {product.Name} has insufficient stock.";

                    return RedirectToAction("Details", "Home", new { shoppingCart.ProductId });

                }

                var limit = 5;
                if (shoppingCart.Count > limit)
                {
                    TempData["Error"] = "Limit exceeded: Maximum 5 items allowed in the cart.";

                    return RedirectToAction("Details", "Home", new { shoppingCart.ProductId });
                }

                if (shoppingfromDB != null)
                {
                    shoppingfromDB.Count += shoppingCart.Count;
                    _unitOfWork.ShoppingCart.Update(shoppingfromDB);

                }
                else
                {
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                }

                _unitOfWork.Save();

                TempData["success"] = "Cart Updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the cart.");
                TempData["Error"] = "An error occurred while updating the cart.";
                return RedirectToAction("Error");
            }
        }


        public IActionResult Search(string searchString, int? categoryId)
        {
            try
            {
                IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");


                if (categoryId != null)
                {
                    productList = productList.Where(p => p.CategoryId == categoryId);
                }


                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim().ToLower();


                    productList = productList.Where(p =>
                        p.Name.ToLower().Contains(searchString) ||
                        p.Category.Name.ToLower().Contains(searchString));
                }

                var categories = _unitOfWork.Category.GetAll().ToList();
                ViewBag.Categories = categories;

                return View("Index", productList.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for products.");
                TempData["Error"] = "An error occurred while searching for products.";
                return RedirectToAction("Error");
            }


        }
        [Authorize]
        public IActionResult UserProfile()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                ApplicationUser Userobj = _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);
                if (Userobj != null)
                {
                    return View(Userobj);
                }
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the user profile.");
                TempData["Error"] = "An error occurred while loading the user profile.";
                return RedirectToAction("Error");
            }
        }
        public IActionResult EditUserProfile()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                ApplicationUser Userobj = _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);
                if (Userobj != null)
                {
                    return View(Userobj);
                }
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the edit user profile page.");
                TempData["Error"] = "An error occurred while loading the edit user profile page.";
                return RedirectToAction("Error");
            }

        }
        [HttpPost]
        public IActionResult EditUserProfile(ApplicationUser applicationUser)
        {
            try
            {
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();
                return RedirectToAction(nameof(UserProfile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the user profile.");
                TempData["Error"] = "An error occurred while updating the user profile.";
                return RedirectToAction("Error");
            }



        }

        public IActionResult Wallet()
        {
            try
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var UserId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                var userobj = _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);
                if (userobj.Wallet == null)
                {
                    userobj.Wallet = 0;
                }
                if (userobj.Wallet <= 0)
                {
                    userobj.Wallet = 0;
                }
                return View(userobj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the wallet page.");
                TempData["Error"] = "An error occurred while loading the wallet page.";
                return RedirectToAction("Error");
            }

        }
        [HttpPost]
        public IActionResult Wallet(ApplicationUser applicationUser)
        {

            var claimIdentity = (ClaimsIdentity)User.Identity;
            var UserId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            SetWalletValue((int)applicationUser.Wallet);

            var UserObj = _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);
            if (UserObj != null)
            {
                if (UserObj.Wallet == null)
                {
                    UserObj.Wallet = 0;
                }
                if (applicationUser.Wallet != null)
                {
                    if (applicationUser.Wallet <= 0)
                    {
                        TempData["ValueNot"] = "Plz enter the value above 0 ";
                        return View(applicationUser);

                    }
                    else
                    {
                        try
                        {
                            var domain = "https://localhost:7118/";
                            var options = new SessionCreateOptions
                            {

                                SuccessUrl = domain + $"customer/Home/WalletSuccess?id={UserObj.Id}",
                                CancelUrl = domain + "customer/Home/Index",
                                LineItems = new List<SessionLineItemOptions>(),
                                Mode = "payment",
                            };
                            var sessionLineItem = new SessionLineItemOptions
                            {
                                PriceData = new SessionLineItemPriceDataOptions()
                                {
                                    UnitAmount = (long?)(applicationUser.Wallet * 100),
                                    Currency = "INR",

                                    ProductData = new SessionLineItemPriceDataProductDataOptions()
                                    {

                                        Name = "Neu Modern",
                                        Description = "Add amount in your wallet"
                                    }

                                },
                                Quantity = 1,

                            };
                            options.LineItems.Add(sessionLineItem);
                            var service = new SessionService();
                            Session session = service.Create(options);

                            service.Create(options);
                            UserObj.Wallet += applicationUser.Wallet;
                            _unitOfWork.ApplicationUser.Update(UserObj);
                            _unitOfWork.Save();
                            TempData["success"] = "Add Amount in Wallet";
                            Response.Headers.Add("Location", session.Url);
                            return new StatusCodeResult(303);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                    }

                    return RedirectToAction(nameof(UserProfile));
                }
            }
            TempData["error"] = "Not add Amount in Wallet";
            return View(applicationUser);

        }

        public IActionResult WalletSuccess(string id)
        {
            ApplicationUser userObj = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            try
            {
                ViewBag.WalletAmount = walletAmount;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return View(userObj);
        }
        private void SetWalletValue(int amount)
        {
            walletAmount = amount;
        }
        private int GetWalletAmount()
        {
            return walletAmount;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
