using Mobilprog.ViewModels;

namespace Mobilprog.Views
{
    public partial class MainPage : ContentPage
    {
        private int count = 0;

        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;
            if (sender is Button button)
            {
                button.Text = $"Clicked {count} time{(count == 1 ? "" : "s")}";
            }
        }
    }
}
