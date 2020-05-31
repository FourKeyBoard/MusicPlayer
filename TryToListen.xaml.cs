using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPF_Player
{
    /// <summary>
    /// TryToListen.xaml 的交互逻辑
    /// </summary>
    /// 
    public static class DispatcherHelper
    {
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrames), frame);
            try { Dispatcher.PushFrame(frame); }
            catch (InvalidOperationException) { }
        }
        private static object ExitFrames(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }
    }
    public partial class TryToListen : Window
    {
        public TryToListen()
        {
            InitializeComponent();
        }

        public string mp3url = "";
        public string name = "";
        public string singer = "";
        public System.Windows.Forms.Integration.WindowsFormsHost hostBox = new System.Windows.Forms.Integration.WindowsFormsHost();
        public AxWMPLib.AxWindowsMediaPlayer axWmpBox = new AxWMPLib.AxWindowsMediaPlayer();


        //判断歌曲能否试听
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hostBox.Child = axWmpBox;
            this.sp1.Children.Add(hostBox);
            axWmpBox.URL = mp3url;
            this.rollingText.Text = name + "-" + singer;
            //axWmpBox.URL = @"D:\音乐\艾索 - 嘘 [n982].mp3";
            //暂停2.5秒，判断歌曲是否在播放
            var t = DateTime.Now.AddMilliseconds(2500);
            while (DateTime.Now < t)
                DispatcherHelper.DoEvents();
            if (axWmpBox.playState != WMPLib.WMPPlayState.wmppsPlaying)
            {
                System.Windows.Forms.MessageBox.Show("该歌曲无法试听！");
            }
        }
        //关闭播放器
        private void Window_Closed(object sender, EventArgs e)
        {
            axWmpBox.Ctlcontrols.stop();
        }
    }
}
