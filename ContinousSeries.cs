using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickChart
{
    public class ContinousSeries : Series
    { 
        public void RemoveFirstYAndAddY(float y)
        {
            for (int i = 1; i < _yValues.Count; i++)
            {
                _yValues[i - 1] = _yValues[i];
            }
            _yValues[_yValues.Count - 1] = y;
            CalcMinMaxY();
            MarkAverageAsOutdated();
        }

        public void RemoveFirstXYAndAddXY(float x, float y)
        {
            for (int i = 1; i < _xValues.Count; i++)
            {
                _xValues[i - 1] = _xValues[i];
            }
            _xValues[_xValues.Count - 1] = x;
            CalcMinMaxX();

            RemoveFirstYAndAddY(y);
        }
    }
}
