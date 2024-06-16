using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NeuModern.Models.ViewModel
{
    public class OfferVM
    {
    

            public Offer Offer { get; set; }

            [ValidateNever]
            public IEnumerable<Category> Categories { get; set; }

            [ValidateNever]
            public IEnumerable<Product> Products { get; set; }
            public int? SelectCategoryId { get; set; }
            public int? SelectProductId { get; set; }
        
    }
}
