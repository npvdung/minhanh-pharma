
namespace Base_Asp_Core_MVC_with_Identity.Models.View
{
    public class ImportViewModel
    {
        public Import ImportMaster { get; set; } = new Import();
        public List<ImportProducts> ProductDetails { get; set; } = new List<ImportProducts>();
    }
}
