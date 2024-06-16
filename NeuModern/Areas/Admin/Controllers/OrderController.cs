using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuModern.Models;
using NeuModern.Models.ViewModel;
using NeuModern.Repository.IRepository;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace NeuModern.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private string _stripeWebhookSecret;

        [BindProperty]
        public OrderVM orderVM { get; set; }

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderid)
        {
            try
            {
                orderVM = new()
                {
                    OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderid, includeProperties: "ApplicationUser"),
                    OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderid, includeProperties: "Product"),
                };
                return View(orderVM);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching order details.";
                return RedirectToAction(nameof(Index));
            }
            
        }

        [HttpPost]
        [Authorize]
        public IActionResult UpdateOrderDetail()
        {
            try
            {
                var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

                orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
                orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
                orderHeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
                orderHeaderFromDb.City = orderVM.OrderHeader.City;
                orderHeaderFromDb.State = orderVM.OrderHeader.State;
                orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;

                _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
                _unitOfWork.Save();

                TempData["Success"] = "Order Details Updated successfully";
                return RedirectToAction(nameof(Details), new { orderid = orderHeaderFromDb.Id });
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while updating order details.";
                return RedirectToAction(nameof(Details), new { orderid = orderVM.OrderHeader.Id });
            }
        }

        [HttpPost]
        [Authorize(Roles = Role.Role_Admin)]
        public IActionResult StartProcessing()
        {
            try
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, Role.StatusInProcess);
                _unitOfWork.Save();
                TempData["success"] = "Order details Updated Successfully";
                return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while updating order status.";
                return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
            }
        }

        [HttpPost]
        [Authorize(Roles = Role.Role_Admin)]
        public IActionResult ShipOrder()
        {
            try
            {
                var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
                orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
                orderHeader.Carrier = orderVM.OrderHeader.Carrier;
                orderHeader.OrderStatus = Role.StatusShipped;
                orderHeader.ShippingDate = DateTime.Now;

                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();

                _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, Role.StatusShipped);
                _unitOfWork.Save();
                TempData["success"] = "Order Shipped Successfully";
                return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while shipping the order.";
                return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
            }
        }
       
        [HttpPost]
        public IActionResult CancelOrder()
        {
            try
            {
                var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

                if (orderHeader == null || orderHeader.OrderStatus == Role.StatusCancelled)
                {
                    TempData["error"] = "Order is already cancelled or does not exist.";
                    return RedirectToAction(nameof(Index));
                }


                if (orderHeader.PaymentMethod == Role.PaymentMethodWallet.ToString() && orderHeader.PaymentStatus == Role.PaymentStatusApproved)
                {
                    var user = _unitOfWork.ApplicationUser.Get(u => u.Id == orderHeader.ApplicationUserId);
                    if (user != null)
                    {
                        user.Wallet += (int)orderHeader.OrderTotal;
                        _unitOfWork.ApplicationUser.Update(user);
                    }
                }
                else if (orderHeader.PaymentMethod == Role.PaymentMethodOnline.ToString() && orderHeader.PaymentStatus == Role.PaymentStatusApproved)
                {
                    var refundResult = RefundOnlinePayment(orderHeader);
                    if (!refundResult)
                    {
                        TempData["error"] = "Failed to process the refund for the online payment.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, Role.StatusCancelled, Role.PaymentStatusRefunded);
                _unitOfWork.Save();

                var orderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderHeader.Id, includeProperties: "Product");
                foreach (var orderDetail in orderDetails)
                {
                    var product = orderDetail.Product;
                    if (product != null)
                    {
                        product.StockQuantity += orderDetail.Count;
                        _unitOfWork.Product.Update(product);
                    }
                }
                _unitOfWork.Save();

                TempData["success"] = "Order Cancelled successfully and the amount has been refunded.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while cancelling the order.";
                return RedirectToAction(nameof(Index));
            }
        }
        private bool RefundOnlinePayment(OrderHeader orderHeader)
        {
            try
            {
                var service = new RefundService();
                var options = new RefundCreateOptions
                {
                    PaymentIntent = orderHeader.PaymentIntentId,
                    Amount = (long?)(orderHeader.OrderTotal * 100),
                };
                Refund refund = service.Create(options);
                return refund.Status == "succeeded";
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Refund exception: {ex.Message}");
                return false;
            }
        }

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            try
            {
                IEnumerable<OrderHeader> orderHeaders;
                if (User.IsInRole(Role.Role_Admin))
                {
                    orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
                }
                else
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                    orderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
                }
                switch (status)
                {
                    case "Processing":
                        orderHeaders = orderHeaders.Where(u => u.OrderStatus == Role.StatusInProcess);
                        break;
                    case "Pending":
                        orderHeaders = orderHeaders.Where(u => u.OrderStatus == Role.PaymentStatusPending);
                        break;
                    case "Shipped":
                        orderHeaders = orderHeaders.Where(u => u.OrderStatus == Role.StatusShipped);
                        break;
                    case "Approved":
                        orderHeaders = orderHeaders.Where(u => u.OrderStatus == Role.StatusApproved);
                        break;
                    case "Cancelled":
                        orderHeaders = orderHeaders.Where(u => u.OrderStatus == Role.StatusCancelled);
                        break;
                    default:
                        break;
                }

                return Json(new { data = orderHeaders });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while fetching orders." });
            }
        }

        public IActionResult Invoice(int orderId)
        {
            try
            {
                orderVM = new()
                {
                    OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                    OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
                };

                return View(orderVM);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching the invoice.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public IActionResult ContinueWithOnlinePayment(int orderId)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            if (orderHeader != null && orderHeader.PaymentStatus == Role.PaymentStatusDelayedPayment)
            {
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/cart/OrderConfirmation?id={orderVM.OrderHeader.Id}",
                    CancelUrl = domain + "Customer/cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = (long?)(orderHeader.OrderTotal * 100),
                        Currency = "INR",
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = "Neu Modern",
                        }
                    },
                    Quantity = 1,
                };
                options.LineItems.Add(sessionLineItem);

                try
                {
                    var service = new SessionService();
                    Session session = service.Create(options);
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                    Response.Headers.Add("Location", session.Url);
                    return new StatusCodeResult(303);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    TempData["error"] = "An error occurred while processing your payment. Please try again.";
                    return RedirectToAction(nameof(PaymentConfirmation), new { id = orderVM.OrderHeader.Id });
                }
            }

            TempData["error"] = "Invalid order or payment status.";
            return RedirectToAction(nameof(PaymentConfirmation), new { id = orderVM.OrderHeader.Id });
        }
        public IActionResult PaymentConfirmation(int id)
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

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }

    }
}
