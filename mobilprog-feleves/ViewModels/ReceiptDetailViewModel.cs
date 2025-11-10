using CommunityToolkit.Mvvm.ComponentModel;
using Mobilprog.Models;
using Mobilprog.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mobilprog.ViewModels
{
    [QueryProperty(nameof(ReceiptId), "receiptId")]
    public partial class ReceiptDetailViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public ReceiptDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Products = new ObservableCollection<Product>();
        }

        public int ReceiptId
        {
            set
            {
                LoadReceiptDetails(value);
            }
        }

        [ObservableProperty]
        private Receipt? _receipt;

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        private async void LoadReceiptDetails(int receiptId)
        {
            Receipt = await _databaseService.GetReceiptAsync(receiptId);
            Products.Clear();
            var productList = await _databaseService.GetProductsForReceiptAsync(receiptId);
            foreach (var product in productList)
            {
                Products.Add(product);
            }
        }
    }
}
