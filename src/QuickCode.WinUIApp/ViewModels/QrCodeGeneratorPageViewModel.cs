using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;
using QuickCode.Components.Data;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace QuickCode.ViewModels
{
    public class QrCodeGeneratorPageViewModel : ObservableObject
    {
        #region Fields
        private readonly DispatcherTimer timer;
        private bool isBusy;
        private IQrCodeDataViewModel selectedDataModel = null!;
        private QrCodeTypes selectedType = QrCodeTypes.Text;
        private BitmapSource? qrCodePreview;
        private string? plainText;
        #endregion

        #region Constructors
        public QrCodeGeneratorPageViewModel()
        {
            timer = new() { Interval = TimeSpan.FromMilliseconds(200) };
            timer.Tick += OnTimerTick;
            SelectedDataModel = new QrCodeTextDataViewModel();
            Types = Enum.GetValues<QrCodeTypes>();
        }
        #endregion

        #region Properties
        public IQrCodeDataViewModel SelectedDataModel { get => selectedDataModel; private set => SelectDataModel(value); }
        public QrCodeTypes SelectedType { get => selectedType; set { selectedType = value; OnSelectedTypeChanged(value); } }
        public QrCodeTypes[] Types { get; }
        public bool IsBusy { get => isBusy; private set { isBusy = value; OnPropertyChanged(); } }
        public BitmapSource? QrCodePreview { get => qrCodePreview; private set { qrCodePreview = value; OnPropertyChanged(); } }
        #endregion

        #region Handlers
        private void OnSelectedTypeChanged(QrCodeTypes type)
        {
            try
            {
                switch (type)
                {
                    case QrCodeTypes.Text: SelectedDataModel = new QrCodeTextDataViewModel(); break;
                    case QrCodeTypes.Link: SelectedDataModel = new QrLinkDataViewModel(); break;
                    case QrCodeTypes.Call: SelectedDataModel = new QrCodeCallDataViewModel(); break;
                    case QrCodeTypes.Sms: SelectedDataModel = new QrCodeSmsDataViewModel(); break;
                    case QrCodeTypes.Email: SelectedDataModel = new QrCodeEmailDataViewModel(); break;
                    case QrCodeTypes.Wifi: SelectedDataModel = new QrWifiDataViewModel(); break;
                    case QrCodeTypes.Location: SelectedDataModel = new QrCodeLocationViewModel(); break;
                    case QrCodeTypes.Vcard: SelectedDataModel = new QrCodeVcardDataViewModel(); break;
                    default: throw new NotImplementedException($"Not supported type: \"{type}\"");
                }
                QrCodePreview = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            OnPropertyChanged(nameof(SelectedType));
        }
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
            await GenerateQrCodeAsync(plainText);
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
        #endregion
    }
}
