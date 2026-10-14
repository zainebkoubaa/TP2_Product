namespace TP_Product.ViewModels
{
    public class EditViewModel : CreateViewModel
    {
        public int ProductId { get; set; }
        public string ExistingImagePath { get; set; }
    }
}