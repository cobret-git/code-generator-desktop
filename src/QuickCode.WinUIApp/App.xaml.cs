using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using QuickCode.ViewModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace QuickCode
{
    public partial class App : Application
    {
        #region Fields
        private MainWindow? _window;
        #endregion

        #region Constructors
        public App()
        {
            Services = ConfigureServices();
            InitializeComponent();
        }
        #endregion

        #region Properties
        public new static App Current => (App)Application.Current;
        public MainWindow? MainWindow { get => _window; }
        public IServiceProvider Services { get; }
        #endregion

        #region Handlers
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
        #endregion

        #region Helpers
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<QrCodeGeneratorPageViewModel>();
            services.AddSingleton<SettingsPageViewModel>();

            return services.BuildServiceProvider();
        }
        #endregion
    }
}
