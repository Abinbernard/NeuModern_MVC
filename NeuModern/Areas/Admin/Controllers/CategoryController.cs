
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuModern.Models;
using NeuModern.Repository;
using NeuModern.Repository.IRepository;
using System.Text.RegularExpressions;

namespace NeuModern.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles=Role.Role_Admin)]
    public class CategoryController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            try
            {
                List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
                return View(objCategoryList);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching categories.";
                return View(new List<Category>());
            }           
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            try
            {
                if (obj.Name == obj.DisplayOrder.ToString())
                {
                    ModelState.AddModelError("Name", "The DisplayOrder cannot exactly match the Name");
                }


                if (_unitOfWork.Category.GetAll().Any(c => c.Name == obj.Name))
                {
                    ModelState.AddModelError("Name", "This CategoryName Already Taken");

                }
                if (_unitOfWork.Category.GetAll().Any(c => c.DisplayOrder == obj.DisplayOrder))
                {
                    ModelState.AddModelError("DisplayOrder", "This DisplayOrder Already Taken");

                }
                if (!Regex.IsMatch(obj.Name, @"^[a-zA-Z]+$"))
                {
                    ModelState.AddModelError("Name", "Only alphabetic characters are allowed in the Name field");
                }
                obj.IsActive = true;
                if (ModelState.IsValid)
                {
                    _unitOfWork.Category.Add(obj);
                    _unitOfWork.Save();
                    TempData["success"] = "Category Created Successfully";
                    return RedirectToAction("Index");
                }

                return View(obj);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while creating the category.";
                return View(obj);
            } 
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            try
            {
                Category categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);
                if (categoryFromDb == null)
                {
                    return NotFound();
                }
                return View(categoryFromDb);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching the category details.";
                return RedirectToAction("Index");
            }
           
        }
        [HttpPost]
        public IActionResult Edit(Category obj, int id, [Bind("Id,Name,DisplayOrder,IsActive")] Category category)
        {
            if (obj.Id == 0)
            {
                return NotFound();
            }
            try
            {
                var existingCategory = _unitOfWork.Category.Get(u => u.Id != obj.Id && u.Name == obj.Name);
                if (existingCategory != null)
                {
                    ModelState.AddModelError("Name", "This CategoryName Already Taken");
                }

                existingCategory = _unitOfWork.Category.Get(u => u.Id != obj.Id && u.DisplayOrder == obj.DisplayOrder);
                if (existingCategory != null)
                {
                    ModelState.AddModelError("DisplayOrder", "This DisplayOrder Already Taken");
                }

                if (ModelState.IsValid)
                {
                    _unitOfWork.Category.Update(obj);
                    _unitOfWork.Save();
                    TempData["success"] = "Category Updated Successfully";
                    return RedirectToAction("Index");
                }

                return View(obj);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while updating the category.";
                return View(obj);
            }
            
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            try
            {
                Category categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);
                if (categoryFromDb == null)
                {
                    return NotFound();
                }
                return View(categoryFromDb);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching the category details.";
                return RedirectToAction("Index");
            }
           
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            try
            {
                Category? obj = _unitOfWork.Category.Get(u => u.Id == id);
                if (obj == null)
                {
                    return NotFound();
                }

                _unitOfWork.Category.Remove(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                
                TempData["error"] = "An error occurred while deleting the category.";
                return RedirectToAction("Index");
            }

        }
    }
}