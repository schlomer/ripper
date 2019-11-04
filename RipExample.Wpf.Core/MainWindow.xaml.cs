using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RipExample.Wpf.Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel { get; } = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.Initialize();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.ConnectAsync();
        }

        private async void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.QueryAsync();
        }
    }
}
