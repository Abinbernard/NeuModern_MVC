using NeuModern.Areas.Identity.Data;
using NeuModern.Models;
using NeuModern.Repository.IRepository;
using System.Linq.Expressions;

namespace NeuModern.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
        }
        public void ClearCart(string userId)
        {
            var cartItems = _db.ShoppingCarts.Where(c => c.ApplicationUserId == userId).ToList();
            _db.ShoppingCarts.RemoveRange(cartItems);
        }
    }
}




//using PlusPerfect.Data;
//using PlusPerfect.Models;

//public class CategoryRepository : ICategoryRepository
//{
//    private readonly ApplicationDbContext _context;

//    public CategoryRepository(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public IEnumerable<Category> GetAll()
//    {
//        return _context.Categories.Where(c => !c.IsDeleted); // Exclude soft deleted categories
//    }

//    public void Add(Category category)
//    {
//        _context.Categories.Add(category);
//    }

//    public void Update(Category category)
//    {
//        _context.Categories.Update(category);
//    }

//    public void Remove(Category category)
//    {
//        category.IsDeleted = true; // Soft delete by marking as deleted
//        _context.Categories.Update(category);
//    }
//}
