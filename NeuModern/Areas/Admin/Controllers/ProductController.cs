
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NeuModern.Models;
using NeuModern.Models.ViewModel;
using NeuModern.Repository.IRepository;
using System.Text.RegularExpressions;

namespace NeuModern.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Role.Role_Admin)]

    public class ProductController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            try
            {
                List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

                return View(objProductList);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching products.";
                return View(new List<Product>());
            }    
        }
        public IActionResult Upsert(int? id)
        {
            //ViewData["CategoryList"]= CategoryList;
            try
            {
                ProductVM productVM = new()
                {
                    CategoryList = _unitOfWork.Category
               .GetAll().Select(u => new SelectListItem
               {
                   Text = u.Name,
                   Value = u.Id.ToString()
               }),
                    Product = new Product()
                };
                if (id == null || id == 0)
                {
                    return View(productVM);
                }
                else
                {
                    productVM.Product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "ProductImages");
                    return View(productVM);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while loading the product details.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    if (productVM.Product.Id == 0)
                    {
                        _unitOfWork.Product.Add(productVM.Product);
                    }
                    else
                    {
                        _unitOfWork.Product.Update(productVM.Product);
                    }

                    _unitOfWork.Save();

                    if (files != null && files.Any())
                    {

                        string wwwRootPath = _webHostEnvironment.WebRootPath;

                        foreach (var file in files)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string productPath = @"images/products/product-" + productVM.Product.Id;
                            string finalPath = Path.Combine(wwwRootPath, productPath);

                            if (!Directory.Exists(finalPath))
                            {
                                Directory.CreateDirectory(finalPath);
                            }

                            string filePath = Path.Combine(finalPath, fileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                file.CopyTo(fileStream);
                            }

                            ProductImage productImage = new ProductImage
                            {
                                ImageUrl = "/" + productPath + "/" + fileName,
                                ProductId = productVM.Product.Id
                            };

                            productVM.Product.ProductImages ??= new List<ProductImage>();
                            productVM.Product.ProductImages.Add(productImage);
                        }

                        _unitOfWork.Product.Update(productVM.Product);
                        _unitOfWork.Save();
                    }

                    TempData["success"] = "Product Created/Updated Successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    productVM.CategoryList = _unitOfWork.Category
                        .GetAll()
                        .Select(u => new SelectListItem
                        {
                            Text = u.Name,
                            Value = u.Id.ToString()
                        });

                    return View(productVM);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while saving the product.";
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
        }


        public IActionResult DeleteImage(int imageId)
        {
            try
            {
                var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
                int productId = imageToBeDeleted.ProductId;
                if (imageToBeDeleted != null)
                {
                    if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                    {
                        var oldImagePath =
                                   Path.Combine(_webHostEnvironment.WebRootPath,
                                   imageToBeDeleted.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                    _unitOfWork.Save();
                    TempData["success"] = "Deleted successfully";

                }
                return RedirectToAction(nameof(Upsert), new { id = productId });
            }
            catch ( Exception ex)
            {
                TempData["error"] = "An error occurred while deleting the image.";
                return RedirectToAction("Index");
            }
            
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
                return Json(new { data = objProductList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while fetching the products." });
            }
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            try
            {
                var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
                if (productToBeDeleted == null)
                {
                    return Json(new { success = false, message = "Error while deleting" });
                }

                string productPath = @"images\products\product-" + id;
                string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

                if (!Directory.Exists(finalPath))
                {
                    string[] filePaths = Directory.GetFiles(finalPath);
                    foreach (string filePath in filePaths)
                    {
                        System.IO.File.Delete(filePath);
                    }
                    Directory.Delete(finalPath);
                }

                _unitOfWork.Product.Remove(productToBeDeleted);
                _unitOfWork.Save();

                return Json(new { success = true, message = "Delete Successfull" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting the product." });
            }
           
        }
    }
}





