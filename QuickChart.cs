using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickChart
{
    public class QuickChart : Control
    {
        private List<Series> _series = new List<Series>();
        private float _xScale;
        private float _yScale;
        private float _xMid;
        private float _yMid;
        private RectangleF _chartArea;

        public bool EnableXAutoZoom { get; set; } = true;

        public bool EnableYAutoZoom { get; set; } = false;

        public float MaxYValue { get; set; } = 1;

        public float MinYValue { get; set; } = 0;

        public float MaxXValue { get; set; } = 1;

        public float MinXValue { get; set; } = 0;

        public string Header { get; set; } = "";

        public string YLabelFormatString { get; set; } = "";

        public string XLabelFormatString { get; set; } = "";

        public Font HeaderFont { get; set; }

        public Pen ChartBorderPen { get; set; } = new Pen(Color.LightGray);

        public Brush ChartFillBrush { get; set; } = Brushes.White;

        public Padding ChartPadding { get; set; } = new Padding(30);

        public QuickChart()
        {
            HeaderFont = new Font(Font, FontStyle.Bold);
            DoubleBuffered = true;
        }

        public virtual void AddSeries(Series series)
        {
            _series.Add(series);
        }

        public virtual void ClearSeries()
        {
            _series.Clear();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            UpdateChartParameters();

            Graphics g = e.Graphics;
            Brush foreBrush = new SolidBrush(ForeColor);
            Pen chartBorderPen = ChartBorderPen ?? Pens.Black;
            Brush chartFillBrush = ChartFillBrush ?? Brushes.White;

            g.SmoothingMode = SmoothingMode.HighSpeed;
            PaintHeader(g, foreBrush);

            g.FillRectangle(chartFillBrush, _chartArea);

            PaintYLabel(g, MinYValue, foreBrush, chartBorderPen);
            PaintYLabel(g, MaxYValue, foreBrush, chartBorderPen);
            PaintYLabel(g, _yMid, foreBrush, chartBorderPen);

            PaintXLabel(g, MinXValue, foreBrush, chartBorderPen, HorizontalAlignment.Left);
            PaintXLabel(g, MaxXValue, foreBrush, chartBorderPen, HorizontalAlignment.Right);
            PaintXLabel(g, _xMid, foreBrush, chartBorderPen, HorizontalAlignment.Center);

            _series.Sort((x, y) => x.Average.CompareTo(y.Average));

            g.SmoothingMode = SmoothingMode.AntiAlias;
            //_series.ForEach(x => x.Paint(g, this));

            //g.SmoothingMode = SmoothingMode.Default;
            //g.DrawRectangle(ChartBorderPen, _chartArea.X, _chartArea.Y, _chartArea.Width, _chartArea.Height);

        }

        private void PaintXLabel(Graphics g, float xValue, Brush textBrush, Pen chartBorderPen, HorizontalAlignment alignment)
        {
            if (XLabelFormatString == null)
                return;

            string text = string.Format(XLabelFormatString, xValue);
            SizeF sizeOfText = g.MeasureString(text, Font);
            float x = GetXOnScreen(xValue);
            float xText = 
                alignment == HorizontalAlignment.Center ? x - sizeOfText.Width / 2f :
                alignment == HorizontalAlignment.Left ? x :
                x - sizeOfText.Width; // alignment == HorizontalAlignment.Right
            float yTop = _chartArea.Top;
            float yBottom = _chartArea.Bottom;

            g.DrawLine(chartBorderPen, x, yTop, x, yBottom);
            g.DrawString(text, Font, textBrush, xText, yBottom);
        }

        private void PaintYLabel(Graphics g, float yValue, Brush textBrush, Pen chartBorderPen)
        {
            if (YLabelFormatString == null)
                return;

            string text = string.Format(YLabelFormatString, yValue);
            SizeF sizeOfText = g.MeasureString(text, Font);
            float xLeft = _chartArea.Left;
            float xRight = _chartArea.Right;
            float y = GetYOnScreen(yValue);
            float yText = y - sizeOfText.Height / 2f;

            if (ChartBorderPen != null)
                g.DrawLine(ChartBorderPen, xLeft, y, xRight, y);

            g.DrawString(text, Font, textBrush, xLeft - sizeOfText.Width, yText);
            g.DrawString(text, Font, textBrush, xRight, yText);
        }

        private void UpdateChartParameters()
        {
            _chartArea = new RectangleF(
                ChartPadding.Left, 
                ChartPadding.Top, 
                Math.Max(Width - ChartPadding.Horizontal, 1), 
                Math.Max(Height - ChartPadding.Vertical, 1));

            FindXMaxAndMinValues();
            FindYMaxAndMinValues();

            float xRange = MaxXValue - MinXValue;
            _xScale = _chartArea.Width / xRange;
            _xMid = MinXValue + xRange / 2f;

            float yRange = MaxYValue - MinYValue;
            _yScale = _chartArea.Height / (yRange * 1.1f);
            _yMid = MinYValue + yRange / 2f;
        }

        private void PaintHeader(Graphics g, Brush foreBrush)
        {
            if (string.IsNullOrWhiteSpace(Header))
                return;

            g.DrawString(Header, HeaderFont, foreBrush, 0, 0);
        }

        private void FindYMaxAndMinValues()
        {
            if (EnableYAutoZoom || float.IsNaN(MinYValue) || float.IsNaN(MaxYValue))
            {
                MinYValue = _series.Count > 0 ? _series.Min(x => x.MinY) : 0;
                MaxYValue = _series.Count > 0 ? _series.Max(x => x.MaxY) : 1;
            }

            if (MinYValue == MaxYValue)
            {
                MaxYValue = MinYValue + 1;
            }
            else if (MinYValue > MaxYValue)
            {
                MinYValue = 0;
                MaxYValue = 1;
            }
        }

        private void FindXMaxAndMinValues()
        {
            if (EnableXAutoZoom || float.IsNaN(MinXValue) || float.IsNaN(MaxXValue))
            {
                MinXValue = _series.Count > 0 ?_series.Min(x => x.MinX) : 0;
                MaxXValue = _series.Count > 0 ? _series.Max(x => x.MaxX) : 1;
            }

            if (MinXValue == MaxXValue)
            {
                MaxXValue = MinXValue + 1;
            }
            else if (MinXValue > MaxXValue)
            {
                MinXValue = 0;
                MaxXValue = 1;
            }
        }

        internal float GetYOnScreen(float y)
        {
            float chartCenter = _chartArea.Top + _chartArea.Height / 2f;
            return chartCenter + (_yMid - y) * _yScale;
        }

        internal float GetXOnScreen(float x)
        {
            float chartCenter = _chartArea.Left + _chartArea.Width / 2f;
            return chartCenter - (_xMid - x) * _xScale;
        }
    }
}
