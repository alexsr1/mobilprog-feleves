using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mobilprog.Models;
using Mobilprog.Services;

namespace Mobilprog.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly PdfParsingService _pdfParsingService;
        private readonly LlmService _llmService;

        [ObservableProperty]
        private Receipt? _latestReceipt;

        [ObservableProperty]
        private ObservableCollection<Product> _latestReceiptProducts;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string? _statusMessage;

        public MainViewModel(DatabaseService databaseService, PdfParsingService pdfParsingService, LlmService llmService)
        {
            _databaseService = databaseService;
            _pdfParsingService = pdfParsingService;
            _llmService = llmService;
            LatestReceiptProducts = new ObservableCollection<Product>();
            _statusMessage = "Ready"; // Initialize status message
            _ = LoadLatestReceiptAsync(); // Load on startup as fire-and-forget
        }

        [RelayCommand]
        private async Task UploadReceipt()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = "Selecting PDF...";

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Pdf,
                    PickerTitle = "Select a PDF receipt"
                });

                if (result == null)
                {
                    StatusMessage = "PDF selection cancelled.";
                    return;
                }

                StatusMessage = $"Parsing PDF: {Path.GetFileName(result.FileName)}...";
                var (receipt, products) = await _pdfParsingService.ParseReceiptAsync(result.FullPath);

                if (products.Any())
                {
                    StatusMessage = "Enhancing product names with AI...";
                    var originalNames = products.Select(p => p.OriginalName).Where(name => name != null).ToList(); // Filter out nulls
                    var readableNames = await _llmService.GetReadableProductNamesAsync(originalNames!); // Null-forgiving as nulls are filtered

                    for (int i = 0; i < products.Count; i++)
                    {
                        products[i].ReadableName = readableNames[i];
                    }
                }

                StatusMessage = "Saving receipt to database...";
                await _databaseService.SaveReceiptAsync(receipt, products);

                await LoadLatestReceiptAsync();
                StatusMessage = "Receipt processed and saved successfully!";
            }
            catch (FileNotFoundException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadLatestReceiptAsync()
        {
            var receipts = await _databaseService.GetReceiptsAsync();
            LatestReceipt = receipts.FirstOrDefault();

            LatestReceiptProducts.Clear();
            if (LatestReceipt != null)
            {
                var products = await _databaseService.GetProductsForReceiptAsync(LatestReceipt.Id);
                foreach (var product in products)
                {
                    LatestReceiptProducts.Add(product);
                }
            }
            else
            {
                StatusMessage = "No receipts found. Upload one to get started!";
            }
        }

        // Navigation commands (to be implemented in the View's code-behind or using a navigation service)
        [RelayCommand]
        private async Task NavigateToReceiptsList()
        {
            await Shell.Current.GoToAsync("///ReceiptsListPage");
        }

        [RelayCommand]
        private async Task NavigateToProductsList()
        {
            await Shell.Current.GoToAsync("///ProductsListPage");
        }

        [RelayCommand]
        private async Task NavigateToManualEntry()
        {
            // Implement navigation to a manual entry page if needed
            StatusMessage = "Manual entry not yet implemented.";
            await Task.CompletedTask;
        }
    }
}
