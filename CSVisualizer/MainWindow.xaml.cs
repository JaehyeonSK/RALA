using CSVisualizer.Modules;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLoadClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "csharp source file|*.cs";

            if(ofd.ShowDialog() == true)
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
                ActualExecution = false
            };

            core.Terminated += () =>
            {
                btnStart.Click -= btnStopHandler;
                btnStart.Click += btnStartClick;
                Dispatcher.Invoke(() =>
                {
                    btnStart.Content = "Start";
                    btnStart.IsEnabled = true;
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
    }
}
