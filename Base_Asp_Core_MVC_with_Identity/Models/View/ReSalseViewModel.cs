namespace Base_Asp_Core_MVC_with_Identity.Models.View
{
    public class ReSalseViewModel
    {
        public ReSales reSalesMaster { get; set; } = new ReSales();
        public List<ReSalesDetail> reSalesDetails { get; set; } = new List<ReSalesDetail>();
    }
}
