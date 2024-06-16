using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuModern.Models;
using NeuModern.Models.ViewModel;
using NeuModern.Repository;
using NeuModern.Repository.IRepository;

namespace NeuModern.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =Role.Role_Admin)]
    public class OfferController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OfferVM offerVM { get; set; }
        public OfferController (IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            try
            {
                List<Offer> offers = _unitOfWork.Offer.GetAll().ToList();
                return View(offers);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching offers.";
                return RedirectToAction("Error", "Home");
            }
           
          
        }
        public IActionResult Create()
        {
            try
            {
                var offerVM = new OfferVM
                {
                    Offer = new Offer(),

                    Categories = _unitOfWork.Category.GetAll(),
                    Products = _unitOfWork.Product.GetAll()


                };
                return View(offerVM);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while preparing the create offer view.";
                return RedirectToAction("Error", "Home");
            }

        }
        [HttpPost]
       
        public IActionResult Create(OfferVM offerVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.Offer.Add(offerVM.Offer);
                    _unitOfWork.Save();
                    return RedirectToAction(nameof(Index));
                }
                offerVM.Categories = _unitOfWork.Category.GetAll();
                offerVM.Products = _unitOfWork.Product.GetAll();
                return View(offerVM);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while creating the offer.";
                offerVM.Categories = _unitOfWork.Category.GetAll();
                offerVM.Products = _unitOfWork.Product.GetAll();
                return View(offerVM);
            }
        }
        public IActionResult Edit(int id)
        {
            try
            {
                var offer = _unitOfWork.Offer.Get(o => o.Id == id);
                if (offer == null)
                {
                    return NotFound();
                }
                offerVM = new OfferVM
                {
                    Offer = offer,
                    Categories = _unitOfWork.Category.GetAll(),
                    Products = _unitOfWork.Product.GetAll()
                };
                return View(offerVM);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while preparing the edit offer view.";
                return RedirectToAction("Error", "Home");
            }
           
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(OfferVM offerVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.Offer.Update(offerVM.Offer);
                    _unitOfWork.Save();
                    return RedirectToAction(nameof(Index));
                }
                offerVM.Categories = _unitOfWork.Category.GetAll();
                offerVM.Products = _unitOfWork.Product.GetAll();
                return View(offerVM);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while updating the offer.";
                offerVM.Categories = _unitOfWork.Category.GetAll();
                offerVM.Products = _unitOfWork.Product.GetAll();
                return View(offerVM);
            }
        }
        public IActionResult Delete(int id)
        {
            try
            {
                var offer = _unitOfWork.Offer.Get(o => o.Id == id);
                if (offer == null)
                {
                    return NotFound();
                }
                return View(offer);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while preparing the delete offer view.";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost, ActionName("Delete")]
        
        public IActionResult DeletePost(int id)
        {
            try
            {
                var offer = _unitOfWork.Offer.Get(o => o.Id == id);
                if (offer == null)
                {
                    return NotFound();
                }
                _unitOfWork.Offer.Remove(offer);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while deleting the offer.";
                return RedirectToAction("Error", "Home");
            }
            
        }
        private void UpdateDiscountedPrices()
        {
            try
            {
                var products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

                foreach (var product in products)
                {
                    var categoryOffer = _unitOfWork.Offer.GetAll()
                        .Where(o => o.CategoryId == product.CategoryId && o.Offertype == Offer.OfferType.Category)
                        .OrderByDescending(o => o.OfferDiscount)
                        .FirstOrDefault();

                    var productOffer = _unitOfWork.Offer.GetAll()
                        .Where(o => o.ProductId == product.Id && o.Offertype == Offer.OfferType.Product)
                        .OrderByDescending(o => o.OfferDiscount)
                        .FirstOrDefault();

                    decimal maxDiscount = 0;

                    if (categoryOffer != null)
                    {
                        maxDiscount = categoryOffer.OfferDiscount;
                    }

                    if (productOffer != null && productOffer.OfferDiscount > maxDiscount)
                    {
                        maxDiscount = productOffer.OfferDiscount;
                    }

                    if (maxDiscount > 0)
                    {
                        product.Discount = Math.Round(product.OfferPrice * (1 - maxDiscount / 100), 2);
                    }

                    else
                    {
                        product.Discount = product.OfferPrice;
                    }

                    _unitOfWork.Product.Update(product);
                }

                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while updating discounted prices.";
            }

        }

    }
}
