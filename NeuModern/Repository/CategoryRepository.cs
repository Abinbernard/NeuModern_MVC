
using NeuModern.Areas.Identity.Data;
using NeuModern.Models;
using NeuModern.Repository.IRepository;

namespace NeuModern.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Category obj)
        {
            _db.Categories.Update(obj);
        }
    }
}



