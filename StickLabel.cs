using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AmidaAmida
{
    class StickLabelAttribute : Attribute
    {
        public Brush Color { get; set; }
        public bool IsGoal { get; set; } = false; // Indicates if this label is a goal
        public StickLabelAttribute(Brush color, bool isGoal = false)
        {
            Color = color; // Initialize color
            IsGoal = isGoal; // Default to not a goal
        }
    }

    internal class StickLabel
    {
        readonly Canvas m_Canvas;
        Size m_Size;
        int m_Count;
        List<StickLabelAttribute> m_Attributes = [];

        public StickLabel(ref Canvas canvas)
        {
            m_Canvas = canvas;
        }

        public void InitialData(int count)
        {
            m_Size = new Size(m_Canvas.Width, m_Canvas.Height);
            RemarkCount(count);
        }

        public int GetCount()
        {
            return m_Count;
        }

        public void RemarkCount(int count, bool forced = false)
        {
            m_Count = count;
            m_Canvas.Children.Clear();
            if (count != m_Attributes.Count || forced)
            {
                m_Attributes.Clear(); // Clear previous attributes
                for (int index = 0; index < count; index++)
                {
                    m_Attributes.Add(new StickLabelAttribute(Brushes.White)); // Initialize attributes for each label
                }
            }
            for (int index = 0; index < count; index++)
            {
                var x = m_Canvas.Width / (count + 1) * (index + 1);
                var y = m_Canvas.Height;
                Add(index, x, y);
            }
        }

        public void Add(int index, double x, double y)
        {
            // Create an instance of a Label.
            string title = (index + 1).ToString();
            Label label = new()
            {
                Name = "Label_" + index.ToString("D2"),
                Content = title,
                FontSize = 12,
                Width = 26,
                Foreground = Brushes.Black,
                Background = m_Attributes[index].Color, //.Transparent,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalContentAlignment = System.Windows.VerticalAlignment.Center
            };
            var left = x - label.Width / 2;
            label.SetValue(Canvas.LeftProperty, left);
            m_Canvas.Children.Add(label);
        }

        internal void RemarkSize(Size size)
        {
            double widthRange = size.Width / m_Size.Width;
            double heightRange = size.Height / m_Size.Height;

            m_Size.Width = size.Width;
            m_Size.Height = size.Height;

            foreach (var child in m_Canvas.Children)
            {
                if (child is Label label)
                {
                    var left = (double)label.GetValue(Canvas.LeftProperty) + label.Width / 2;
                    label.SetValue(Canvas.LeftProperty, left * widthRange - label.Width / 2);
                }
                if (child is Line line)
                {
                    line.X1 *= widthRange;
                    line.X2 *= widthRange;
                    line.Y1 *= heightRange;
                    line.Y2 *= heightRange;
                }
            }
        }

        public void ColorChange(int index)
        {
            string name = "Label_" + index.ToString("D2"); // Adjusted to match the label's Name property format
            for (var loop = 0; loop < m_Canvas.Children.Count; loop++)
            {
                var child = m_Canvas.Children[loop];
                if (child is Label label) // Ensure the object is a Label before accessing its properties
                {
                    if (label.Name == name) // Corrected to access the Name property of the Label
                    {
                        label.Background = System.Windows.Media.Brushes.OrangeRed; // Highlight the goal
                    }
                    else
                    {
                        label.Background = m_Attributes[loop].Color; // Reset the background color for other labels
                    }
                }
            }
        }
        public void ColorChangeReady()
        {
            foreach (var label in m_Attributes)
            {
                if (!label.IsGoal)
                {
                    label.Color = Brushes.Aqua; // Reset all labels to the default color
                }
            }
            for (var index = 0; index < m_Canvas.Children.Count; index++)
            {
                var child = m_Canvas.Children[index];
                if (child is Label label) // Ensure the object is a Label before accessing its properties
                {
                    label.Background = m_Attributes[index].Color; // Reset the background color for all labels
                }
            }
        }

        public bool ColorChangeEndMark(int index)
        {
            m_Attributes[index].IsGoal = true; // Mark this label as a goal
            m_Attributes[index].Color = Brushes.LightGray; // Change the color of the specified label

            bool isGoal = true;
            foreach (var attribute in m_Attributes)
            {
                isGoal &= attribute.IsGoal; // Check if all labels are marked as goals
            }
            return isGoal;
        }

        public void ColorReset()
        {
            for (var index = 0; index < m_Canvas.Children.Count; index++)
            {
                var child = m_Canvas.Children[index];
                if (child is Label label) // Ensure the object is a Label before accessing its properties
                {
                    label.Background = m_Attributes[index].Color; // Reset the background color
                }
            }
        }

        public void ActionLine(double x, double y)
        {
            var line = new Line
            {
                X1 = x,
                X2 = x,
                Y1 = y / 2,
                Y2 = y,
                Stroke = Brushes.Red,
                StrokeThickness = 3
            };
            m_Canvas.Children.Add(line);
        }
    }
}
