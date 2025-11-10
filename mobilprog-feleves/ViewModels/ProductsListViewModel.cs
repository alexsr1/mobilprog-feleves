using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Mobilprog.Models;
using Mobilprog.Services;

namespace Mobilprog.ViewModels
{
    public partial class ProductsListViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        [ObservableProperty]
        private bool _isBusy;

        public ProductsListViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Products = new ObservableCollection<Product>();
        }

        [RelayCommand]
        private async Task LoadProducts()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Products.Clear();
                var allProducts = await _databaseService.GetAllProductsAsync();

                // Group products by ReadableName to show unique products with their prices
                // For simplicity, this example just lists all products.
                // A more advanced implementation might aggregate prices or show a list of unique product names.
                foreach (var product in allProducts.OrderBy(p => p.ReadableName))
                {
                    Products.Add(product);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Optional: Command to navigate to a product detail page if needed
        [RelayCommand]
        private async Task GoToProductDetails(Product product)
        {
            if (product == null) return;
            // Example: await Shell.Current.GoToAsync($"///ProductDetailPage?productId={product.Id}");
            await Shell.Current.DisplayAlert("Product Details", $"Product: {product.ReadableName}\nPrice: {product.Price} {product.Unit}", "OK");
        }
    }
}
