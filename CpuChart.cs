using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QuickChart
{
    public class CpuChart : QuickChart2
    {
        private Timer _timer;
        private ContinousSeries _cpuSeries;
        private PerformanceCounter _cpuCounter;

        public CpuChart(Color colorSeries, int numberOfValues, float timeStepInSeconds, string header)
        {
            Initialize(
                new SolidBrush(Color.FromArgb(125, colorSeries)),
                new Pen(colorSeries, 2),
                new SolidBrush(Color.Black),
                new Pen(Color.LightGray), 
                numberOfValues, 
                timeStepInSeconds, 
                Font, 
                header);
        }

        public CpuChart(Brush seriesBrush, Pen seriesPen, Brush labelBrush, Pen gridPen, int numberOfValues, float timeStepInSeconds, Font font, string header)
        {
            Initialize(seriesBrush, seriesPen, labelBrush, gridPen, numberOfValues, timeStepInSeconds, font, header);
        }

        private void Initialize(Brush seriesBrush, Pen seriesPen, Brush labelBrush, Pen gridPen, int numberOfValues, float timeStepInSeconds, Font font, string header)
        {
            Font headerFont = new Font(font, FontStyle.Bold);
            Font labelFont = font;

            float xMin = -numberOfValues * timeStepInSeconds;
            float xMax = 0;
            float xSpacing = (float)Math.Round(Math.Abs(xMin / 3));
            float yMin = 0;
            float yMax = 100;
            float ySpacing = 50;

            Font = headerFont;
            Text = header;
            XAxis = new XAxis(xMin, xMax, xSpacing, true, true, gridPen, labelBrush, "{0:;#s;0}", labelFont);
            YAxis = new YAxis(yMin, yMax, ySpacing, true, true, gridPen, labelBrush, "{0:0.#\\%;;0}", labelFont, HorizontalAlignment.Right);
            
            _cpuSeries = new ContinousSeries();
            _cpuSeries.Brush = seriesBrush;
            _cpuSeries.Pen = seriesPen;
            for (int i = -numberOfValues; i <= 0; i++)
            {
                _cpuSeries.AddXY(i * timeStepInSeconds, 0);
            }
            base.AddSeries(_cpuSeries);

            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = (int)(timeStepInSeconds * 1000f);
            _timer.Tick += (object sender, EventArgs e) => UpdateSeries();
            _timer.Start();
        }

        private void UpdateSeries()
        {
            float value = _cpuCounter.NextValue();
            _cpuSeries.RemoveFirstYAndAddY(value);
            Invalidate();
        }

        public override void AddSeries(Series series)
        {
            throw new InvalidOperationException();
        }

        public override void ClearSeries()
        {
            throw new InvalidOperationException();
        }
    }
}