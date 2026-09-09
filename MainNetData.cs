using System.Windows;
using System.Windows.Controls;

namespace AmidaAmida
{
    internal class MainNetData : NetDataBase
    {

        private Ladder? prevLadder;
        private int prevAxisId = -1;

        public MainNetData(ref Canvas canvas) : base(ref canvas)
        {
        }

        public double GetAxisXPos(int id)
        {
            if (id < 0 || id >= m_LineList.Count)
            {
                return -1; // Return -1 if the id is out of bounds
            }
            return m_LineList[id].LineXpos;
        }

        public AxisLine? FindAxis(ref Point p)
        {
            double nearestPos = double.MaxValue;
            double nearestDistance = double.MaxValue;
            AxisLine? axisLine = null;
            foreach (var line in m_LineList)
            {
                double dsitance = Math.Abs(p.X - line.LineXpos);
                if (nearestDistance > dsitance)
                {
                    axisLine = line;
                    nearestDistance = dsitance;
                    nearestPos = line.LineXpos;
                }
            }
            p.X = nearestPos;
            return axisLine;
        }

        public void PushPos(int id, ref Point p)
        {
            var axisLine = FindAxis(ref p);
            if (axisLine != null) // Ensure axisLine is not null before dereferencing
            {
                axisLine.CheckNearest(ref p);
                prevLadder = new Ladder() { Id = id, AxisId = axisLine.Id, Position = p.Y };
                prevAxisId = axisLine.Id;
            }
        }

        public bool SetNearestPos(int id, ref Point p)
        {
            if (prevAxisId == -1)
            {
                return false;
            }
            int prevId = m_LineList.FindIndex(x => x.Id == prevAxisId);
            if (prevId == -1)
            {
                return false;
            }

            double nearestPos = double.MaxValue;
            AxisLine? axisLine = m_LineList[prevId];

            double distance = Math.Abs(m_LineList[1].LineXpos - m_LineList[0].LineXpos) / 2;
            if (Math.Abs(axisLine.LineXpos - p.X) < distance)
            {
                return false;
            }
            if ((p.X - axisLine.LineXpos) > 0.0)
            {
                if (m_LineList.Count > (prevId + 1))
                {
                    axisLine = m_LineList[prevId + 1];
                    nearestPos = m_LineList[prevId + 1].LineXpos;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (0 <= (prevId - 1))
                {
                    axisLine = m_LineList[prevId - 1];
                    nearestPos = m_LineList[prevId - 1].LineXpos;
                }
                else
                {
                    return false;
                }
            }

            if (axisLine != null) // Ensure axisLine is not null before dereferencing
            {
                axisLine.CheckNearest(ref p);
                if (prevLadder != null)
                {
                    var prevLine = m_LineList.FirstOrDefault(x => x.Id == prevAxisId);

                    if (prevLine != null) // Ensure prevLine is not null before dereferencing
                    {
                        // get list of ladders for the previous line
                        var prevLadderList = prevLine.LadderList.FindAll(x => x.AxisId == axisLine.Id);
                        var existingLadderList = axisLine.LadderList.FindAll(x => x.AxisId == prevLine.Id);
                        var prevLineXpos = prevLine.LineXpos;
                        var existLineXpos = axisLine.LineXpos;
                        foreach (var prevs in prevLadderList)
                        {
                            Ladder? refs = existingLadderList.FirstOrDefault(x => x.Id == prevs.Id);
                            if (refs != null)
                            {
                                bool isCrossed = Judge
                                    (
                                    new Point(prevLineXpos, prevs.Position),
                                    new Point(existLineXpos, refs.Position),
                                    new Point(prevLineXpos, prevLadder.Position),
                                    new Point(existLineXpos, p.Y));
                                if (isCrossed)
                                {
                                    return false;
                                }
                            }
                        }

                        prevLine.SetLine(prevLadder.Id, axisLine.Id, prevLadder.Position);
                        axisLine.SetLine(id, prevLine.Id, p.Y);
                    }
                }
            }
            p.X = nearestPos;
            return true;
        }

        bool Judge(Point a, Point b, Point c, Point d)
        {
            double s, t;
            s = (a.X - b.X) * (c.Y - a.Y) - (a.Y - b.Y) * (c.X - a.X);
            t = (a.X - b.X) * (d.Y - a.Y) - (a.Y - b.Y) * (d.X - a.X);
            if (s * t > 0)
                return false;

            s = (c.X - d.X) * (a.Y - c.Y) - (c.Y - d.Y) * (a.X - c.X);
            t = (c.X - d.X) * (b.Y - c.Y) - (c.Y - d.Y) * (b.X - c.X);
            if (s * t > 0)
                return false;
            return true;
        }
    }
}