using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AmidaAmida
{
    /// <summary>  
    /// Interaction logic for MainWindow.xaml  
    /// </summary>  
    public partial class MainWindow : Window
    {
        CancellationTokenSource cancelSource = new CancellationTokenSource();
        static bool autoEvent = false;
        static bool scroll = false;
        
        private enum Process
        {
            None,
            Createing,
            Selecting,
            Playing,
            Calulating,
            Moving
        };
        private Process process = Process.None;
        private readonly MainNetData mainNetData;
        private readonly StartNetData startNetData;
        private readonly GoalNetData goalNetData;
        private readonly StickLabel stickLabel;
        private readonly ResultLabel resultLabel;
        private readonly Route route;
        private readonly AnimationControl animationColtrol;
        private readonly Line LinePos = new();
        private Point MouseIn, MouseOut;
        private Boolean isMouseDown = false;

        private Size ViewSize;
        private readonly List<bool> lineTypeList = [];

        private int SelectedAxisId = -1; // Add this field to track the selected axis
        private static readonly Random random = new Random(); // Add this field to the class

        private double m_AnimationSpeed = 100;

        private string DefaultCharactorFile = Properties.Settings.Default.DefaultCharactorFile;
        private double AppLeft = Properties.Settings.Default.AppLeft;
        private double AppTop = Properties.Settings.Default.AppTop;
        private double AppWidth = Properties.Settings.Default.AppWidth;
        private double AppHeight = Properties.Settings.Default.AppHeight;
        private string AxisCount = Properties.Settings.Default.AxisCount;
        private string AnimationSpeed = Properties.Settings.Default.AnimationSpeed;

        private readonly SoundControl soundControl = SoundControl.Instance;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save the current window position and size to settings
            Properties.Settings.Default.AppLeft = this.Left;
            Properties.Settings.Default.AppTop = this.Top;
            Properties.Settings.Default.AppWidth = this.Width;
            Properties.Settings.Default.AppHeight = this.Height;
            Properties.Settings.Default.AppWindowMode = (int)this.WindowState;

            // Save the AxisCount and AnimationSpeed settings
            Properties.Settings.Default.AxisCount = AxisCountTextBox.Text;

            // Save the selected animation speed based on the checked radio button
            if (CheckVerySlow.IsChecked == true)
            {
                Properties.Settings.Default.AnimationSpeed = "0"; // Very Slow speed
            }
            else if (CheckSlow.IsChecked == true)
            {
                Properties.Settings.Default.AnimationSpeed = "1"; // Slow speed
            }
            else if (CheckFast.IsChecked == true)
            {
                Properties.Settings.Default.AnimationSpeed = "2"; // Fast speed
            }
            else if (CheckVeryFast.IsChecked == true)
            {
                Properties.Settings.Default.AnimationSpeed = "3"; // Very Fast speed
            }
            else if (CheckNonStop.IsChecked == true)
            {
                Properties.Settings.Default.AnimationSpeed = "4"; // Non Stop
            }
            Properties.Settings.Default.DefaultCharactorFile = DefaultCharactorFile;
            Properties.Settings.Default.Save();
        }

        public MainWindow()
        {
            InitializeComponent();
            mainNetData = new(ref AmidaCanvasBase);
            startNetData = new(ref AmidaStartCanvas);
            goalNetData = new(ref AmidaGoalCanvas);
            stickLabel = new(ref AmidaStickLabel);
            resultLabel = new(ref AmidaResultLabel);
            route = new(mainNetData.GetAxisLines());
            animationColtrol = new(ref AnimationCanvas, DefaultCharactorFile);
            AxisUndoButton.IsEnabled = false;
            AxisRunButton.IsEnabled = false;
            AxisResetButton.IsEnabled = false;
            GoalMaskImage.Visibility = Visibility.Hidden;
            if (AppLeft < 0 || AppTop < 0 || AppWidth <= 0 || AppHeight <= 0)
            {
                // Default values if settings are invalid
                Properties.Settings.Default.AppLeft = AppLeft = this.Left;
                Properties.Settings.Default.AppTop = AppTop = this.Top;
                Properties.Settings.Default.AppWidth = AppWidth = this.Width;
                Properties.Settings.Default.AppHeight = AppHeight = this.Height;
                Properties.Settings.Default.Save();
            }
            this.Left = AppLeft;
            this.Top = AppTop;
            this.Width = AppWidth;
            this.Height = AppHeight;
            this.WindowState = (WindowState)Properties.Settings.Default.AppWindowMode;

            // Set the initial AxisCount from settings
            AxisCountTextBox.Text = AxisCount;

            // Set the initial AxisCount and AnimationSpeed from settings
            switch (AnimationSpeed)
            {
                case "0": // Very Slow speed
                    CheckVerySlow.IsChecked = true;
                    m_AnimationSpeed = 400;
                    break;
                case "1": // Slow speed
                    CheckSlow.IsChecked = true;
                    m_AnimationSpeed = 300;
                    break;
                case "2": // Fast speed
                    CheckFast.IsChecked = true;
                    m_AnimationSpeed = 200;
                    break;
                case "3": // Very Fast speed
                    CheckVeryFast.IsChecked = true;
                    m_AnimationSpeed = 100;
                    break;
                case "4": // Non Stop
                    CheckNonStop.IsChecked = true;
                    m_AnimationSpeed = 10;
                    break;
                default: // Default to Very Slow speed
                    CheckVerySlow.IsChecked = true;
                    m_AnimationSpeed = 400;
                    break;
            }
        }

        private void AmidaCanvasBase_Loaded(object sender, RoutedEventArgs e)
        {
            ResultMessage.Visibility = Visibility.Hidden;
            AxisFixButton.IsEnabled = false;
            var height = SystemParameters.PrimaryScreenHeight;
            var iWidth = AmidaBodyGrid.ActualWidth;
            ViewSize = new Size(iWidth, height);
            AmidaStartCanvas.Width = iWidth;
            AmidaStartCanvas.Height = 50;
            AmidaStartCanvas.Background = Brushes.White;
            AmidaStickLabel.Width = iWidth;
            AmidaStickLabel.Height = 50;
            AmidaStickLabel.Background = Brushes.Transparent;
            AmidaCanvasBase.Width = iWidth;
            AmidaCanvasBase.Height = height;
            AmidaCanvasBase.Background = Brushes.White;
            AmidaCanvasBase.Children.Clear();
            AmidaCanvas.Width = iWidth;
            AmidaCanvas.Height = height;
            AmidaCanvas.Background = Brushes.Transparent;
            AmidaActionCanvas.Width = iWidth;
            AmidaActionCanvas.Height = height;
            AmidaActionCanvas.Background = Brushes.Transparent;
            AmidaGoalCanvas.Width = iWidth;
            AmidaGoalCanvas.Height = 50;
            AmidaGoalCanvas.Background = Brushes.White;
            AmidaResultLabel.Width = iWidth;
            AmidaResultLabel.Height = 50;
            AmidaResultLabel.Background = Brushes.Transparent;
            GoalMaskImage.Width = iWidth;
            AnimationCanvas.Width = iWidth;
            AmidaCanvas.Children.Clear();
            if (!int.TryParse(AxisCountTextBox.Text, out int axisCount))
            {
                axisCount = 2; // Default value if parsing fails
                AxisCountTextBox.Text = axisCount.ToString();
            }
            if (axisCount == 2)
            {
                AxisCountDownButton.IsEnabled = false; // Disable the button if count is 2
            }
            else
            {
                AxisCountDownButton.IsEnabled = true; // Enable the button if count is greater than 2
            }
            mainNetData.InitialData(axisCount);
            mainNetData.MarkSize(new Size(iWidth, height));
            startNetData.InitialData(axisCount);
            goalNetData.InitialData(axisCount);
            stickLabel.InitialData(axisCount);
            resultLabel.InitialData(axisCount);
            route.SetMaxYposition((int)height);
            AxisCountTextBox.Text = mainNetData.GetCount().ToString();
            animationColtrol.AnimationHandle += AnimationDone;
            animationColtrol.SetOffset(0, AmidaStartCanvas.Height);
            animationColtrol.JumpToFrame(AnimationControl.Direction.Down);
            soundControl.SetEnable();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var height = SystemParameters.PrimaryScreenHeight;
            var iWidth = AmidaBodyGrid.ActualWidth;
            AmidaStartCanvas.Width = iWidth;
            AmidaStickLabel.Width = iWidth;
            AmidaCanvasBase.Width = iWidth;
            AmidaCanvasBase.Height = height;
            AmidaCanvas.Width = iWidth;
            AmidaCanvas.Height = height;
            AmidaActionCanvas.Width = iWidth;
            AmidaActionCanvas.Height = height;
            AmidaGoalCanvas.Width = iWidth;
            AmidaResultLabel.Width = iWidth;
            var size = new Size(iWidth, height);
            mainNetData.RemarkSize(new Size(iWidth, height));
            startNetData.RemarkSize(new Size(iWidth, AmidaStartCanvas.ActualHeight));
            goalNetData.RemarkSize(new Size(iWidth, AmidaGoalCanvas.ActualHeight));
            stickLabel.RemarkSize(new Size(iWidth, AmidaStickLabel.ActualHeight));
            resultLabel.RemarkSize(new Size(iWidth, AmidaResultLabel.ActualHeight));
            route.RemarkRoute(mainNetData.GetAxisLines());
            AnimationCanvas.Width = iWidth;
            GoalMaskImage.Width = iWidth;
            RemarkLineSize(size);
        }

        private void AxisCountUpButton_Click(object sender, RoutedEventArgs e)
        {
            AmidaCanvas.Children.Clear();
            mainNetData.Clear();
            startNetData.Clear();
            goalNetData.Clear();
            lineTypeList.Clear();
            AxisUndoButton.IsEnabled = false;
            AxisCreateButton.IsEnabled = true;
            AxisFixButton.IsEnabled = false;
            AxisCountDownButton.IsEnabled = true;
            GoalMaskImage.Visibility = Visibility.Hidden;
            process = Process.None;
            _ = int.TryParse(AxisCountTextBox.Text, out int param);
            param++;
            if (param > 99)
            {
                param = 99; // Limit the maximum axis count to 99
                AxisCountUpButton.IsEnabled = false; // Disable the button if count exceeds 99
                soundControl.PlayID(SoundControl.ID.LineNG);
            }
            else
            {
                soundControl.PlayID(SoundControl.ID.BtnUp);
            }
            AxisCountTextBox.Text = param.ToString();
            mainNetData.RemarkCount(param);
            startNetData.RemarkCount(param);
            stickLabel.RemarkCount(param);
            goalNetData.RemarkCount(param);
            resultLabel.RemarkCount(param);
        }

        private void AxisCountDownButton_Click(object sender, RoutedEventArgs e)
        {
            AmidaCanvas.Children.Clear();
            mainNetData.Clear();
            startNetData.Clear();
            goalNetData.Clear();
            lineTypeList.Clear();
            AxisUndoButton.IsEnabled = false;
            AxisCreateButton.IsEnabled = true;
            AxisFixButton.IsEnabled = false;
            AxisCountUpButton.IsEnabled = true;
            GoalMaskImage.Visibility = Visibility.Hidden;
            process = Process.None;
            _ = int.TryParse(AxisCountTextBox.Text, out int param);
            if (param > 2)
            {
                soundControl.PlayID(SoundControl.ID.BtnDown);
                param--;
                AxisCountTextBox.Text = param.ToString();
                mainNetData.RemarkCount(param);
                startNetData.RemarkCount(param);
                stickLabel.RemarkCount(param);
                goalNetData.RemarkCount(param);
                resultLabel.RemarkCount(param);
            }
            else
            {
                AxisCountDownButton.IsEnabled = false;
                soundControl.PlayID(SoundControl.ID.LineNG);
            }
        }

        private void AxisCreateButton_Click(object sender, RoutedEventArgs e)
        {
            soundControl.PlayID(SoundControl.ID.BtnClick);

            int hight = (int)AmidaCanvas.Height;
            int width = (int)AmidaCanvas.Width;
            int spawn = mainNetData.GetCount();
            int step = 40;
            if (spawn >= 70)
            {
                step = 10;
            }
            else if (spawn >= 50)
            {
                step = 20;
            }
            else if (spawn >= 30)
            {
                step = 30;
            }
            int count = hight / step;
            int delta = (int)(width / (spawn + 1));
            bool loop = false;
            // Clear previous lines and reset the lineTypeList
            for (int i = 1; i < count; i++)
            {
                bool continueFlag = (spawn > 2) && ((i & 1) == 1); // Toggle continueFlag based on the row index
                // Create lines for each axis in the current row
                for (int j = 0; j < spawn - 1; j++)
                {
                    if (!(continueFlag || random.Next(0, 20) == 1))
                    {
                        int id = AmidaCanvas.Children.Count + 1;
                        mainNetData.GetAxisLines();
                        MouseIn = new Point(mainNetData.GetAxisXPos(j), i * step);
                        mainNetData.PushPos(id, ref MouseIn);
                        MouseOut = new Point(mainNetData.GetAxisXPos(j + 1), MouseIn.Y);
                        if (mainNetData.SetNearestPos(id, ref MouseOut))
                        {
                            string name = "_" + id.ToString();
                            var newLine = new Line()
                            {
                                X1 = MouseIn.X,
                                Y1 = MouseIn.Y,
                                X2 = MouseOut.X,
                                Y2 = MouseOut.Y,
                                Stroke = Brushes.Black,
                                Name = name
                            };
                            lineTypeList.Add(loop);
                            loop = true;
                            AmidaCanvas.Children.Add(newLine); // Add the line to the canvas
                        }
                    }
                    continueFlag = !continueFlag; // Toggle continueFlag
                }
            }
            AxisUndoButton.IsEnabled = true;
            AxisCreateButton.IsEnabled = false;
            AxisFixButton.IsEnabled = true;
            GoalMaskImage.Children.Clear();
            GoalMaskImage.Children.Add(new Label
            {
                Content = "You can use the mouse to draw additional horizontal lines.",
                FontSize = 20,
                Foreground = Brushes.Black,
                Width = GoalMaskImage.ActualWidth,
                Height = GoalMaskImage.ActualHeight,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)) // Semi-transparent background
            });
            GoalMaskImage.Visibility = Visibility.Visible;
            process = Process.Createing;

            count = random.Next(mainNetData.GetCount());
            resultLabel.SetGoal(count);
        }

        private void AxisFixButton_Click(object sender, RoutedEventArgs e)
        {
            soundControl.PlayID(SoundControl.ID.BtnClick);
            AxisCountUpButton.IsEnabled = false;
            AxisCountDownButton.IsEnabled = false;
            AxisUndoButton.IsEnabled = false;
            AxisFixButton.IsEnabled = false;
            AxisRunButton.IsEnabled = false;

            foreach (var child in AmidaCanvas.Children)
            {
                if (child is Line line)
                {
                    line.Stroke = Brushes.Black;
                }
            }
            mainNetData.FixLines();
            startNetData.FixLines();
            goalNetData.FixLines();
            stickLabel.ColorChangeReady();
            GoalMaskImage.Children.Clear();
            GoalMaskImage.Children.Add(new Label
            {
                Content = "Select an axis number and then press RUN to start the game.",
                FontSize = 20,
                Foreground = Brushes.Black,
                Width = GoalMaskImage.ActualWidth,
                Height = GoalMaskImage.ActualHeight,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)) // Semi-transparent background
            });
            process = Process.Selecting;
        }

        private void AxisUndoButton_Click(object sender, RoutedEventArgs e)
        {
            soundControl.PlayID(SoundControl.ID.UnDo);
            AmidaResultLabel.Children.Clear();
            AmidaActionCanvas.Children.Clear();
            route.ClearRoute();

            if (AmidaCanvas.Children.Count > 0)
            {
                if (lineTypeList.Count != 0)
                {
                    bool loop;
                    bool isLoop = false;
                    do
                    {
                        if (lineTypeList.Count == 0)
                            break;
                        loop = lineTypeList[AmidaCanvas.Children.Count - 1];
                        lineTypeList.RemoveAt(AmidaCanvas.Children.Count - 1);
                        mainNetData.RemoveLine(AmidaCanvas.Children.Count);
                        AmidaCanvas.Children.RemoveAt(AmidaCanvas.Children.Count - 1);
                        if (loop) isLoop = true;
                    } while (loop);

                    if (isLoop)
                    {
                        AxisCreateButton.IsEnabled = true;
                    }
                }
            }
            if (AmidaCanvas.Children.Count == 0)
            {
                AxisUndoButton.IsEnabled = false;
                AxisFixButton.IsEnabled = false;
                process = Process.None;
                resultLabel.ClearGoal();
                GoalMaskImage.Visibility = Visibility.Hidden;
            }
        }

        private async void AxisRunButton_Click(object sender, RoutedEventArgs e)
        {
            soundControl.PlayID(SoundControl.ID.Start);
            ResultMessage.Visibility = Visibility.Hidden;
            process = Process.Playing;
            AxisRunButton.IsEnabled = false;
            GoalMaskImage.Visibility = Visibility.Hidden;
            var posX = route.GetAxisPosition(SelectedAxisId);
            var count = route.StartRoute(posX);
            if (count > 0)
            {
                process = Process.Moving;
                await Playing(count); // Use await to call the async method
            }
        }

        private void AxisResetButton_Click(object sender, RoutedEventArgs e)
        {
            soundControl.PlayID(SoundControl.ID.UnDo);
            ResultMessage.Visibility = Visibility.Hidden;
            animationColtrol.StopAnimation();
            AmidaCanvas.Children.Clear();
            AmidaActionCanvas.Children.Clear();
            mainNetData.Clear();
            startNetData.Clear();
            goalNetData.Clear();
            lineTypeList.Clear();
            AxisCountUpButton.IsEnabled = true;
            AxisCountDownButton.IsEnabled = true;
            AxisUndoButton.IsEnabled = false;
            AxisCreateButton.IsEnabled = true;
            AxisFixButton.IsEnabled = false;
            AxisRunButton.IsEnabled = false;
            AxisResetButton.IsEnabled = false;
            GoalMaskImage.Visibility = Visibility.Hidden;
            process = Process.None;
            _ = int.TryParse(AxisCountTextBox.Text, out int param);
            mainNetData.RemarkCount(param);
            startNetData.RemarkCount(param);
            stickLabel.RemarkCount(param, true);
            goalNetData.RemarkCount(param);
            resultLabel.RemarkCount(param, true);
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            soundControl.PlayID(SoundControl.ID.BtnClick);
            if (sender is RadioButton radioButton)
            {
                m_AnimationSpeed = radioButton.Tag switch
                {
                    "0" => 400,// Very Slow speed
                    "1" => 300,// Slow speed
                    "2" => 200,// Fast speed
                    "3" => 100,// Very Fast speed
                    "4" => 10,// Non Stop
                    _ => (double)400,// Very Slow speed
                };
                animationColtrol?.SetAnimationSpeed(m_AnimationSpeed);
            }
        }
        private void CustomizeButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the problematic code
            var defaultPath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "resource");
            OpenFileDialog openFileDialog = new()
            {
                Title = "Select a Charactor File.",
                Filter = "PNG Files (*.png)|*.png",
                DefaultExt = "*.png",
                DefaultDirectory = defaultPath
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var file = openFileDialog.FileName;
                if (AnimationControl.CheckAnimationCharacter(file))
                {
                    DefaultCharactorFile = System.IO.Path.GetFileName(file);
                    var newName = System.IO.Path.Combine(defaultPath, DefaultCharactorFile);
                    if (File.Exists(newName))
                    {
                        MessageBox.Show("Already exist. Please select another file or delete the existing one.");
                    }
                    else
                    {
                        File.Copy(file, System.IO.Path.Combine(defaultPath, DefaultCharactorFile), true);
                        MessageBox.Show("Change Animation Charactor when Next open Application.");
                    }
                }
                else
                {
                    DefaultCharactorFile = "DefaultCharactor.png";
                    MessageBox.Show("Invalid Charactor File. Please select a valid PNG file.");
                }
            }
        }
        private void AmidaView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (process)
            {
                case Process.Createing:
                    if (sender is Grid amidaBodyGrid && amidaBodyGrid.Name == "AmidaBodyGrid")
                    {
                        if (!isMouseDown)
                        {
                            soundControl.PlayID(SoundControl.ID.LineSet);
                            isMouseDown = true;
                            MouseIn = e.GetPosition(AmidaCanvas);
                            int id = AmidaCanvas.Children.Count + 1;
                            mainNetData.PushPos(id, ref MouseIn);
                        }
                    }
                    break;
                case Process.Selecting:
                    if (sender is Grid amidaStartGrid && amidaStartGrid.Name == "AmidaStartGrid")
                    {
                        soundControl.PlayID(SoundControl.ID.BtnClick);
                        ResultMessage.Visibility = Visibility.Hidden;
                        animationColtrol.StopAnimation();

                        AmidaActionCanvas.Children.Clear();
                        stickLabel.RemarkCount(stickLabel.GetCount());
                        resultLabel.RemarkCount(resultLabel.GetCount());

                        var pos = e.GetPosition(AmidaCanvas);
                        pos.Y = 0;
                        SelectedAxisId = route.GetAxisId(pos);
                        stickLabel.ColorReset();
                        stickLabel.ColorChange(SelectedAxisId);

                        AxisRunButton.IsEnabled = true;
                    }
                    break;
            }
        }

        private void AmidaView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (process)
            {
                case Process.Createing:
                    {
                        if (isMouseDown)
                        {
                            isMouseDown = false;
                            MouseOut = e.GetPosition(AmidaCanvas);
                            MouseOut.Y = MouseIn.Y;

                            if (AmidaCanvas.Children.Contains(LinePos))
                            {
                                AmidaCanvas.Children.Remove(LinePos); // Remove the previous line if it exists
                            }

                            int id = AmidaCanvas.Children.Count + 1;
                            if (mainNetData.SetNearestPos(id, ref MouseOut))
                            {
                                soundControl.PlayID(SoundControl.ID.Connect);
                                string name = "_" + id.ToString();
                                var newLine = new Line()
                                {
                                    X1 = MouseIn.X,
                                    Y1 = MouseIn.Y,
                                    X2 = MouseOut.X,
                                    Y2 = MouseOut.Y,
                                    Stroke = Brushes.Black,
                                    Name = name
                                };
                                lineTypeList.Add(false);
                                AmidaCanvas.Children.Add(newLine); // Add the line to the canvas
                                AxisUndoButton.IsEnabled = true;
                                AxisFixButton.IsEnabled = true;
                            }
                            else
                            {
                                soundControl.PlayID(SoundControl.ID.LineNG);
                            }
                        }
                    }
                    break;
            }
        }

        private void AmidaView_MouseMove(object sender, MouseEventArgs e)
        {
            switch (process)
            {
                case Process.Createing:
                    {
                        if (isMouseDown)
                        {
                            if (AmidaCanvas.Children.Contains(LinePos))
                            {
                                AmidaCanvas.Children.Remove(LinePos); // Remove the previous line if it exists
                            }
                            Point currentPosition = e.GetPosition(AmidaCanvas);

                            LinePos.X1 = MouseIn.X;
                            LinePos.Y1 = MouseIn.Y;
                            LinePos.X2 = currentPosition.X;
                            LinePos.Y2 = currentPosition.Y;
                            LinePos.Stroke = Brushes.Blue;
                            LinePos.Name = "_temp";

                            AmidaCanvas.Children.Add(LinePos); // Add the new line to the canvas  
                        }
                    }
                    break;
            }
        }

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //    Debug.WriteLine("Grid_PreviewMouseLeftButtonDown");
        }

        private void Grid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //    Debug.WriteLine("Grid_PreviewMouseLeftButtonUp");
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            //    Debug.WriteLine("Grid_MouseMove");
        }

        private void RemarkLineSize(Size size)
        {
            do
            {
                if (size.Width == 0 || size.Height == 0)
                    break;
                if (AmidaCanvas.Children.Count == 0)
                    break;
                if (ViewSize.Width == 0 || ViewSize.Height == 0)
                    break;
                if (AmidaCanvas.Width == 0 || AmidaCanvas.Height == 0)
                    break;
                if (ViewSize.Width == size.Width && ViewSize.Height == size.Height)
                    break;

                double widthRange = size.Width / ViewSize.Width;
                double heightRange = size.Height / ViewSize.Height;
                foreach (var child in AmidaCanvas.Children)
                {
                    if (child is Line line)
                    {
                        line.X1 *= widthRange;
                        line.Y1 *= heightRange;
                        line.X2 *= widthRange;
                        line.Y2 *= heightRange;
                    }
                }
                foreach (var child in AmidaActionCanvas.Children)
                {
                    if (child is Line line)
                    {
                        line.X1 *= widthRange;
                        line.Y1 *= heightRange;
                        line.X2 *= widthRange;
                        line.Y2 *= heightRange;
                    }
                }
            } while (false);
            ViewSize.Width = size.Width;
            ViewSize.Height = size.Height;
        }

        private async Task Playing(int count)
        {
            bool isGoal = AxisResetButton.IsEnabled;
            AxisResetButton.IsEnabled = false;
            this.ResizeMode = ResizeMode.NoResize;
            AmidaActionCanvas.Children.Clear();
            var pos0 = route.GetPoint(0);

            animationColtrol.JumpToFrame(AnimationControl.Direction.Down);
            animationColtrol.SetOffset(0, AmidaStartCanvas.Height);
            animationColtrol.SetPosition(pos0.X, -AmidaStartCanvas.Height);
            animationColtrol.SetAnimationSpeed(m_AnimationSpeed);
            animationColtrol.StartAnimation();

            stickLabel.ActionLine(pos0.X, 50);
            int scrollOffset = 0; // Initialize scroll offset

            for (int i = 1; i < count; i++)
            {
                var pos1 = route.GetPoint(i);
                var newLine = new Line
                {
                    X1 = pos0.X,
                    Y1 = pos0.Y,
                    X2 = pos1.X,
                    Y2 = pos1.Y,
                    Stroke = Brushes.Red,
                    StrokeThickness = 3
                };
                _ = AmidaActionCanvas.Children.Add(newLine);

                AmidaActionCanvas.InvalidateVisual();
                AmidaActionCanvas.UpdateLayout();

                do
                {
                    AmidaView.ScrollToVerticalOffset(scrollOffset - this.Height / 3);
                    var Vpos = AmidaView.VerticalOffset;
                    if (animationColtrol.MoveTo(pos1.X, pos1.Y - Vpos, 5))
                    {
                        scroll = false; 
                        autoEvent = true; // Set the autoEvent to true to indicate animation is in progress
                        while (autoEvent)
                        {
                            if (pos0.Y < pos1.Y)
                            {
                                scrollOffset += 5; // Scroll down
                            }
                            else if (pos0.Y > pos1.Y)
                            {
                                scrollOffset -= 5; // Scroll up
                            }

                            if (scroll)
                            {
                                scroll = false; // Reset the scroll flag
                                Vpos = AmidaView.VerticalOffset;
                                if (!animationColtrol.MoveTo(pos1.X, pos1.Y - Vpos, 5))
                                {
                                    break;
                                }
                            }
                            AmidaView.ScrollToVerticalOffset(scrollOffset - this.Height / 3);
                            await Task.Delay((int)m_AnimationSpeed);
                        }
                    }
                    if (scrollOffset > pos1.Y)
                    {
                        break;
                    }
                    else if (scrollOffset < pos1.Y)
                    {
                        break;
                    }
                    else break;
                } while (true);

                pos0 = pos1; // Update pos0 for the next iteration
            }

            resultLabel.ActionLine(pos0.X, 50);
            {
                var Vpos = AmidaView.VerticalOffset;
                if (animationColtrol.MoveTo(pos0.X, pos0.Y - Vpos + AmidaGoalCanvas.Height, 5))
                {
                    scroll = false; // Reset the scroll flag
                    autoEvent = true; // Set the autoEvent to true to indicate animation is in progress
                    while (autoEvent)
                    {
                        //if (scroll)
                        //{
                        //    scroll = false; // Reset the scroll flag
                        //    Vpos = AmidaView.VerticalOffset;
                        //    if (!animationColtrol.MoveTo(pos0.X, pos0.Y - Vpos + AmidaGoalCanvas.Height / 2, 5))
                        //    {
                        //        break;
                        //    }
                        //}
                        //AmidaView.ScrollToVerticalOffset(scrollOffset - AmidaView.Actualheight / 3);
                        await Task.Delay((int)m_AnimationSpeed);
                    }
                }
                animationColtrol.SetAnimationSpeed(300);
            }
            var axisLine = route.GetAxis(pos0);

            // Lucky Goal Check
            if (resultLabel.CheckGoal(axisLine.Id, SelectedAxisId + 1))
            {
                isGoal = true;
                ResultMessage.Content = " Congratulations!\nYou found the goal!";
                soundControl.PlayID(SoundControl.ID.Connect);
            }
            else
            {
                ResultMessage.Content = "That's too bad.";
                animationColtrol.PauseAnimation();
                soundControl.PlayID(SoundControl.ID.LineNG);
            }

            // Fixing the syntax error in the ternary operator assignment.  
            ResultMessage.Background = isGoal ? new SolidColorBrush(Color.FromArgb(0x80, 0x90, 0xFF, 0x90)) : new SolidColorBrush(Color.FromArgb(0x80, 0xF0, 0x80, 0x80));

            // All Done Check
            if (stickLabel.ColorChangeEndMark(SelectedAxisId))
            {
                isGoal = true;
            }

            ResultMessage.Visibility = Visibility.Visible;
            process = Process.Selecting;
            this.ResizeMode = ResizeMode.CanResize;
            AxisResetButton.IsEnabled = isGoal;
        }

        private void AmidaView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            scroll = true; // Set the scroll flag to true when the scroll changes
        }

        private void AnimationDone(object? sender, EventArgs e)
        {
            cancelSource.Cancel(); // Cancel any ongoing tasks
            autoEvent = false; // Reset the autoEvent to false when the animation is done
        }
    }
}