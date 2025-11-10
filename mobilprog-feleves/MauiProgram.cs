using Microsoft.Extensions.Logging;
using Mobilprog.Services;
using Mobilprog.ViewModels;
using Mobilprog.Views;
using Mobilprog.Converters;


namespace Mobilprog
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Services
            builder.Services.AddSingleton<DatabaseService>();
            // You'll need to provide your Google Gemini API key here
            // For example, from a configuration file or environment variable
            builder.Services.AddSingleton<LlmService>(s => new LlmService("YOUR_GOOGLE_GEMINI_API_KEY"));
            builder.Services.AddSingleton<PdfParsingService>();

            // Register ViewModels
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<ReceiptsListViewModel>();
            builder.Services.AddSingleton<ProductsListViewModel>();
            builder.Services.AddSingleton<ReceiptDetailViewModel>(); // Singleton for detail viewmodel
            builder.Services.AddTransient<ReceiptDetailPage>(); // Transient for detail pages that take parameters

            // Register Views
            builder.Services.AddSingleton<AppShell>(); // Register AppShell
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<ReceiptsListPage>();
            builder.Services.AddSingleton<ProductsListPage>();

            // Register Converters
            builder.Services.AddSingleton<IsNotNullConverter>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
