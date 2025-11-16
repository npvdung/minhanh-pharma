namespace Base_Asp_Core_MVC_with_Identity.Models.View
{
    public class ReturnViewModel
    {
       public DisposalRecords disposalRecordsMaster { get; set; } = new DisposalRecords();
        public List<DisposalProducts> ReturnsDetails { get;set; } = new List<DisposalProducts>();
    }
   
}
