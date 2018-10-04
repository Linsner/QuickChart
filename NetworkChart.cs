using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickChart
{
    public class NetworkChart : QuickChart2
    {
        private Timer timer;
        private ContinousSeries _sentSeries;
        private ContinousSeries _recievedSeries;
        private NetworkInterface[] interfaces;
        private long _totalSent = 0;
        private long _totalRecieved = 0;

        public NetworkChart(Color sent, Color recieved, int numberOfValues, float timeStepInSeconds, string header)
        {
            Initialize(
                new SolidBrush(Color.FromArgb(125, sent)),
                new Pen(sent, 2),
                new SolidBrush(Color.FromArgb(125, recieved)),
                new Pen(recieved, 2),
                new SolidBrush(Color.Black),
                new Pen(Color.LightGray),
                numberOfValues,
                timeStepInSeconds,
                Font,
                header);
        }

        public NetworkChart(Brush sentBrush, Pen sentPen, Brush recievedBrush, Pen recievedPen, Brush labelBrush, Pen gridPen, int numberOfValues, float timeStepInSeconds, Font font, string header)
        {
            Initialize(sentBrush, sentPen, recievedBrush, recievedPen, labelBrush, gridPen, numberOfValues, timeStepInSeconds, font, header);
        }

        private void Initialize(Brush sentBrush, Pen sentPen, Brush recievedBrush, Pen recievedPen, Brush labelBrush, Pen gridPen, int numberOfValues, float timeStepInSeconds, Font font, string header)
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
            
            _sentSeries = new ContinousSeries();
            _sentSeries.Brush = sentBrush;
            _sentSeries.Pen = sentPen;
            for (int i = -numberOfValues; i <= 0; i++)
            {
                _sentSeries.AddXY(i * timeStepInSeconds, 0);
            }
            base.AddSeries(_sentSeries);

            _recievedSeries = new ContinousSeries();
            _recievedSeries.Brush = recievedBrush;
            _recievedSeries.Pen = recievedPen;
            for (int i = -numberOfValues; i <= 0; i++)
            {
                _recievedSeries.AddXY(i * timeStepInSeconds, 0);
            }
            base.AddSeries(_recievedSeries);

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                interfaces = NetworkInterface.GetAllNetworkInterfaces();
            }

            UpdateYLabels();

            timer = new Timer();
            timer.Interval = (int)(timeStepInSeconds * 1000f);
            timer.Tick += (object sender, EventArgs e) => UpdateSeries();
            timer.Start();
        }

        private void UpdateSeries()
        {
            long totalSentNow = 0;
            long totalRecievedNow = 0;
            foreach (NetworkInterface ni in interfaces)
            {
                IPInterfaceStatistics iPInterfaceStatistics = ni.GetIPStatistics();
                totalSentNow += iPInterfaceStatistics.BytesSent;
                totalRecievedNow += iPInterfaceStatistics.BytesReceived;
            }

            long deltaSentInKBytes = (_totalSent == 0) ? 0 : (totalSentNow - _totalSent) / 1000;
            long deltaRecievedInKBytes = (_totalRecieved == 0) ? 0 : (totalRecievedNow - _totalRecieved) / 1000;

            long deltaSentInKBit = (_totalSent == 0) ? 0 : (totalSentNow - _totalSent) / 1024 * 8;
            long deltaRecievedInKBit = (_totalRecieved == 0) ? 0 : (totalRecievedNow - _totalRecieved) / 1024 * 8;

            _totalSent = totalSentNow;
            _totalRecieved = totalRecievedNow;

            _sentSeries.RemoveFirstYAndAddY(deltaSentInKBit);
            _recievedSeries.RemoveFirstYAndAddY(deltaRecievedInKBit);

            UpdateYLabels();

            Invalidate();
        }

        private void UpdateYLabels()
        {
            AutoZoomY();

            float max = YAxis.Max;
            //string formatString =
            //    max < 1024 ? "{0:0.# KBit;;0}" :
            //    (max < 1024 * 1024) ? "{0:#, MBit;;0}" :
            //    "{0:#,, GBit;;0}";
            string unitString =
                max < 1024 ? "KBit" :
                (max < 1024 * 1024) ? "MBit" :
                "GBit";
            float divisor =
                max < 1024 ? 1 :
                (max < 1024 * 1024) ? 1024 :
                1024 * 1024;

            YAxis.ClearLabels();
            YAxis.AddLabel(0, "0");
            YAxis.AddLabel(max / 2, Math.Round(max / divisor / 2, 1) + " " + unitString);
            YAxis.AddLabel(max, Math.Round(max / divisor, 1) + " " + unitString);

            //if (max < 1024)
            //{
            //    YAxis.LabelFormatString = "{0:0.# KBit;;0}";
            //}
            //else if (max < 1024 * 1024)
            //{
            //    YAxis.LabelFormatString = "{0:#, MBit;;0}";
            //}
            //else
            //{
            //    YAxis.LabelFormatString = "{0:#,, GBit;;0}";
            //}

            //YAxis.LabelSpacing = (float)Math.Truncate(max / 3);
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
