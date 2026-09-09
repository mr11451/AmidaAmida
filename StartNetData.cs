using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AmidaAmida
{
    internal class StartNetData : NetDataBase
    {
        public StartNetData(ref Canvas canvas) : base(ref canvas)
        {
        }
        public override Line MakeLine(double x, double y)
        {
            var line = new Line
            {
                X1 = x,
                X2 = x,
                Y1 = y/2,
                Y2 = y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            return line;
        }
    }
}
