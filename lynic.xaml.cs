using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPF_Player
{
    /// <summary>
    /// lynic.xaml 的交互逻辑
    /// </summary>
    public partial class lynic : Window
    {
        DispatcherTimer timer = new DispatcherTimer();//定时器
        bool move;
        public static string s;
        public lynic()
        {
            InitializeComponent();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = TimeSpan.FromSeconds(0.1);   //设置刷新的间隔时间
            timer.Start();
        }
        int mm = 1;
        private void timer_Tick(object sender, EventArgs e)
        {
            mm++;
            this.rollingText.Text = s;

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MiniButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void title_MouseDown(object sender, MouseButtonEventArgs e)
        {
            move = true;
        }

        private void title_MouseUp(object sender, MouseButtonEventArgs e)
        {
            move = false;
        }

        private void title_MouseMove(object sender, MouseEventArgs e)
        {
            if (move)
            {
                try
                {
                    this.DragMove();
                }
                catch (Exception)
                { }
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
