using Microsoft.UI.Xaml.Media;
using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Windows.UI;

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
            // Get start and end points from the brush
            double x1 = brush.StartPoint.X;
            double y1 = brush.StartPoint.Y;
            double x2 = brush.EndPoint.X;
            double y2 = brush.EndPoint.Y;

            var gradient = new XElement(_svgNs + "linearGradient",
                new XAttribute("id", id),
                new XAttribute("x1", $"{x1:F2}"),
                new XAttribute("y1", $"{y1:F2}"),
                new XAttribute("x2", $"{x2:F2}"),
                new XAttribute("y2", $"{y2:F2}")
            );

            // Add gradient stops from the GradientStops collection
            if (brush.GradientStops != null)
            {
                foreach (var stop in brush.GradientStops)
                {
                    gradient.Add(CreateStopElement(stop));
                }
            }

            // Add relative/absolute mapping mode
            if (brush.MappingMode == BrushMappingMode.Absolute)
            {
                gradient.Add(new XAttribute("gradientUnits", "userSpaceOnUse"));
            }
            else
            {
                gradient.Add(new XAttribute("gradientUnits", "objectBoundingBox"));
            }

            // Add transform if present
            if (brush.Transform != null)
            {
                var transform = brush.Transform;
                if (transform is MatrixTransform matrixTransform)
                {
                    var matrix = matrixTransform.Matrix;
                    string transformValue = $"matrix({matrix.M11:F6},{matrix.M12:F6},{matrix.M21:F6},{matrix.M22:F6},{matrix.OffsetX:F6},{matrix.OffsetY:F6})";
                    gradient.Add(new XAttribute("gradientTransform", transformValue));
                }
                else if (transform is RotateTransform rotateTransform)
                {
                    string transformValue = $"rotate({rotateTransform.Angle:F2} {rotateTransform.CenterX:F2} {rotateTransform.CenterY:F2})";
                    gradient.Add(new XAttribute("gradientTransform", transformValue));
                }
                else if (transform is ScaleTransform scaleTransform)
                {
                    string transformValue = $"scale({scaleTransform.ScaleX:F6},{scaleTransform.ScaleY:F6})";
                    gradient.Add(new XAttribute("gradientTransform", transformValue));
                }
                else if (transform is SkewTransform skewTransform)
                {
                    string transformValue = $"skewX({skewTransform.AngleX:F2}) skewY({skewTransform.AngleY:F2})";
                    gradient.Add(new XAttribute("gradientTransform", transformValue));
                }
                else if (transform is TranslateTransform translateTransform)
                {
                    string transformValue = $"translate({translateTransform.X:F6},{translateTransform.Y:F6})";
                    gradient.Add(new XAttribute("gradientTransform", transformValue));
                }
                else if (transform is TransformGroup transformGroup)
                {
                    var transforms = new List<string>();
                    foreach (var t in transformGroup.Children)
                    {
                        if (t is MatrixTransform mt)
                        {
                            var m = mt.Matrix;
                            transforms.Add($"matrix({m.M11:F6},{m.M12:F6},{m.M21:F6},{m.M22:F6},{m.OffsetX:F6},{m.OffsetY:F6})");
                        }
                        else if (t is RotateTransform rt)
                            transforms.Add($"rotate({rt.Angle:F2} {rt.CenterX:F2} {rt.CenterY:F2})");
                        else if (t is ScaleTransform st)
                            transforms.Add($"scale({st.ScaleX:F6},{st.ScaleY:F6})");
                        else if (t is SkewTransform skt)
                            transforms.Add($"skewX({skt.AngleX:F2}) skewY({skt.AngleY:F2})");
                        else if (t is TranslateTransform tt)
                            transforms.Add($"translate({tt.X:F6},{tt.Y:F6})");
                    }
                    if (transforms.Count > 0)
                    {
                        gradient.Add(new XAttribute("gradientTransform", string.Join(" ", transforms)));
                    }
                }
            }

            // Add spread method
            string spreadMethod = brush.SpreadMethod switch
            {
                GradientSpreadMethod.Pad => "pad",
                GradientSpreadMethod.Reflect => "reflect",
                GradientSpreadMethod.Repeat => "repeat",
                _ => "pad"
            };
            gradient.Add(new XAttribute("spreadMethod", spreadMethod));

            // Add color interpolation mode
            if (brush.ColorInterpolationMode == ColorInterpolationMode.ScRgbLinearInterpolation)
            {
                gradient.Add(new XAttribute("color-interpolation", "linearRGB"));
            }

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
        private XElement CreateStopElement(GradientStop stop)
        {
            var color = stop.Color;
            string colorHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            double opacity = color.A / 255.0;

            var stopElement = new XElement(_svgNs + "stop",
                new XAttribute("offset", $"{stop.Offset * 100:F2}%"),
                new XAttribute("stop-color", colorHex)
            );

            if (opacity < 1.0)
            {
                stopElement.Add(new XAttribute("stop-opacity", $"{opacity:F3}"));
            }

            return stopElement;
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
