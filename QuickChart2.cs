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
    public class QuickChart2 : Control
    {
        private List<Series> _series = new List<Series>();

        public RectangleF ChartArea
        {
            get
            {
                return new RectangleF(
                  Padding.Left,
                  Padding.Top,
                  Width - Padding.Horizontal - 1,
                  Height - Padding.Vertical - 1);
            }
        }

        public XAxis XAxis { get; set; }

        public YAxis YAxis { get; set; }
        
        public QuickChart2(XAxis xAxis, YAxis yAxis)
        {
            XAxis = xAxis;
            YAxis = yAxis;
            DoubleBuffered = true;
        }

        protected QuickChart2()
        {

        }

        public virtual void AddSeries(Series series)
        {
            _series.Add(series);
        }

        public virtual void ClearSeries()
        {
            _series.Clear();
        }

        public void AutoZoomY()
        {
            YAxis.Min = _series.Count > 0 ? _series.Min(x => x.MinY) : 0;
            YAxis.Max = _series.Count > 0 ? _series.Max(x => x.MaxY) : 1;
            if (YAxis.Min == YAxis.Max)
                YAxis.Max += 1;
        }

        public void AutoZoomX()
        {
            XAxis.Min = _series.Count > 0 ? _series.Min(x => x.MinX) : 0;
            XAxis.Max = _series.Count > 0 ? _series.Max(x => x.MaxX) : 1;
            if (XAxis.Min == XAxis.Max)
                XAxis.Max += 1;
        }

        internal float GetYOnScreen(float y)
        {
            return YAxis.GetYOnScreen(y);
        }

        internal float GetXOnScreen(float x)
        {
            return XAxis.GetXOnScreen(x);
        }

        private void MeasurePadding()
        {
            Graphics g = Graphics.FromHwnd(Handle);
            
            float paddingLeft = YAxis.HorizontalAlignment == HorizontalAlignment.Left ? YAxis.GetWidthOnScreen(g) : 0;
            float paddingRight = YAxis.HorizontalAlignment == HorizontalAlignment.Right ? YAxis.GetWidthOnScreen(g) : 0;
            float paddingTop = MeasureHeader(g).Height;
            float paddingBottom = XAxis.GetHeightOnScreen(g);

            Padding = new Padding((int)paddingLeft, (int)paddingTop, (int)paddingRight, (int)paddingBottom);
        }

        private SizeF MeasureHeader(Graphics g)
        {
            if (string.IsNullOrWhiteSpace(Text))
                return new SizeF();
            return g.MeasureString(Text, Font);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        #region Paint 

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            MeasurePadding();

            PaintHeader(e.Graphics);

            YAxis.Paint(e.Graphics, ChartArea);
            XAxis.Paint(e.Graphics, ChartArea);

            PaintSeries(e.Graphics);
        }

        private void PaintSeries(Graphics g)
        {
            _series.Sort((x, y) => -x.Average.CompareTo(y.Average));

            g.SmoothingMode = SmoothingMode.AntiAlias;

            _series.ForEach(x => x.Paint(g, this));
        }

        private void PaintHeader(Graphics g)
        {
            if (string.IsNullOrWhiteSpace(Text))
                return;
            g.DrawString(Text, Font, new SolidBrush(ForeColor), 0, 0);
        }

        #endregion
    }
}
