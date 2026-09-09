using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AmidaAmida
{
    internal class NetDataBase
    {
        internal readonly Canvas m_Canvas;
        internal Size m_Size;
        internal int m_Count;
        internal readonly List<AxisLine> m_LineList = [];

        public NetDataBase(ref Canvas canvas)
        {
            m_Canvas = canvas;
        }

        public List<AxisLine> GetAxisLines()
        {
            return m_LineList;
        }

        public virtual Line MakeLine(double x, double y)
        {
            var line = new Line
            {
                X1 = x,
                X2 = x,
                Y1 = 0,
                Y2 = y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            return line;
        }

        public void InitialData(int count)
        {
            m_LineList.Clear();
            m_Count = count;
            m_Size = new Size(m_Canvas.Width, m_Canvas.Height);

            m_Canvas.Children.Clear();
            for (int index = 0; index < count; index++)
            {
                var x = m_Canvas.Width / (count + 1) * (index + 1);
                var y = m_Canvas.Height;
                m_LineList.Add(new AxisLine(index));
                m_LineList[index].ChangePos(x);
                m_Canvas.Children.Add(m_LineList[index].GetLine(MakeLine(x, y)));
            }
        }

        public int GetCount()
        {
            return m_LineList.Count;
        }

        public void RemarkCount(int count)
        {
            while (m_LineList.Count > count)
            {
                m_LineList.RemoveAt(m_LineList.Count - 1);
            }

            while (m_LineList.Count < count)
            {
                m_LineList.Add(new AxisLine(m_LineList.Count));
            }

            m_Count = count;

            m_Canvas.Children.Clear();
            for (int index = 0; index < count; index++)
            {
                var x = m_Canvas.Width / (count + 1) * (index + 1);
                var y = m_Canvas.Height;
                m_LineList[index].ChangePos(x);
                m_Canvas.Children.Add(m_LineList[index].GetLine(MakeLine(x, y)));
            }
        }

        public void FixLines()
        {
            foreach (var ladder in m_Canvas.Children)
            {
                if (ladder is Line line)
                {
                    line.Stroke = Brushes.Black;
                    line.StrokeThickness = 2;
                }
            }
        }
        public void RemoveLine(int id)
        {
            foreach (var line in m_LineList)
            {
                line.RemoveLine(id);
            }
        }

        public void MarkSize(Size size)
        {
            m_Size.Width = size.Width;
            m_Size.Height = size.Height;
        }

        public void RemarkSize(Size size)
        {
            if (size.Width == 0 || size.Height == 0)
            {
                return;
            }
            if (m_Size.Width == size.Width && m_Size.Height == size.Height)
            {
                return;
            }

            double widthRange = size.Width / m_Size.Width;
            double heightRange = size.Height / m_Size.Height;
            foreach (var line in m_LineList)
            {
                line.LineXpos *= widthRange;
                foreach (var ladder in line.LadderList)
                {
                    ladder.Position *= heightRange;
                }
            }
            foreach (var child in m_Canvas.Children)
            {
                if (child is Line line)
                {
                    line.X1 *= widthRange;
                    line.X2 *= widthRange;
                    line.Y1 *= heightRange;
                    line.Y2 *= heightRange;
                }
            }
            m_Size.Width = size.Width;
            m_Size.Height = size.Height;
        }

        public void Clear()
        {
            foreach (var list in m_LineList)
            {
                list.Clear();
            }
            m_LineList.Clear();
        }
    }
}
