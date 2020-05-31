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

namespace WPF_Player
{
    /// <summary>
    /// ModifySongList.xaml 的交互逻辑
    /// </summary>
    public partial class ModifySongList : Window
    {
        public string text01;
        public string text02;

        public ModifySongList()
        {

            InitializeComponent();
        }
        //界面加载
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textbox01.Text = text01;
            this.textbox02.Text = text02;

        }
        //保存用户对于标签的修改
        private void save_Click(object sender, RoutedEventArgs e)
        {
            MainWindow w = this.Owner as MainWindow;
            text01 = this.textbox01.Text;
            text02 = this.textbox02.Text;
            if (MainWindow.s2 == "我喜欢的音乐")
            {
                MainWindow.texts01[2].T1 = text01;
                MainWindow.texts01[2].T2 = text02;
                w.text01.Text = text01;
                w.text02.Text = text02;
            }
            else if (MainWindow.s2 == "我的下载")
            {
                MainWindow.texts01[1].T1 = text01;
                MainWindow.texts01[1].T2 = text02;
                w.text01.Text = text01;
                w.text02.Text = text02;
                //App.DoEvents();
            }
            else
            {
                MainWindow.texts01[0].T1 = text01;
                MainWindow.texts01[0].T2 = text02;
                w.text01.Text = text01;
                w.text02.Text = text02;
            }
            ListText.listTexts = MainWindow.texts01;
            string s1 = "s.xml";
            ListText.Export(s1);
            this.Close();
            //MessageBox.Show(text01 + text02);
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //MainWindow mainWindow = new MainWindow();
        }
    }
}
