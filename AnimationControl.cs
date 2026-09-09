using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AmidaAmida
{
    internal class AnimationControl
    {
        static object lockObject = new();
        public delegate void AnimationDelegate(object? sender, EventArgs e);
        public AnimationDelegate? AnimationHandle { get; set; }

        // This class controls the animation on a given canvas.
        // It provides methods to start, stop, pause, resume, and manipulate the animation.
        // The canvas parameter is used to draw the animation.
        public enum Direction
        {
            Down,
            Left,
            Right,
            Up,
        };

        private readonly Canvas m_canvas;
        private int m_totalFrames = 3; // Total number of frames in the sprite sheet
        private int m_frameWidth = 16; // Width of each frame
        private int m_frameHeight = 20; // Height of each frame
        private BitmapImage? m_spriteSheet;
        private readonly DispatcherTimer m_timer;
        private Image m_Image = new();

        private int m_currentFrame = 0;
        private Direction m_direction = Direction.Down;
        private double m_PosX = 0;
        private double m_PosY = 0;
        private double m_OffsetX = 0;
        private double m_OffsetY = 0;
        private double m_ToX = 0;
        private double m_ToY = 0;
        private double m_Speed = 0.1;
        private double m_DeltaX = 0;
        private double m_DeltaY = 0;

        private int IntDirection => m_direction switch
        {
            Direction.Down => 0,
            Direction.Left => 1,
            Direction.Right => 2,
            Direction.Up => 3,
            _ => 0
        };

        readonly int[] stroke = [1, 0, 1, 2];

        public static bool CheckAnimationCharacter(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return false; // Invalid file name or file does not exist
            }
            try
            {
                var spriteSheet = new BitmapImage(new Uri(fileName, UriKind.Relative));
                var frameWidth = (int)(spriteSheet.Width / 3);
                var frameHeight = (int)(spriteSheet.Height / 4);
                var image = new Image()
                {
                    Width = frameWidth,
                    Height = frameHeight,
                };
                var rect = new Int32Rect(
                    2 * frameWidth,
                    3 * frameHeight,
                    frameWidth,
                    frameHeight);
                var croppedBitmap = new CroppedBitmap(spriteSheet, rect);
                image.Source = croppedBitmap;
                image = null; // Release the old image
                croppedBitmap = null; // Release the old cropped bitmap
                spriteSheet = null; // Release the old sprite sheet
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sprite sheet: {ex.Message}");
                return false; // Failed to load the sprite sheet
            }
            return true; // Successfully set the character
        }

        public AnimationControl(ref Canvas canvas, string fileName)
        {
            m_canvas = canvas;
            bool loopSw = true;
            do
            {
                try
                {
                    var defaultPath = Path.Combine(System.Environment.CurrentDirectory, "resource");
                    var animeFile = Path.Combine(defaultPath, fileName);
                    {
                        m_spriteSheet = new BitmapImage(new Uri(animeFile, UriKind.Relative));
                        m_frameWidth = (int)(m_spriteSheet.Width / 3);
                        m_frameHeight = (int)(m_spriteSheet.Height / 4);
                        m_Image = new Image()
                        {
                            Width = m_frameWidth,
                            Height = m_frameHeight,
                        };
                        loopSw = false;
                    }
                }
                catch (Exception ex)
                {
                    if (fileName == "DefaultAnime.png")
                    {
                        MessageBox.Show($"Error loading sprite sheet: {ex.Message}");
                        // Exit Application if default animation fails
                        Environment.Exit(1);
                    }
                    fileName = "DefaultAnime.png";
                }
            } while (loopSw);
            m_timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300) // Frame duration
            };
            m_timer.Tick += AnimateSprite;
        }

        private void AnimateSprite(object? sender, EventArgs e)
        {
            var rect = new Int32Rect(
                stroke[m_currentFrame] * m_frameWidth,
                IntDirection * m_frameHeight,
                m_frameWidth,
                m_frameHeight);
            var croppedBitmap = new CroppedBitmap(m_spriteSheet, rect);
            m_Image.Source = croppedBitmap;
            m_currentFrame = (m_currentFrame + 1) % stroke.Length;

            lock (lockObject)
            {
                if (m_PosX != m_ToX || m_PosY != m_ToY)
                {
                    m_PosX += m_DeltaX * m_Speed;
                    m_PosY += m_DeltaY * m_Speed;
                    if (Math.Abs(m_PosX - m_ToX) <= m_Speed && Math.Abs(m_PosY - m_ToY) <= m_Speed)
                    {
                        m_PosX = m_ToX;
                        m_PosY = m_ToY;
                        m_DeltaX = 0;
                        m_DeltaY = 0;
                        AnimationHandle?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            m_Image.SetValue(Canvas.LeftProperty, m_PosX - m_frameWidth / 2 + m_OffsetX);
            m_Image.SetValue(Canvas.TopProperty, m_PosY - m_frameHeight + m_OffsetY);
            m_canvas.Children.Clear(); // Clear previous images
            m_canvas.Children.Add(m_Image);
        }

        public void StartAnimation()
        {
            m_Image.Visibility = Visibility.Visible;
            m_timer.Start();
        }
        public void StopAnimation()
        {
            m_timer.Stop();
            m_Image.Visibility = Visibility.Hidden;
        }
        public void PauseAnimation()
        {
            m_timer.Stop();
        }
        public void ResumeAnimation()
        {
            m_timer.Start();
        }
        public void SetAnimationSpeed(double speed)
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(speed); // Frame duration
        }
        public void ResetAnimation()
        {
            m_currentFrame = 1; // Reset to the first frame
        }
        public void JumpToFrame(Direction direction)
        {
            m_direction = direction;
        }
        public void SetPosition(double X, double Y)
        {
            lock (lockObject)
            {
                m_ToX = m_PosX = X;
                m_ToY = m_PosY = Y;
                m_Image.SetValue(Canvas.LeftProperty, m_PosX - m_frameWidth / 2 + m_OffsetX);
                m_Image.SetValue(Canvas.TopProperty, m_PosY - m_frameHeight + m_OffsetY);
            }
        }
        public void SetOffset(double X, double Y)
        {
            lock (lockObject)
            {
                m_OffsetX = X;
                m_OffsetY = Y;
            }
        }
        public bool MoveTo(double X, double Y, double speed)
        {
            lock (lockObject)
            {
                m_ToX = X;
                m_ToY = Y;
                m_Speed = speed;

                if (m_PosX == m_ToX && m_PosY == m_ToY)
                {
                    return false; // No movement needed
                }
                else
                {
                    m_DeltaX = m_ToX - m_PosX;
                    m_DeltaY = m_ToY - m_PosY;
                    double distance = Math.Sqrt(m_DeltaX * m_DeltaX + m_DeltaY * m_DeltaY);
                    if (distance > 0)
                    {
                        // Normalize the direction vector
                        m_DeltaX /= distance;
                        m_DeltaY /= distance;
                    }
                    if (m_DeltaX < 0)
                    {
                        m_direction = Direction.Left;
                    }
                    else if (m_DeltaX > 0)
                    {
                        m_direction = Direction.Right;
                    }
                    else if (m_DeltaY < 0)
                    {
                        m_direction = Direction.Up;
                    }
                    else
                    {
                        m_direction = Direction.Down;
                    }
                }
                return true; // Movement initiated
            }
        }
        private void AnimationDone(object? sender, EventArgs e) { }
    }
}
