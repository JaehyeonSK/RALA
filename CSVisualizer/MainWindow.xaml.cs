using CSVisualizer.Modules;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CSVisualizer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        Core core;
        GuiHandler guiHandler;
        string path;

        List<Snackbar> snacks;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLoadClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "csharp source file|*.cs";

            if (ofd.ShowDialog() == true)
            {
                tbCodeName.Text = ofd.FileName;
                textCode.Text = File.ReadAllText(ofd.FileName);
                path = ofd.FileName;
            }
        }

        private void btnStartClick(object sender, RoutedEventArgs e)
        {
            int interval;
            if (!int.TryParse(textInterval.Text, out interval))
            {
                MessageBox.Show("Interval field content must be integer number.");
                return;
            }

            if (string.IsNullOrEmpty(textCode.Text))
            {
                MessageBox.Show("Load your C# source code");
                return;
            }

            rootCanvas.Children.Clear();
            textLog.Clear();
            codeArea.Children.Clear();

            chkAutoStep.IsEnabled = false;
            chkShowRef.IsEnabled = false;
            textInterval.IsEnabled = false;

            snacks = new List<Snackbar>();
            guiHandler = new GuiHandler(this);
            guiHandler.Init();

            RoutedEventHandler btnStopHandler = (s, ev) =>
            {
                // Core가 완전히 종료될 때까지 비활성화
                btnStart.IsEnabled = false;
                core.Stop();
            };

            core = new Core(int.TryParse(textInterval.Text, out interval) ? interval : 0)
            {
                ActualExecution = false,
                ManualMode = (chkAutoStep.IsChecked == null) ? false : !((bool)chkAutoStep.IsChecked)
            };

            core.StepEnabled += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    btnStep.IsEnabled = true;
                });
            };

            core.CodeUnitSent += NewCodeSnackbar;

            core.Terminated += () =>
            {
                btnStart.Click -= btnStopHandler;
                btnStart.Click += btnStartClick;
                Dispatcher.Invoke(() =>
                {
                    btnStart.Content = "Start";
                    btnStart.IsEnabled = true;

                    btnStep.IsEnabled = false;

                    textInterval.IsEnabled = (chkAutoStep.IsChecked == null) ? false : ((bool)chkAutoStep.IsChecked);
                    chkAutoStep.IsEnabled = true;
                    chkShowRef.IsEnabled = true;

                    int ms = 30;
                    foreach (var snack in snacks)
                    {
                        Task.Delay(ms).ContinueWith(_ =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Duration duration = new Duration(new TimeSpan(0, 0, 0, 0, 330));
                                DoubleAnimation da = new DoubleAnimation(Canvas.GetLeft(snack), -snack.ActualWidth * 2, duration);

                                snack.BeginAnimation(Canvas.LeftProperty, da);
                            });
                        });
                        ms += 30;
                    }
                });
            };

            ThreadPool.QueueUserWorkItem((o) =>
            {
                core.Start(path);
            }, null);

            // Stop 버튼으로 바뀐다.
            btnStart.Click -= btnStartClick;
            btnStart.Click += btnStopHandler;
            btnStart.Content = "Stop";
        }

        private void NewCodeSnackbar(string code)
        {
            Dispatcher.Invoke(() =>
            {
                Snackbar snack = new Snackbar();
                snack.Message = new SnackbarMessage() { Content = code };
                snack.Margin = new Thickness(0, 0, 0, 5);
                Canvas.SetBottom(snack, 5);
                Canvas.SetLeft(snack, 20);
                codeArea.Children.Add(snack);

                UpdateList(snack);

                snack.IsActive = true;
            });
        }

        private void UpdateList(Snackbar newSnack)
        {
            int ms = (chkAutoStep != null && chkAutoStep.IsChecked == true) ?
                        (int)(int.Parse(textInterval.Text) * 0.2) : 200;
            Duration duration = new Duration(new TimeSpan(0, 0, 0, 0, ms));

            const double HEIGHT = 50;
            foreach (var snack in snacks)
            {
                DoubleAnimation da = new DoubleAnimation(Canvas.GetBottom(snack), Canvas.GetBottom(snack) + HEIGHT + snack.Margin.Bottom, duration);
                da.AccelerationRatio = 0.01;

                if (Canvas.GetBottom(snack) + HEIGHT * 2 > codeArea.ActualHeight)
                {
                    da.Completed += (s, ev) =>
                    {
                        snack.IsActive = false;
                        snacks.Remove(snack);

                        Task.Delay(1000).ContinueWith(_ => 
                            Dispatcher.Invoke(()=>codeArea.Children.Remove(snack))
                        );
                    };
                }

                snack.BeginAnimation(Canvas.BottomProperty, da);
            }

            snacks.Add(newSnack);
        }

        private void AutoStepChecked(object sender, RoutedEventArgs e)
        {
            if (textInterval == null)
                return;

            textInterval.IsEnabled = true;
        }

        private void AutoStepUnchecked(object sender, RoutedEventArgs e)
        {
            if (textInterval == null)
                return;

            textInterval.Text = "0";
            textInterval.IsEnabled = false;
        }

        private void btnStepClick(object sender, RoutedEventArgs e)
        {
            core.Step();
            btnStep.IsEnabled = false;
        }
    }
}
