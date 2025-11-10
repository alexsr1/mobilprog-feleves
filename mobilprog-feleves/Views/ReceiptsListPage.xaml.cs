using Mobilprog.ViewModels;

namespace Mobilprog.Views
{
    public partial class ReceiptsListPage : ContentPage
    {
        public ReceiptsListPage(ReceiptsListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ReceiptsListViewModel viewModel)
            {
                viewModel.LoadReceiptsCommand.Execute(null);
            }
        }
    }
}
