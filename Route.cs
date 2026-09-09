using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace AmidaAmida
{
    internal class Route
    {
        List<Point> m_Points = new List<Point>();
        List<AxisLine> m_AxisLines;
        int MaxYposition;
        public Route(List<AxisLine> NetData)
        {
            m_AxisLines = NetData;
        }

        public void RemarkRoute(List<AxisLine> NetData)
        {
            m_AxisLines = NetData;
        }

        public void SetMaxYposition(int maxYposition)
        {
            MaxYposition = maxYposition;
        }

        public AxisLine GetAxis(Point p)
        {
            double nearestPos = double.MaxValue;
            double nearestDistance = double.MaxValue;
            AxisLine axisLine = m_AxisLines[0];
            foreach (var line in m_AxisLines)
            {
                double distance = Math.Abs(p.X - line.LineXpos);
                if (nearestDistance > distance)
                {
                    axisLine = line;
                    nearestDistance = distance;
                    nearestPos = line.LineXpos;
                }
            }
            return axisLine;
        }

        public int GetAxisId(Point p)
        {
            var axisLine = GetAxis(p);
            return axisLine.Id;
        }

        public int GetAxisPosition(int axisId)
        {
            var axisLine = m_AxisLines.FirstOrDefault(x => x.Id == axisId);
            if (axisLine == null)
            {
                return -1;
            }
            return (int)axisLine.LineXpos;
        }

        public Ladder? GetLadder(int axisId, double y)
        {
            var axisLine = m_AxisLines.FirstOrDefault(x => x.Id == axisId);
            if (axisLine == null)
            {
                return null;
            }
            Ladder? localLadder = null;
            double localY = double.MaxValue;
            // Get Near Position
            foreach (var ladder in axisLine.LadderList)
            {
                if (y <= ladder.Position)
                {
                    double distance = Math.Abs(y - ladder.Position);
                    if (localY > distance)
                    {
                        localY = distance;
                        localLadder = ladder;
                    }
                }
            }
            return localLadder;
        }

        public int GetLadderId(int axisId, double y)
        {
            var ladder = GetLadder(axisId, y);
            if (ladder != null)
            {
                return ladder.Id;
            }
            return -1; // Return -1 if no ladder is found
        }

        public double GetLadderPosAxisId(int axisId, int ladderId)
        {
            var axisLine = m_AxisLines.FirstOrDefault(x => x.Id == axisId);
            if (axisLine == null)
            {
                return -1; // Return -1 if no axis line is found
            }
            var ladder = axisLine.LadderList.FirstOrDefault(x => x.Id == ladderId);
            if (ladder != null)
            {
                return ladder.Position; // Return the axis ID of the ladder
            }
            return -1; // Return -1 if no ladder is found
        }

        public void ClearRoute()
        {
            m_Points.Clear(); // Clear the list of points in the route
        }

        public int StartRoute(int x)
        {
            Point point = new(x,0);
            int axisId = GetAxisId(point);
            m_Points.Clear();
            m_Points.Add(point);
            while (true)
            {
                if (axisId == -1)
                {
                    break; // Exit loop if no axis line is found
                }
                double y = point.Y;
                var ladder = GetLadder(axisId, y);
                if (ladder != null)
                {
                    point.Y = ladder.Position;
                    m_Points.Add(point);
                    var ladderId = ladder.Id; // Get the ladder ID
                    if (ladderId == -1)
                    {
                        break; // Exit loop if no ladder ID is found
                    }
                    axisId = ladder.AxisId; // Get the axis ID of the ladder
                    var Line = m_AxisLines.FirstOrDefault(x => x.Id == axisId);
                    if (Line == null)
                    {
                        break; // Exit loop if no axis line is found
                    }
                    point.Y = GetLadderPosAxisId(axisId, ladderId); // Update point's Y position to the ladder's position
                    point.X = (int)Line.LineXpos; // Update point's Y position to the ladder's position
                    m_Points.Add(point);
                }
                else
                {
                    break; // Exit loop if no ladder is found
                }
                point.Y += 10; // Move down by 10 units
            }
            if (m_Points.Count > 0)
            {
                point.Y = MaxYposition;
                m_Points.Add(point);
            }
            return m_Points.Count; // Return the number of points in the route
        }

        public List<Point> GetRoutePoints()
        {
            return m_Points; // Return the list of points in the route
        }

        public Point GetPoint(int index)
        {
            if (index >= 0 && index < m_Points.Count)
            {
                return m_Points[index]; // Return the point at the specified index
            }
            return new Point(0, 0); // Return a default point if the index is out of range
        }

        internal int GetAxisCount()
        {
            return m_AxisLines.Count; // Return the number of axis lines
        }
    }
}
