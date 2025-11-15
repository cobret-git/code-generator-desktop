using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.Storage.Pickers;
using QRCoder;
using QuickCode.Components.Data;
using QuickCode.Model;
using SkiaSharp;
using Svg;
using Svg.Skia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using WinRT.Interop;

namespace QuickCode.ViewModels
{
    public partial class QrCodeGeneratorPageViewModel : ObservableObject
    {
        #region Fields
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly DispatcherTimer timer;
        private bool isBusy;
        private IQrCodeDataViewModel selectedDataModel = null!;
        private QrCodeSvgProcessor? svgProcessor;
        private SvgImageSource? qrCodePreviewSvg;
        private string? plainText;
        private ExportImageOptinos selectedExportOption = ExportImageOptinos.Svg;
        private SKSvg? svg = null;
        #endregion

        #region Constructors
        public QrCodeGeneratorPageViewModel()
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
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
        public ExportImageOptinos SelectedExportOption { get => selectedExportOption; set { selectedExportOption = value; OnPropertyChanged(); } }
        public IQrCodeDataViewModel SelectedDataModel { get => selectedDataModel; set => SelectDataModel(value); }
        public IQrCodeDataViewModel[] DataModels { get; }
        public bool IsBusy { get => isBusy; private set { isBusy = value; OnIsBusyChanged(); } }
        public SvgImageSource? QrCodePreviewSvg { get => qrCodePreviewSvg; private set { qrCodePreviewSvg = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        [RelayCommand(CanExecute = nameof(CanRegenerate))] private async Task ExportAsImage(ExportImageOptinos options)
        {
            try
            {
                SelectedExportOption = options;
                ArgumentNullException.ThrowIfNull(svgProcessor);
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.MainWindow);
                var picker = new Windows.Storage.Pickers.FileSavePicker();
                InitializeWithWindow.Initialize(picker, hwnd);
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                if (options == ExportImageOptinos.Svg) picker.FileTypeChoices.Add("SVG Image", new List<string>() { ".svg" });
                else picker.FileTypeChoices.Add("PNG Image", new List<string>() { ".png" });
                picker.SuggestedFileName = $"qr-code {DateTime.Now:dd-MM-yyyy HH-mm-ss}";
                var file = await picker.PickSaveFileAsync();
                if (file == null) return;
                using var stream = await file.OpenStreamForWriteAsync();
                switch (options)
                {
                    case ExportImageOptinos.Svg: SaveToSvgFile(stream, svgProcessor); break;
                    case ExportImageOptinos.PngXs: await SaveToPngFile(stream, 128); break;
                    case ExportImageOptinos.PngS: await SaveToPngFile(stream, 256); break; 
                    case ExportImageOptinos.PngM: await SaveToPngFile(stream, 512); break; 
                    case ExportImageOptinos.PngL: await SaveToPngFile(stream, 1024); break; 
                    case ExportImageOptinos.PngXl: await SaveToPngFile(stream, 2048); break; 
                    default: throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {

            }
        }
        [RelayCommand(CanExecute = nameof(CanRegenerate))] private async Task RegenerateQrCode()
        {
            IsBusy = true;
            await GenerateQrCodeSvgAsync(plainText);
        }
        #endregion

        #region Handlers
        private void SelectedDataModel_RawDataReceived(object? sender, string? e)
        {
            NotifyCanExecuteChanged();
            plainText = e;
            IsBusy = true;
            timer.Stop();
            timer.Start();
        }
        private void OnIsBusyChanged()
        {
            NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsBusy));
        }
        private async void OnTimerTick(object? sender, object e)
        {
            timer.Stop();
            await GenerateQrCodeSvgAsync(plainText);
        }
        #endregion

        #region CanExecute
        private bool CanRegenerate() => !IsBusy && !string.IsNullOrWhiteSpace(plainText);
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
            QrCodePreviewSvg = null;
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
                    this.svgProcessor = new QrCodeSvgProcessor(svgXml);
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
        private void SaveToSvgFile(Stream stream, QrCodeSvgProcessor qrCodeSvgProcessor)
        {
            using var sw = new StreamWriter(stream);
            sw.Write(qrCodeSvgProcessor.ToSvgString());
        }
        private async Task SaveToPngFile(Stream stream, int size)
        {
            using var memStream = new MemoryStream(svgProcessor.ToByteArray());
            var svgPath = svgProcessor.ToSvgString();
            var svg = this.svg ?? new SKSvg();
            if (this.svg == null) this.svg = svg;
            svg.Load(memStream);

            var info = new SKImageInfo(size, size);
            using var surface = SKSurface.Create(info);
            using var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            // Scale to fit
            var scaleX = (float)size / svg.Picture.CullRect.Width;
            var scaleY = (float)size / svg.Picture.CullRect.Height;
            canvas.Scale(Math.Min(scaleX, scaleY));
            canvas.DrawPicture(svg.Picture);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            data.SaveTo(stream);
        }
        private void NotifyCanExecuteChanged()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                RegenerateQrCodeCommand.NotifyCanExecuteChanged();
                ExportAsImageCommand.NotifyCanExecuteChanged();
            });
        }
        #endregion
    }
}
