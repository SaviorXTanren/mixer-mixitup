using Mixer.Base.Model.Interactive;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MixItUp.WPF.Controls.Interactive
{
    /// <summary>
    /// Interaction logic for InteractiveBoardControl.xaml
    /// </summary>
    public partial class InteractiveBoardControl : UserControl
    {
        private const int LargeWidth = 80;
        private const int LargeHeight = 22;

        private const int MediumWidth = 40;
        private const int MediumHeight = 25;

        private const int SmallWidth = 30;
        private const int SmallHeight = 40;

        private InteractiveSceneModel scene;

        private bool[,] board;

        public InteractiveBoardControl()
        {
            InitializeComponent();

            this.Loaded += InteractiveBoardControl_Loaded;
            this.SizeChanged += InteractiveBoardControl_SizeChanged;
        }

        public void RefreshScene(int width, int height)
        {
            int perBlockWidth = (int)this.ActualWidth / width;
            int perBlockHeight = (int)this.ActualHeight / height;
            int blockWidthHeight = Math.Min(perBlockWidth, perBlockHeight);

            this.board = new bool[width, height];
            foreach (InteractiveButtonControlModel control in this.scene.buttons)
            {
                this.BlockOutControlArea(control);
            }

            foreach (InteractiveJoystickControlModel control in this.scene.joysticks)
            {
                this.BlockOutControlArea(control);
            }

            this.InteractiveBoard.Children.Clear();
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    if (!this.board[w,h])
                    {
                        Rectangle rect = new Rectangle();
                        rect.Stroke = Brushes.Blue;
                        rect.StrokeThickness = 1;
                        rect.Width = rect.Height = blockWidthHeight;
                        Canvas.SetLeft(rect, w * blockWidthHeight);
                        Canvas.SetTop(rect, h * blockWidthHeight);
                        this.InteractiveBoard.Children.Add(rect);
                    }
                }
            }
        }

        private void BlockOutControlArea(InteractiveControlModel control)
        {
            InteractiveControlPositionModel position = control.position.FirstOrDefault(p => p.size.Equals("large"));
            for (int w = position.x; w < position.width; w++)
            {
                for (int h = position.y; h < position.height; h++)
                {
                    this.board[w,h] = true;
                }
            }
        }

        private void InteractiveBoardControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                this.scene = (InteractiveSceneModel)this.DataContext;
                this.RefreshScene(LargeWidth, LargeHeight);
            }
        }

        private void InteractiveBoardControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.scene != null)
            {
                this.RefreshScene(LargeWidth, LargeHeight);
            }
        }
    }
}
