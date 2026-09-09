using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AmidaAmida
{
    internal class AxisLine(int id)
    {
        public int Id = id;
        public double LineXpos { get; set; }
        public List<Ladder> LadderList = new List<Ladder>();

        public void ChangePos(double X)
        {
            LineXpos = X;
        }

        public Line GetLine(Line line)
        {
            line.X1 = LineXpos;
            line.Stroke = Brushes.DarkRed;
            return line;
        }

        public void SetLine(int ladderId, int axisId, double y)
        {
            LadderList.Add(new Ladder() { Id = ladderId,  AxisId = axisId, Position = y });
            var sorted = LadderList.OrderBy(x => x.Position);
            LadderList = sorted.ToList();
        }

        public void RemoveLine(int ladderId)
        {
            var ladder = LadderList.FirstOrDefault(x => x.Id == ladderId);
            if (ladder != null)
            {
                LadderList.Remove(ladder);
                var sorted = LadderList.OrderBy(x => x.Position);
                LadderList = sorted.ToList();
            }
        }

        public void CheckNearest(ref Point p)
        {
            int nearestPos = (int)(p.Y / 10) * 10;
            foreach (var line in LadderList)
            {
                if (line.Position == nearestPos)
                {
                    nearestPos += 10;
                }
            }
            p.Y = nearestPos;
        }

        public void Clear()
        {
            LadderList.Clear();
        }
    }
}
