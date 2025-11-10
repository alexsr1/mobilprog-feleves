using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Mobilprog.Models;
using Mobilprog.Services;

namespace Mobilprog.ViewModels
{
    public partial class ReceiptsListViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<Receipt> _receipts;

        [ObservableProperty]
        private bool _isBusy;

        public ReceiptsListViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Receipts = new ObservableCollection<Receipt>();
        }

        [RelayCommand]
        private async Task LoadReceipts()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Receipts.Clear();
                var receiptList = await _databaseService.GetReceiptsAsync();
                foreach (var receipt in receiptList)
                {
                    Receipts.Add(receipt);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToReceiptDetails(Receipt receipt)
        {
            if (receipt == null) return;
            await Shell.Current.GoToAsync($"///ReceiptDetailPage?receiptId={receipt.Id}");
        }

        [RelayCommand]
        private async Task DeleteReceipt(Receipt receipt)
        {
            if (receipt == null) return;

            // Add confirmation dialog in the UI
            bool confirm = await Shell.Current.DisplayAlert("Delete Receipt", $"Are you sure you want to delete the receipt from {receipt.StoreName} on {receipt.Date.ToShortDateString()}?", "Yes", "No");

            if (confirm)
            {
                await _databaseService.DeleteReceiptAsync(receipt);
                await LoadReceipts(); // Reload the list after deletion
            }
        }

        [RelayCommand]
        private async Task AddReceipt()
        {
            // Navigate to a page where a new receipt can be added or uploaded
            // For now, let's navigate to the main page which has the upload functionality
            await Shell.Current.GoToAsync("///MainPage");
        }
    }
}
