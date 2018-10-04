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
        public bool PaintGrid { get; set; }
        public Pen GridPen { get; set; }
        public Brush LabelBrush { get; set; }
        public string LabelFormatString { get; set; } = "";
        public Font Font { get; set; }

        protected Axis(float min, float max, float labelSpacing, bool paintLabels, bool paintGrid, Pen gridPen, Brush labelBrush, string labelFormatString, Font font)
        {
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
        
        //protected Axis()
        //{
        //    LabelSpacing = float.NaN;
        //    PaintLabels = true;
        //    PaintGrid = true;
        //}
    }

    public class XAxis : Axis
    {
        public string Labels { get; set; }

        public XAxis(float min, float max, float labelSpacing, bool paintLabels, bool paintGrid, Pen gridPen, Brush labelBrush, string labelFormatString, Font font) :
            base(min, max, labelSpacing, paintLabels, paintGrid, gridPen, labelBrush, labelFormatString, font)
        {
        }
        
        public void Paint(Graphics g, RectangleF chartArea)
        {
            ScreenMin = chartArea.Left;
            ScreenMax = chartArea.Right;

            float y1 = chartArea.Top;
            float y2 = chartArea.Bottom;

            float start = Math.Abs(Min % LabelSpacing) + Min;

            for (float f = start; f <= Max; f += LabelSpacing)
            {
                float x = GetXOnScreen(f);

                if (PaintGrid)
                    g.DrawLine(GridPen, x, y1, x, y2);

                if (PaintLabels)
                {
                    string text = string.Format(LabelFormatString, f);
                    SizeF sizeOfText = g.MeasureString(text, Font);
                    float xText = x - sizeOfText.Width / 2;
                    float yText = y2 + Font.Size * 0.3f;
                    xText = Math.Max(xText, ScreenMin);
                    xText = Math.Min(xText, ScreenMax - sizeOfText.Width);

                    //if (xText > ScreenMin && xText + sizeOfText.Width < ScreenMax)
                    g.DrawString(text, Font, LabelBrush, xText, yText);
                }
            }
        }

        internal float GetXOnScreen(float x)
        {
            return ScreenMin + (x - Min) * Scale;
        }

        public float GetHeightOnScreen(Graphics g)
        {
            if (!PaintLabels)
                return 0;

            float start = Math.Abs(Min % LabelSpacing) + Min;
            float maxHeight = 0;

            for (float f = start; f <= Max; f += LabelSpacing)
            {
                string text = string.Format(LabelFormatString, f);
                SizeF sizeOfText = g.MeasureString(text, Font);
                maxHeight = Math.Max(maxHeight, sizeOfText.Height + Font.Size * 0.3f);
            }

            return maxHeight;
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
        
        public void Paint(Graphics g, RectangleF chartArea)
        {
            ScreenMin = chartArea.Top;
            ScreenMax = chartArea.Bottom;

            float x1 = chartArea.Left;
            float x2 = chartArea.Right;

            float start = Math.Abs(Min % LabelSpacing) + Min;

            for (float f = start; f <= Max; f += LabelSpacing)
            {
                float y = GetYOnScreen(f);

                if (PaintGrid)
                    g.DrawLine(GridPen, x1, y, x2, y);  //draw grid

                if (PaintLabels)
                {
                    string text = string.Format(LabelFormatString, f);
                    SizeF sizeOfText = g.MeasureString(text, Font);
                    float xText =
                        HorizontalAlignment == HorizontalAlignment.Left ? x1 - sizeOfText.Width - Font.Size * 0.3f :
                        HorizontalAlignment == HorizontalAlignment.Right ? x2 + Font.Size * 0.3f :
                        throw new ArgumentException();
                    float yText = y - sizeOfText.Height / 2;
                    yText = Math.Max(yText, ScreenMin);
                    yText = Math.Min(yText, ScreenMax - sizeOfText.Height);

                    //if (yText > ScreenMin && yText + sizeOfText.Height < ScreenMax)
                    g.DrawString(text, Font, LabelBrush, xText, yText);
                }
            }
        }

        internal float GetYOnScreen(float y)
        {
            return ScreenMax - (y - Min) * Scale;
        }

        public float GetWidthOnScreen(Graphics g)
        {
            if (!PaintLabels)
                return 0;

            float start = Math.Abs(Min % LabelSpacing) + Min;
            float maxWidth = 0;

            for (float f = start; f <= Max; f += LabelSpacing)
            {
                string text = string.Format(LabelFormatString, f);
                SizeF sizeOfText = g.MeasureString(text, Font);
                maxWidth = Math.Max(maxWidth, sizeOfText.Width + Font.Size * 0.3f);
            }

            return maxWidth;
        }
    }
}
