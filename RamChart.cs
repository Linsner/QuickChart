using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickChart
{
    public class RamChart : QuickChart2
    {
        private Timer timer;
        private ContinousSeries _series;
        private PerformanceCounter _ramCounter;
        private float _totalRam;

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);

        public RamChart(Color colorSeries, int numberOfValues, float timeStepInSeconds, string header)
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

        public RamChart(Brush seriesBrush, Pen seriesPen, Brush labelBrush, Pen gridPen, int numberOfValues, float timeStepInSeconds, Font font, string header)
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

            Font = headerFont;
            Text = header;

            XAxis = new XAxis(xMin, xMax, xSpacing, true, true, gridPen, labelBrush, "{0:;#s;0}", labelFont);
            YAxis = YAxis.PercentAxis(50, gridPen, labelBrush, labelFont, HorizontalAlignment.Right);
            
            _series = new ContinousSeries();
            _series.Brush = seriesBrush;
            _series.Pen = seriesPen;
            for (int i = -numberOfValues; i <= 0; i++)
            {
                _series.AddXY(i * timeStepInSeconds, 0);
            }
            base.AddSeries(_series);
            
            timer = new Timer();
            timer.Interval = (int)(timeStepInSeconds * 1000f);
            timer.Tick += (object sender, EventArgs e) => UpdateSeries();
            timer.Start();

            long systemMemory;
            GetPhysicallyInstalledSystemMemory(out systemMemory);

            _totalRam = systemMemory / 1024f;

            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }

        private void UpdateSeries()
        {
            float freeRam = _ramCounter.NextValue();
            float usedRam = _totalRam - freeRam;
            float percent = usedRam / _totalRam * 100f;

            _series.RemoveFirstYAndAddY(percent);
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
