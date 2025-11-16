namespace Base_Asp_Core_MVC_with_Identity.Models.View
{
    public class InvoiceViewModel
    {
       public Sales Sales { get; set; } = new Sales();
        public List<SalesProducts> salesProductsDetails { get; set; } = new List<SalesProducts>();
    }
}
