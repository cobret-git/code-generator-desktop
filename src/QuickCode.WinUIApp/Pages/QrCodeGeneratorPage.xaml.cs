using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using QuickCode.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace QuickCode.Pages
{
    public sealed partial class QrCodeGeneratorPage : Page
    {
        public QrCodeGeneratorPage()
        {
            this.DataContext = App.Current.Services.GetService<QrCodeGeneratorPageViewModel>();
            InitializeComponent();
        }
        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            DownloadSelectedOption.Text = item.Text;
        }
    }
}
