using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace CIMViewer.Helpers.AvalonEdit {

    /// <summary>
    /// Highlight XML Validation errors in the AvalonEdit TextEdit control
    /// </summary>
    /// <remarks>From <a href="http://stackoverflow.com/questions/11149907/showing-invalid-xml-syntax-with-avalonedit"/>
    /// </remarks>
    internal class TextMarkerService : IBackgroundRenderer, IVisualLineTransformer {
        private readonly TextEditor _textEditor;
        private readonly TextSegmentCollection<TextMarker> _textMarkers;

        public sealed class TextMarker : TextSegment {
            public TextMarker(int startOffset, int length) {
                StartOffset = startOffset;
                Length = length;
            }

            public Color? BackgroundColor { get; set; }
            public Color MarkerColor { get; set; }
            public string ToolTip { get; set; }
        }

        public TextMarkerService(TextEditor textEditor) {
            this._textEditor = textEditor;
            _textMarkers = new TextSegmentCollection<TextMarker>(textEditor.Document);
        }

        public void Draw(TextView textView, DrawingContext drawingContext) {
            if (_textMarkers == null || !textView.VisualLinesValid) {
                return;
            }
            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0) {
                return;
            }
            int viewStart = visualLines.First().FirstDocumentLine.Offset;
            int viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
            foreach (TextMarker marker in _textMarkers.FindOverlappingSegments(viewStart, viewEnd - viewStart)) {
                if (marker.BackgroundColor != null) {
                    var geoBuilder = new BackgroundGeometryBuilder { AlignToWholePixels = true, CornerRadius = 3 };
                    geoBuilder.AddSegment(textView, marker);
                    Geometry geometry = geoBuilder.CreateGeometry();
                    if (geometry != null) {
                        Color color = marker.BackgroundColor.Value;
                        var brush = new SolidColorBrush(color);
                        brush.Freeze();
                        drawingContext.DrawGeometry(brush, null, geometry);
                    }
                }
                foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker)) {
                    Point startPoint = r.BottomLeft;
                    Point endPoint = r.BottomRight;

                    var usedPen = new Pen(new SolidColorBrush(marker.MarkerColor), 1);
                    usedPen.Freeze();
                    const double offset = 2.5;

                    int count = Math.Max((int)((endPoint.X - startPoint.X) / offset) + 1, 4);

                    var geometry = new StreamGeometry();

                    using (StreamGeometryContext ctx = geometry.Open()) {
                        ctx.BeginFigure(startPoint, false, false);
                        ctx.PolyLineTo(CreatePoints(startPoint, endPoint, offset, count).ToArray(), true, false);
                    }

                    geometry.Freeze();

                    drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
                    break;
                }
            }
        }

        public KnownLayer Layer {
            get { return KnownLayer.Selection; }
        }

        public void Transform(ITextRunConstructionContext context, IList<VisualLineElement> elements) { }

        private IEnumerable<Point> CreatePoints(Point start, Point end, double offset, int count) {
            for (int i = 0; i < count; i++) {
                yield return new Point(start.X + (i * offset), start.Y - ((i + 1) % 2 == 0 ? offset : 0));
            }
        }

        public void Clear() {
            foreach (TextMarker m in _textMarkers) {
                Remove(m);
            }
        }

        private void Remove(TextMarker marker) {
            if (_textMarkers.Remove(marker)) {
                Redraw(marker);
            }
        }

        private void Redraw(ISegment segment) {
            _textEditor.TextArea.TextView.Redraw(segment);
        }

        public void Create(int offset, int length, string message) {
            var m = new TextMarker(offset, length);
            _textMarkers.Add(m);
            m.MarkerColor = Colors.Red;
            m.ToolTip = message;
            Redraw(m);
        }

        public IEnumerable<TextMarker> GetMarkersAtOffset(int offset) {
            return _textMarkers == null ? Enumerable.Empty<TextMarker>() : _textMarkers.FindSegmentsContaining(offset);
        }
    }
}
