using Avalonia.Controls;
using GymManager.ViewModels;

namespace GymManager.Views
{
    public partial class ClientWindow : Window
    {
        public ClientWindow()
        {
            InitializeComponent();
        }

        public ClientWindow(ClientWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
