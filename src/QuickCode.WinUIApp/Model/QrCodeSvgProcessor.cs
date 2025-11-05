using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace QuickCode.Model
{
    public class QrCodeSvgProcessor
    {
        #region Fields
        private XDocument _svgDocument;
        private XNamespace _svgNs = "http://www.w3.org/2000/svg";
        private int _nextGradientId;
        private readonly Dictionary<LinearGradientBrush, int> _gradientIds = new();
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the processor with the raw SVG XML string.
        /// </summary>
        public QrCodeSvgProcessor(string rawSvgXml)
        {
            _svgDocument = XDocument.Parse(rawSvgXml);
            // Ensure the correct namespace is used if not automatically inferred
            ArgumentNullException.ThrowIfNull(_svgDocument.Root);
            _svgNs = _svgDocument.Root.Name.Namespace;
            _nextGradientId = 0;
            EnsureDefsElement();
        }
        #endregion

        #region Methods
        public int AddGradient(LinearGradientBrush brush)
        {
            if (brush == null)
                throw new ArgumentNullException(nameof(brush));

            // Check if gradient already exists
            if (_gradientIds.TryGetValue(brush, out int existingId))
                return existingId;

            int gradientId = _nextGradientId++;
            string id = $"gradient_{gradientId}";

            var gradientElement = CreateLinearGradientElement(brush, id);

            var defs = _svgDocument.Root?.Element(_svgNs + "defs");
            defs?.Add(gradientElement);

            _gradientIds[brush] = gradientId;

            return gradientId;
        }
        public bool RemoveGradient(LinearGradientBrush brush)
        {
            if (brush == null)
                throw new ArgumentNullException(nameof(brush));

            if (!_gradientIds.TryGetValue(brush, out int gradientId))
                return false;

            string id = $"gradient_{gradientId}";

            var defs = _svgDocument.Root?.Element(_svgNs + "defs");
            var gradientElement = defs?.Elements(_svgNs + "linearGradient")
                .FirstOrDefault(e => e.Attribute("id")?.Value == id);

            if (gradientElement != null)
            {
                gradientElement.Remove();
                _gradientIds.Remove(brush);
                return true;
            }

            return false;
        }
        public void SetBackground(LinearGradientBrush brush)
        {
            if (brush == null)
                throw new ArgumentNullException(nameof(brush));

            if (!_gradientIds.TryGetValue(brush, out int gradientId))
                throw new InvalidOperationException("Gradient must be added to the document before it can be used. Call AddGradient() first.");

            var rect = _svgDocument.Root?.Element(_svgNs + "rect");
            if (rect == null)
                throw new InvalidOperationException("No <rect> element found in SVG root.");

            string gradientReference = $"url(#gradient_{gradientId})";

            var fillAttr = rect.Attribute("fill");
            if (fillAttr != null)
                fillAttr.Value = gradientReference;
            else
                rect.Add(new XAttribute("fill", gradientReference));
        }
        public void SetBackground(Color color)
        {
            var rect = _svgDocument.Root?.Element(_svgNs + "rect");
            if (rect == null)
                throw new InvalidOperationException("No <rect> element found in SVG root.");

            string colorHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";

            var fillAttr = rect.Attribute("fill");
            if (fillAttr != null)
                fillAttr.Value = colorHex;
            else
                rect.Add(new XAttribute("fill", colorHex));

            // Handle opacity if color has alpha channel
            if (color.A < 255)
            {
                double opacity = color.A / 255.0;
                var opacityAttr = rect.Attribute("fill-opacity");
                if (opacityAttr != null)
                    opacityAttr.Value = $"{opacity:F3}";
                else
                    rect.Add(new XAttribute("fill-opacity", $"{opacity:F3}"));
            }
            else
            {
                // Remove opacity attribute if color is fully opaque
                rect.Attribute("fill-opacity")?.Remove();
            }
        }
        public void SetForeground(LinearGradientBrush brush)
        {
            if (brush == null)
                throw new ArgumentNullException(nameof(brush));

            if (!_gradientIds.TryGetValue(brush, out int gradientId))
                throw new InvalidOperationException("Gradient must be added to the document before it can be used. Call AddGradient() first.");

            var path = _svgDocument.Root?.Element(_svgNs + "path");
            if (path == null)
                throw new InvalidOperationException("No <path> element found in SVG root.");

            string gradientReference = $"url(#gradient_{gradientId})";

            var fillAttr = path.Attribute("fill");
            if (fillAttr != null)
                fillAttr.Value = gradientReference;
            else
                path.Add(new XAttribute("fill", gradientReference));
        }
        public void SetForeground(Color color)
        {
            var path = _svgDocument.Root?.Element(_svgNs + "path");
            if (path == null)
                throw new InvalidOperationException("No <path> element found in SVG root.");

            string colorHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";

            var fillAttr = path.Attribute("fill");
            if (fillAttr != null)
                fillAttr.Value = colorHex;
            else
                path.Add(new XAttribute("fill", colorHex));

            // Handle opacity if color has alpha channel
            if (color.A < 255)
            {
                double opacity = color.A / 255.0;
                var opacityAttr = path.Attribute("fill-opacity");
                if (opacityAttr != null)
                    opacityAttr.Value = $"{opacity:F3}";
                else
                    path.Add(new XAttribute("fill-opacity", $"{opacity:F3}"));
            }
            else
            {
                // Remove opacity attribute if color is fully opaque
                path.Attribute("fill-opacity")?.Remove();
            }
        }
        public string ToSvgString()
        {
            return _svgDocument.ToString();
        }
        public byte[] ToByteArray()
        {
            string svgString = ToSvgString();
            return Encoding.UTF8.GetBytes(svgString);
        }
        #endregion

        #region Helpers
        private XElement CreateLinearGradientElement(LinearGradientBrush brush, string id)
        {
            // Calculate gradient vector from Rectangle and LinearGradientMode
            var rect = brush.Rectangle;
            double x1 = 0, y1 = 0, x2 = 0, y2 = 0;

            // Determine direction based on LinearGradientMode
            switch (brush.LinearColors.Length)
            {
                case 2: // Simple two-color gradient
                        // Try to determine angle from transform if available
                    var transform = brush.Transform;
                    if (transform != null && !transform.IsIdentity)
                    {
                        // Extract angle from transform matrix
                        double angle = Math.Atan2(transform.Elements[1], transform.Elements[0]);
                        double angleInDegrees = angle * 180 / Math.PI;

                        // Convert angle to x1, y1, x2, y2
                        (x1, y1, x2, y2) = AngleToCoordinates(angleInDegrees);
                    }
                    else
                    {
                        // Default horizontal gradient
                        x1 = 0; y1 = 0;
                        x2 = 1; y2 = 0;
                    }
                    break;
            }

            var gradient = new XElement(_svgNs + "linearGradient",
                new XAttribute("id", id),
                new XAttribute("x1", $"{x1:F2}"),
                new XAttribute("y1", $"{y1:F2}"),
                new XAttribute("x2", $"{x2:F2}"),
                new XAttribute("y2", $"{y2:F2}")
            );

            // Handle color blend
            if (brush.InterpolationColors != null)
            {
                var blend = brush.InterpolationColors;
                for (int i = 0; i < blend.Colors.Length; i++)
                {
                    var color = blend.Colors[i];
                    var position = blend.Positions[i];

                    gradient.Add(CreateStopElement(color, position));
                }
            }
            else
            {
                // Use LinearColors (simple two-color gradient)
                var colors = brush.LinearColors;
                if (colors.Length >= 2)
                {
                    gradient.Add(CreateStopElement(colors[0], 0));
                    gradient.Add(CreateStopElement(colors[1], 1));
                }
            }

            // Add gradient transform if present
            if (brush.Transform != null && !brush.Transform.IsIdentity)
            {
                var matrix = brush.Transform;
                var elements = matrix.Elements;
                string transformValue = $"matrix({elements[0]:F6},{elements[1]:F6},{elements[2]:F6},{elements[3]:F6},{elements[4]:F6},{elements[5]:F6})";
                gradient.Add(new XAttribute("gradientTransform", transformValue));
            }

            // Add wrap mode (spreadMethod)
            string spreadMethod = brush.WrapMode switch
            {
                WrapMode.Tile => "repeat",
                WrapMode.TileFlipX => "repeat",
                WrapMode.TileFlipY => "repeat",
                WrapMode.TileFlipXY => "repeat",
                WrapMode.Clamp => "pad",
                _ => "pad"
            };
            gradient.Add(new XAttribute("spreadMethod", spreadMethod));

            return gradient;
        }
        private XElement CreateStopElement(Color color, float offset)
        {
            string colorHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            double opacity = color.A / 255.0;

            var stop = new XElement(_svgNs + "stop",
                new XAttribute("offset", $"{offset * 100:F2}%"),
                new XAttribute("stop-color", colorHex)
            );

            if (opacity < 1.0)
            {
                stop.Add(new XAttribute("stop-opacity", $"{opacity:F3}"));
            }

            return stop;
        }
        private (double x1, double y1, double x2, double y2) AngleToCoordinates(double angleDegrees)
        {
            // Normalize angle to 0-360
            angleDegrees = angleDegrees % 360;
            if (angleDegrees < 0) angleDegrees += 360;

            double angleRad = angleDegrees * Math.PI / 180;

            double x2 = Math.Cos(angleRad);
            double y2 = Math.Sin(angleRad);

            // Normalize to 0-1 range
            return (0.5 - x2 / 2, 0.5 - y2 / 2, 0.5 + x2 / 2, 0.5 + y2 / 2);
        }
        private void EnsureDefsElement()
        {
            var root = _svgDocument.Root;
            if (root == null)
                throw new InvalidOperationException("SVG document has no root element");

            var defs = root.Element(_svgNs + "defs");
            if (defs == null)
            {
                defs = new XElement(_svgNs + "defs");
                root.AddFirst(defs);
            }
        }
        #endregion
    }
}
