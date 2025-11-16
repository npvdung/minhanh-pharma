namespace Base_Asp_Core_MVC_with_Identity.Models.View
{
    public class ProductViewModel
    {
       public Product productMaster { get; set; } = new Product();
        public List<ProductUnit> productUnits { get;set; } = new List<ProductUnit>();
    }
   
}
