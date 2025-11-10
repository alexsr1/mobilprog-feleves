using Mobilprog.ViewModels;

namespace Mobilprog.Views
{
    [QueryProperty(nameof(ReceiptId), "receiptId")]
    public partial class ReceiptDetailPage : ContentPage
    {
        private readonly ReceiptDetailViewModel _viewModel;

        public int ReceiptId
        {
            set
            {
                _viewModel.ReceiptId = value;
            }
        }

        public ReceiptDetailPage(ReceiptDetailViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}
