using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;
using QuickCode.Components.Data;
using QuickCode.Model;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCode.ViewModels
{
    public class QrCodeGeneratorPageViewModel : ObservableObject
    {
        #region Fields
        private readonly DispatcherTimer timer;
        private bool isBusy;
        private IQrCodeDataViewModel selectedDataModel = null!;
        private BitmapSource? qrCodePreview;
        private SvgImageSource? qrCodePreviewSvg;
        private string? plainText;
        #endregion

        #region Constructors
        public QrCodeGeneratorPageViewModel()
        {
            timer = new() { Interval = TimeSpan.FromMilliseconds(200) };
            timer.Tick += OnTimerTick;
            SelectedDataModel = new QrCodeTextDataViewModel();
            DataModels = new IQrCodeDataViewModel[]
            {
                new QrCodeCalendarEventViewModel(), new QrCodeCallDataViewModel(),
                new QrCodeEmailDataViewModel(), new QrCodeLinkDataViewModel(),
                new QrCodeLocationViewModel(), new QrCodeSepaPaymentDataViewModel(),
                new QrCodeSmsDataViewModel(), new QrCodeTextDataViewModel(),
                new QrCodeVcardDataViewModel(), new QrCodeWifiDataViewModel(),
            };
            SelectedDataModel = DataModels.First(x => x is QrCodeTextDataViewModel);
        }
        #endregion

        #region Properties
        public IQrCodeDataViewModel SelectedDataModel { get => selectedDataModel; private set => SelectDataModel(value); }
        public IQrCodeDataViewModel[] DataModels { get; }
        public bool IsBusy { get => isBusy; private set { isBusy = value; OnPropertyChanged(); } }
        public BitmapSource? QrCodePreview { get => qrCodePreview; private set { qrCodePreview = value; OnPropertyChanged(); } }
        public SvgImageSource? QrCodePreviewSvg { get => qrCodePreviewSvg; private set { qrCodePreviewSvg = value; OnPropertyChanged(); } }
        #endregion

        #region Handlers
        private void SelectedDataModel_RawDataReceived(object? sender, string? e)
        {
            plainText = e;
            IsBusy = true;
            timer.Stop();
            timer.Start();
        }
        private async void OnTimerTick(object? sender, object e)
        {
            timer.Stop();
            await GenerateQrCodeSvgAsync(plainText);
        }
        #endregion

        #region Helpers
        private void SelectDataModel(IQrCodeDataViewModel value)
        {
            if (selectedDataModel != null)
            {
                selectedDataModel.RawDataReceived -= SelectedDataModel_RawDataReceived;
            }
            selectedDataModel = value;
            selectedDataModel.RawDataReceived += SelectedDataModel_RawDataReceived;
            OnPropertyChanged(nameof(SelectedDataModel));
            QrCodePreview = null;
            QrCodePreviewSvg = null;
        }
        private async Task GenerateQrCodeAsync(string? plainText)
        {
            try
            {
                BitmapSource? bitmap = null;

                if (!string.IsNullOrWhiteSpace(plainText))
                {
                    using var qrGenerator = new QRCodeGenerator();
                    using var qrCodeData = qrGenerator.CreateQrCode(plainText, QRCodeGenerator.ECCLevel.Q);
                    using var qrCode = new PngByteQRCode(qrCodeData);
                    using var stream = new MemoryStream(qrCode.GetGraphic(20));
                    using var randomAccessStream = stream.AsRandomAccessStream();

                    bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(randomAccessStream);
                }
                QrCodePreview = bitmap;
                IsBusy = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                IsBusy = false;
            }
        }
        private async Task GenerateQrCodeSvgAsync(string? plainText)
        {
            try
            {
                SvgImageSource a = null!;
                if (!string.IsNullOrWhiteSpace(plainText))
                {
                    using var qrGenerator = new QRCodeGenerator();
                    using var qrCodeData = qrGenerator.CreateQrCode(plainText, QRCodeGenerator.ECCLevel.Q);
                    using var qrCode = new SvgQRCode(qrCodeData);
                    var svgXml = qrCode.GetGraphic(20);
                    var svgProcessor = new QrCodeSvgProcessor(svgXml);
                    using var stream = new MemoryStream(svgProcessor.ToByteArray());
                    using var randomAccessStream = stream.AsRandomAccessStream();

                    a = new SvgImageSource();
                    await a.SetSourceAsync(randomAccessStream);
                }
                QrCodePreviewSvg = a;
                IsBusy = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                IsBusy = false;
            }
        }
        #endregion
    }
}
