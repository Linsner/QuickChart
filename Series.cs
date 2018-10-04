using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickChart
{
    public class Series
    {
        protected List<float> _yValues = new List<float>();
        protected List<float> _xValues = new List<float>();
        private float _average;

        public float Average
        {
            get
            {
                if (float.IsNaN(_average))
                    _average = CalcAverage();
                return _average;
            }
            private set => _average = value;
        }

        public float MinX { get; private set; } = float.MaxValue;

        public float MaxX { get; private set; } = float.MinValue;

        public float MinY { get; private set; } = float.MaxValue;

        public float MaxY { get; private set; } = float.MinValue;

        public Pen Pen { get; set; } = new Pen(Color.FromArgb(0, 123, 255));

        public Brush Brush { get; set; } = new SolidBrush(Color.FromArgb(0, 123, 255));

        public Series(Pen pen, Brush brush)
        {
            Pen = pen;
            Brush = brush;
            Clear();
        }

        public Series()
        {
            Clear();
        }

        public void AddY(float y)
        {
            _yValues.Add(y);
            _average = float.NaN;
            CalcMinMaxY(y);
        }

        public void AddXY(float x, float y)
        {
            AddY(y);
            _xValues.Add(x);
            CalcMinMaxX(x);
        }

        public void Remove(int index, bool calculateParameters)
        {
            if (_yValues.Count > index)
            {
                _yValues.RemoveAt(index);
                if (calculateParameters)
                    CalcMinMaxY();
            }

            if (_xValues.Count > index)
            {
                _xValues.RemoveAt(index);
                if (calculateParameters)
                    CalcMinMaxX();
            }

            MarkAverageAsOutdated();
        }

        public void AddRange(float[] y)
        {
            _yValues.AddRange(y);
            MarkAverageAsOutdated();

            foreach (var item in y)
            {
                CalcMinMaxY(item);
            }
        }

        public void AddRange(float[] x, float[] y)
        {
            AddRange(y);

            _xValues.AddRange(x);

            foreach (var item in x)
            {
                CalcMinMaxX(item);
            }
        }

        public void Clear()
        {
            _xValues.Clear();
            _yValues.Clear();
            MinX = float.MaxValue;
            MaxX = float.MinValue;
            MinY = float.MaxValue;
            MaxY = float.MinValue;
        }

        public void Paint(Graphics g, QuickChart2 quickChart)
        {
            PointF[] line = GetLine(quickChart);

            if (Brush != null)
            {
                PointF[] area = GetArea(quickChart, line);
                g.FillPolygon(Brush, area);
            }
            if (Pen != null)
                g.DrawLines(Pen, line);
        }

        private PointF[] GetLine(QuickChart2 quickChart)
        {
            PointF[] pts = new PointF[_yValues.Count];
            for (int i = 0; i < _yValues.Count && (_xValues == null || i < _xValues.Count); i++)
            {
                pts[i] = new PointF(
                    quickChart.GetXOnScreen(_xValues == null ? i :_xValues[i]),
                    quickChart.GetYOnScreen(_yValues[i]));
            }
            return pts;
        }

        private PointF[] GetArea(QuickChart2 quickChart, PointF[] line)
        {
            int length = line.Length;
            PointF[] area = new PointF[length + 2];
            Array.Copy(line, area, length);

            float baseLineY =
                quickChart.YAxis.Min > 0 ? quickChart.YAxis.Min :
                quickChart.YAxis.Max < 0 ? quickChart.YAxis.Max :
                0;
            float baseLineYOnScreen = quickChart.GetYOnScreen(baseLineY);

            area[length] = new PointF(line[length - 1].X, baseLineYOnScreen);
            area[length + 1] = new PointF(line[0].X, baseLineYOnScreen);
            return area;
        }

        protected float CalcAverage()
        {
            return _yValues.Sum() / _yValues.Count;
        }

        protected void CalcMinMaxY()
        {
            MinY = float.MaxValue;
            MaxY = float.MinValue;
            foreach (var y in _yValues)
            {
                CalcMinMaxY(y);
            }
        }

        protected void CalcMinMaxX()
        {
            MinX = float.MaxValue;
            MaxX = float.MinValue;
            foreach (var x in _xValues)
            {
                CalcMinMaxX(x);
            }
        }

        protected void CalcMinMaxY(float addedY)
        {
            MinY = Math.Min(MinY, addedY);
            MaxY = Math.Max(MaxY, addedY);
        }

        protected void CalcMinMaxX(float addedX)
        {
            MinX = Math.Min(MinX, addedX);
            MaxX = Math.Max(MaxX, addedX);
        }

        protected void MarkAverageAsOutdated()
        {
            _average = float.NaN;
        }
    }
}




//public float GetMinY()
//{
//    float min = float.MaxValue;
//    foreach (var item in _yValues)
//    {
//        min = Math.Min(min, (float)item);
//    }
//    return min;
//}

//public float GetMaxY()
//{
//    float max = float.MinValue;
//    foreach (var item in _yValues)
//    {
//        max = Math.Max(max, (float)item);
//    }
//    return max;
//}

//public float GetMinX()
//{
//    if (_xValues == null)
//        return 0;

//    float min = float.MaxValue;
//    foreach (var item in _yValues)
//    {
//        min = Math.Min(min, (float)item);
//    }
//    return min;
//}

//public float GetMaxX()
//{
//    if (_xValues == null)
//        return _yValues.Count - 1;

//    float max = float.MinValue;
//    foreach (var item in _xValues)
//    {
//        max = Math.Max(max, (float)item);
//    }
//    return max;
//}