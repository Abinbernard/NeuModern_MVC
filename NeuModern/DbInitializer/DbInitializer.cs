using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NeuModern.Areas.Identity.Data;
using NeuModern.Models;

namespace NeuModern.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;


        public DbInitializer(
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            try
            {
                if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex) 
            {

            }
            if (!_roleManager.RoleExistsAsync(Role.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Role.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Role.Role_Admin)).GetAwaiter().GetResult();



                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin007@gmail.com",
                    Email = "admin007@gmail.com",
                    Name = "Admin",
                    PhoneNumber = "1234567890",
                    StreetAddress = "New York",
                    State = "USA",
                    PostalCode = "123456",
                    City = "Chicago"
                }, "Admin@123").GetAwaiter().GetResult();


                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin007@gmail.com");
                _userManager.AddToRoleAsync(user, Role.Role_Admin).GetAwaiter().GetResult();

            }


            return;
        }
    }
}
