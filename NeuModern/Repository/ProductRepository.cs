using NeuModern.Areas.Identity.Data;

using NeuModern.Models;
using NeuModern.Repository.IRepository;
using System.Linq.Expressions;

namespace NeuModern.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product obj)
        {
            var objFromDb = _db.Products.FirstOrDefault(u=> u.Id == obj.Id);
            if (objFromDb != null) 
            {
                objFromDb.Name = obj.Name;
                objFromDb.Description = obj.Description;
                objFromDb.Size = obj.Size;
                objFromDb.StockQuantity = obj.StockQuantity;
                objFromDb.Brand = obj.Brand;
                objFromDb.Discount = obj.Discount;
                objFromDb.OfferPrice = obj.OfferPrice;
                objFromDb.Price = obj.Price;
                objFromDb.CategoryId = obj.CategoryId;
                objFromDb.ProductImages = obj.ProductImages;
               
                //if(obj.ImageUrl!= null)
                //{
                //    objFromDb.ImageUrl = obj.ImageUrl;
                //}

            }
        }
    }
}

