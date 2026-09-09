using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AmidaAmida
{
    class ResultLabelAttributes
    {
        public Visibility Visible { get; set; }
        public Brush Color { get; set; }
        public string Label { get; set; } = "×"; // Default label content
        public ResultLabelAttributes(Brush color, Visibility visible = Visibility.Hidden, string label = "×")
        {
            Color = color;　// Initialize color
            Visible = visible;　// Initialize visibility
            Label = label; // Initialize label content
        }
    }

    internal class ResultLabel
    {
        readonly Canvas m_Canvas;
        Size m_Size;
        int m_Count;
        int m_GoalIndex = -1; // Default to -1, indicating no goal set
        List<ResultLabelAttributes> m_Attributes = [];

        public ResultLabel(ref Canvas canvas)
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
                    m_Attributes.Add(new ResultLabelAttributes(Brushes.Aqua)); // Initialize attributes for each label
                }
            }

            for (int index = 0; index < count; index++)
            {
                string name = index.ToString("D2");
                var x = m_Canvas.Width / (count + 1) * (index + 1);
                var y = m_Canvas.Height;
                Add(index, x, y);
            }
        }

        public void Add(int index, double x, double y)
        {
            // Create an instance of a Label.
            string name = index.ToString("D2");
            Label label = new()
            {
                Name = "Label_" + name,
                Content = m_Attributes[index].Label,
                FontSize = 12,
                Width = 26,
                Foreground = Brushes.Black,
                Background = m_Attributes[index].Color, //.Transparent,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                Visibility = m_Attributes[index].Visible // Initially hidden
            };
            var left = x - label.Width / 2;
            var top = m_Canvas.Height / 2;
            label.SetValue(Canvas.LeftProperty, left);
            label.SetValue(Canvas.TopProperty, top);
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
        public bool CheckGoal(int index, int start)
        {
            bool ret = false;
            if (index < 0 || index >= m_Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be within the range of the count.");
            }
            if (index == m_GoalIndex)
            {
                m_Attributes[index].Label = start.ToString(); // Update the label content to the start value
                m_Attributes[index].Visible = Visibility.Visible; // Set the visibility of the goal label    
                ret = true;
            }
            else
            {
                m_Attributes[index].Label = start.ToString(); // Update the label content to the start value
                m_Attributes[index].Visible = Visibility.Visible; // Set the visibility of the goal label    
                m_Attributes[index].Color = Brushes.Yellow; // Set the color of the goal label
            }
            string name = "Label_" + index.ToString("D2"); // Adjusted to match the label's Name property format
            foreach (var child in m_Canvas.Children)
            {
                if (child is Label label) // Ensure the object is a Label before accessing its properties
                {
                    if (label.Name == name) // Corrected to access the Name property of the Label
                    {
                        label.Visibility = m_Attributes[index].Visible; // Show the label
                        label.Background = m_Attributes[index].Color; // Highlight the goal
                        label.Content = m_Attributes[index].Label; // Change the content to indicate the goal has been reached
                    }
                }
            }
            return ret;
        }

        public void SetGoal(int index)
        {
            m_GoalIndex = index; // Store the goal index

            m_Attributes[index].Visible = Visibility.Visible; // Set the visibility of the goal label    
            m_Attributes[index].Color = Brushes.OrangeRed; // Set the color of the goal label
            m_Attributes[index].Label = "◎"; // Change the label content to indicate the goal

            string name = "Label_" + index.ToString("D2"); // Adjusted to match the label's Name property format
            foreach (var child in m_Canvas.Children)
            {
                if (child is Label label) // Ensure the object is a Label before accessing its properties
                {
                    if (label.Name == name) // Corrected to access the Name property of the Label
                    {
                        label.Visibility = m_Attributes[index].Visible; // Show the label
                        label.Background = m_Attributes[index].Color; // Highlight the goal
                        label.Content = m_Attributes[index].Label; // Change the content to indicate the goal has been set
                    }
                }
            }
        }

        public int GetGoal()
        {
            return m_GoalIndex; // Return the goal index
        }

        public void ClearGoal()
        {
            m_GoalIndex = -1; // Reset the goal index to -1
            foreach (var child in m_Canvas.Children)
            {
                if (child is Label label) // Ensure the object is a Label before accessing its properties
                {
                    label.Visibility = Visibility.Hidden; // Hide all labels
                }
            }
            foreach (var attribute in m_Attributes)
            {
                attribute.Visible = Visibility.Hidden; // Hide all attributes
                attribute.Color = Brushes.Aqua; // Reset color to default
                attribute.Label = "×"; // Reset label content
            }   
        }

        public void ActionLine(double x, double y)
        {
            var line = new Line
            {
                X1 = x,
                X2 = x,
                Y1 = 0,
                Y2 = y / 2,
                Stroke = Brushes.Red,
                StrokeThickness = 3
            };
            m_Canvas.Children.Add(line);
            m_Canvas.UpdateLayout(); // Ensure the canvas updates to show the new line
        }
    }
}
