using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickChart
{
    public abstract class Axis
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public float ScreenMin { get; set; }
        public float ScreenMax { get; set; }
        public float Scale => (ScreenMax - ScreenMin) / (Max - Min);
        public float LabelSpacing { get; set; }
        public bool PaintLabels { get; set; }
        protected float LabelMargin => Font.Size * 0.3f;
        public bool PaintGrid { get; set; }
        public Pen GridPen { get; set; }
        public Brush LabelBrush { get; set; }
        public string LabelFormatString { get; set; } = "";
        public Font Font { get; set; }

        protected Dictionary<float, string> _labels;

        protected Axis(float min, float max, float labelSpacing, bool paintLabels, bool paintGrid, Pen gridPen, Brush labelBrush, string labelFormatString, Font font)
        {
            Initialize(min, max, labelSpacing, paintLabels, paintGrid, gridPen, labelBrush, labelFormatString, font);
        }

        protected Axis(float min, float max, float labelSpacing, Pen gridPen, Brush labelBrush, string labelFormatString, Font font)
        {
            Initialize(min, max, labelSpacing, true, true, gridPen, labelBrush, labelFormatString, font);
        }

        protected Axis(float min, float max, string[] labels, float[] labelValues, Pen gridPen, Brush labelBrush, Font font)
        {
            Initialize(min, max, 0, true, true, gridPen, labelBrush, "", font);
            for (int i = 0; i < labels.Length && i < labelValues.Length; i++)
            {
                AddLabel(labelValues[i], labels[i]);
            }
        }

        protected Axis(float min, float max)
        {
            Initialize(min, max, 0, false, false, null, null, null, null);
        }

        private void Initialize(float min, float max, float labelSpacing, bool paintLabels, bool paintGrid, Pen gridPen, Brush labelBrush, string labelFormatString, Font font)
        {
            if (min >= max)
                throw new ArgumentException(nameof(min));
            if (labelSpacing < 0)
                throw new ArgumentException(nameof(labelSpacing));

            Min = min;
            Max = max;
            LabelSpacing = labelSpacing;
            PaintLabels = paintLabels;
            PaintGrid = paintGrid;
            if (PaintGrid)
                GridPen = gridPen ?? throw new ArgumentNullException(nameof(gridPen));
            if (PaintLabels)
            {
                LabelBrush = labelBrush ?? throw new ArgumentNullException(nameof(labelBrush));
                LabelFormatString = labelFormatString ?? throw new ArgumentNullException(nameof(labelFormatString));
                Font = font ?? throw new ArgumentNullException(nameof(font));
            }
        }

        public void AddLabel(float value, string label)
        {
            if (_labels == null)
                _labels = new Dictionary<float, string>();
            _labels.Add(value, label);
        }

        public void ClearLabels()
        {
            _labels = null;
        }

        protected Dictionary<float, string> GenerateLabels()
        {
            Dictionary<float, string> labels = new Dictionary<float, string>();

            float start = Math.Abs(Min % LabelSpacing) + Min;

            for (float f = start; f <= Max; f += LabelSpacing)
            {
                string text = string.Format(LabelFormatString, f);
                labels.Add(f, text);
            }

            return labels;
        }

        public abstract void Paint(Graphics g, RectangleF chartArea);
    }

    public class XAxis : Axis
    {
        public XAxis(float min, float max, float labelSpacing, bool paintLabels, bool paintGrid, Pen gridPen, Brush labelBrush, string labelFormatString, Font font) :
            base(min, max, labelSpacing, paintLabels, paintGrid, gridPen, labelBrush, labelFormatString, font)
        { }

        protected XAxis(float min, float max, float labelSpacing, Pen gridPen, Brush labelBrush, string labelFormatString, Font font) :
            base(min, max, labelSpacing, gridPen, labelBrush, labelFormatString, font)
        { }

        protected XAxis(float min, float max, string[] labels, float[] labelValues, Pen gridPen, Brush labelBrush, Font font) :
            base(min, max, labels, labelValues, gridPen, labelBrush, font)
        { }

        protected XAxis(float min, float max) : base(min, max) { }

        public override void Paint(Graphics g, RectangleF chartArea)
        {
            ScreenMin = chartArea.Left;
            ScreenMax = chartArea.Right;

            PaintGridAndLabels(g, chartArea, _labels ?? GenerateLabels());
        }

        private void PaintGridAndLabels(Graphics g, RectangleF chartArea, Dictionary<float, string> labels)
        {
            foreach (var label in labels)
            {
                float x = GetXOnScreen(label.Key);

                if (PaintGrid)
                    g.DrawLine(GridPen, x, chartArea.Top, x, chartArea.Bottom);

                if (PaintLabels)
                    PaintLabel(g, label.Value, x, chartArea.Bottom);
            }
        }

        private void PaintLabel(Graphics g, string label, float xOnScreen, float yOnScreen)
        {
            SizeF sizeOfText = g.MeasureString(label, Font);

            // position of text on screen
            float xText = xOnScreen - sizeOfText.Width / 2;
            float yText = yOnScreen + LabelMargin;

            // align text with chart borders
            xText = Math.Max(xText, ScreenMin);
            xText = Math.Min(xText, ScreenMax - sizeOfText.Width);

            g.DrawString(label, Font, LabelBrush, xText, yText);
        }

        internal float GetXOnScreen(float x)
        {
            return ScreenMin + (x - Min) * Scale;
        }

        public float GetHeightOnScreen(Graphics g)
        {
            if (!PaintLabels)
                return 0;

            float maxHeight = 0;

            foreach (var label in _labels ?? GenerateLabels())
            {
                SizeF sizeOfText = g.MeasureString(label.Value, Font);
                maxHeight = Math.Max(maxHeight, sizeOfText.Height + LabelMargin);
            }

            return maxHeight;
        }

        public static XAxis TrigonometryAxis(Pen gridPen, Brush labelBrush, Font font)
        {
            return new XAxis(0, (float)(Math.PI * 2), new string[] { "0", "π", "2π" }, new float[] { 0, (float)Math.PI, (float)(Math.PI * 2) }, gridPen, labelBrush, font);
        }
    }

    public class YAxis : Axis
    {
        public HorizontalAlignment HorizontalAlignment { get; set; }

        public YAxis(float min, float max, float labelSpacing, bool paintLabels, bool paintGrid, Pen gridPen, Brush labelBrush, string labelFormatString, Font font, HorizontalAlignment horizontalAlignment) :
            base(min, max, labelSpacing, paintLabels, paintGrid, gridPen, labelBrush, labelFormatString, font)
        {
            HorizontalAlignment = horizontalAlignment;
        }

        protected YAxis(float min, float max, float labelSpacing, Pen gridPen, Brush labelBrush, string labelFormatString, Font font, HorizontalAlignment horizontalAlignment) :
            base(min, max, labelSpacing, gridPen, labelBrush, labelFormatString, font)
        {
            HorizontalAlignment = horizontalAlignment;
        }

        protected YAxis(float min, float max, string[] labels, float[] labelValues, Pen gridPen, Brush labelBrush, Font font, HorizontalAlignment horizontalAlignment) :
            base(min, max, labels, labelValues, gridPen, labelBrush, font)
        {
            HorizontalAlignment = horizontalAlignment;
        }

        protected YAxis(float min, float max) : base(min, max) { }

        public override void Paint(Graphics g, RectangleF chartArea)
        {
            ScreenMin = chartArea.Top;
            ScreenMax = chartArea.Bottom;

            PaintGridAndLabels(g, chartArea, _labels ?? GenerateLabels());
        }

        private void PaintGridAndLabels(Graphics g, RectangleF chartArea, Dictionary<float, string> labels)
        {
            foreach (var label in labels)
            {
                float y = GetYOnScreen(label.Key);

                if (PaintGrid)
                    g.DrawLine(GridPen, chartArea.Left, y, chartArea.Right, y);

                if (PaintLabels)
                {
                    float x = HorizontalAlignment == HorizontalAlignment.Left ? chartArea.Left : chartArea.Right;
                    PaintLabel(g, label.Value, x, y);
                }
            }
        }

        private void PaintLabel(Graphics g, string label, float xOnScreen, float yOnScreen)
        {
            SizeF sizeOfText = g.MeasureString(label, Font);

            // position of text on screen
            float xText =
                HorizontalAlignment == HorizontalAlignment.Left ? xOnScreen - sizeOfText.Width - LabelMargin :
                xOnScreen + LabelMargin; // HorizontalAlignment == HorizontalAlignment.Right
            float yText = yOnScreen - sizeOfText.Height / 2;

            // align text with chart borders
            yText = Math.Max(yText, ScreenMin);
            yText = Math.Min(yText, ScreenMax - sizeOfText.Height);

            g.DrawString(label, Font, LabelBrush, xText, yText);
        }

        internal float GetYOnScreen(float y)
        {
            return ScreenMax - (y - Min) * Scale;
        }

        public float GetWidthOnScreen(Graphics g)
        {
            if (!PaintLabels)
                return 0;

            float maxWidth = 0;

            foreach (var label in _labels ?? GenerateLabels())
            {
                SizeF sizeOfText = g.MeasureString(label.Value, Font);
                maxWidth = Math.Max(maxWidth, sizeOfText.Width + LabelMargin);
            }

            return maxWidth;
        }

        public static YAxis TrigonometryAxis(Pen gridPen, Brush labelBrush, Font font, HorizontalAlignment horizontalAlignment)
        {
            return new YAxis(-1, 1, new string[] { "-1", "0", "1" }, new float[] { -1, 0, 1 }, gridPen, labelBrush, font, horizontalAlignment);
        }

        public static YAxis PercentAxis(float labelSpacing, Pen gridPen, Brush labelBrush, Font font, HorizontalAlignment horizontalAlignment)
        {
            return new YAxis(0, 100, labelSpacing, true, true, gridPen, labelBrush, "{0:0.#\\%;;0}", font, horizontalAlignment);
        }
    }
}
