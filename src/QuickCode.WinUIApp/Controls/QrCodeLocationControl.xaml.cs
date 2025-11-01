using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using QuickCode.ViewModels;
using System;
using System.Text.Json;

namespace QuickCode.Controls
{
    public sealed partial class QrCodeLocationControl : UserControl
    {
        #region Fields
        private readonly Microsoft.Windows.ApplicationModel.Resources.ResourceLoader resourceLoader = new();
        private double? selectedLatitude;
        private double? selectedLongitude;
        #endregion

        #region Constructors
        public QrCodeLocationControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        private async void OnPickLocationClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not QrCodeLocationViewModel viewModel)
            {
                return;
            }

            selectedLatitude = viewModel.Latitude;
            selectedLongitude = viewModel.Longitude;

            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = resourceLoader.GetString("QrCodeLocationPickerDialog.Title"),
                PrimaryButtonText = resourceLoader.GetString("QrCodeLocationPickerUseButton.Content"),
                CloseButtonText = resourceLoader.GetString("QrCodeLocationPickerCancelButton.Content"),
                DefaultButton = ContentDialogButton.Primary,
                MinWidth = 480,
                MinHeight = 480
            };

            var webView = new WebView2
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            dialog.Content = webView;

            try
            {
                await webView.EnsureCoreWebView2Async();
            }
            catch (Exception)
            {
                dialog.Content = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = resourceLoader.GetString("QrCodeLocationStatusGenericError.Text")
                };
            }

            CoreWebView2WebMessageReceivedEventHandler? messageHandler = null;
            EventHandler<CoreWebView2DOMContentLoadedEventArgs>? domHandler = null;

            if (webView.CoreWebView2 != null)
            {
                messageHandler = (senderView, args) =>
                {
                    try
                    {
                        var payload = JsonSerializer.Deserialize<MapSelection>(args.WebMessageAsJson, JsonOptions);
                        if (payload != null)
                        {
                            selectedLatitude = payload.Latitude;
                            selectedLongitude = payload.Longitude;
                        }
                    }
                    catch
                    {
                    }
                };

                domHandler = (senderView, args) =>
                {
                    var latitude = selectedLatitude ?? 0;
                    var longitude = selectedLongitude ?? 0;
                    var zoom = selectedLatitude.HasValue && selectedLongitude.HasValue ? 12 : 2;
                    var message = JsonSerializer.Serialize(new MapSelection(latitude, longitude, zoom), JsonOptions);
                    senderView.PostWebMessageAsJson(message);
                };

                webView.CoreWebView2.WebMessageReceived += messageHandler;
                webView.CoreWebView2.DOMContentLoaded += domHandler;
                webView.NavigateToString(MapPickerHtml);

                dialog.Closed += (s, args) =>
                {
                    if (webView.CoreWebView2 != null)
                    {
                        if (messageHandler != null)
                        {
                            webView.CoreWebView2.WebMessageReceived -= messageHandler;
                        }
                        if (domHandler != null)
                        {
                            webView.CoreWebView2.DOMContentLoaded -= domHandler;
                        }
                    }
                };
            }

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && selectedLatitude.HasValue && selectedLongitude.HasValue)
            {
                viewModel.SetCoordinates(selectedLatitude.Value, selectedLongitude.Value);
            }
        }
        #endregion

        #region Types
        private sealed record MapSelection(double Latitude, double Longitude, int Zoom = 2);
        #endregion

        #region Constants
        private const string MapPickerHtml = @"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' integrity='sha256-sA+4J8b6E3lPoY7MT8ZDiJ9N6UNN1p6EDWmZ0i0wHmw=' crossorigin=''/>
<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js' integrity='sha256-Y9t9zNig0DJ2m3dA5lZ3YKD7RyhokW2Nlv1qf0R0zDY=' crossorigin=''></script>
<style>
html, body, #map {
    height: 100%;
    margin: 0;
}
</style>
</head>
<body>
<div id='map'></div>
<script>
const map = L.map('map').setView([0, 0], 2);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap contributors'
}).addTo(map);
const marker = L.marker([0, 0]).addTo(map);
function updateMarker(lat, lng, zoom) {
    marker.setLatLng([lat, lng]);
    if (typeof zoom === 'number') {
        map.setView([lat, lng], zoom, { animate: true });
    }
}
map.on('click', function (e) {
    const lat = e.latlng.lat;
    const lng = e.latlng.lng;
    updateMarker(lat, lng);
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage({ latitude: lat, longitude: lng });
    }
});
if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', event => {
        const data = event.data;
        if (!data) {
            return;
        }
        const lat = typeof data.latitude === 'number' ? data.latitude : 0;
        const lng = typeof data.longitude === 'number' ? data.longitude : 0;
        const zoom = typeof data.zoom === 'number' ? data.zoom : null;
        updateMarker(lat, lng, zoom);
    });
}
</script>
</body>
</html>";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        #endregion
    }
}
