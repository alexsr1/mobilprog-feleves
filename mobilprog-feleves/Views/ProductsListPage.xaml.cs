using Mobilprog.ViewModels;

namespace Mobilprog.Views
{
    public partial class ProductsListPage : ContentPage
    {
        public ProductsListPage(ProductsListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ProductsListViewModel viewModel)
            {
                viewModel.LoadProductsCommand.Execute(null);
            }
        }
    }
}
