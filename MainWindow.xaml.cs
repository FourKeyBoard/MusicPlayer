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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading;
using System.Text.RegularExpressions;
using WPF_Player.Services;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace WPF_Player
{
    using IOPath = System.IO.Path;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

        }
        enum PlayerStatus
        {
            NerverStart = 1,
            Start,
            Pause,
            Stop
        }
       
        private EntityInterface EntityService = new EntityServiceImpl();
        public string text { set; get; }
        private bool move = false;
        MyPlayer player = new MyPlayer();
        MediaPlayer playerhandle = null;
        PlayerStatus currentStatus = PlayerStatus.NerverStart;
        PlayerModel currentModel = PlayerModel.sequence;
        ObservableCollection<Song> playList = new ObservableCollection<Song>();
        private int currentIndex = 0;
      
       
       
       
        public string t01;
        public string t02;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.rollingText.Text = "音乐播放器处于暂停状态";
            
       
            this.listBox.DataContext = playList;
            playerhandle = player.GetPlayerHandle();
            playerhandle.MediaEnded += playerhandle_MediaEnded;
            playerhandle.MediaFailed += playerhandle_MediaFailed;
            playerhandle.MediaOpened += playerhandle_MediaOpened;
            player.playEvent += player_playEvent;
            player.playEvent_thread += player_playEvent_thread;

            this.slider.DataContext = player;


            //读取配置
            string folderPath = ConfigurationManager.AppSettings["folderPath"];
            folderPath = IOPath.GetFullPath(folderPath);
            if (!Directory.Exists(folderPath))
                return;
            EntityService.GetMusicFiles(folderPath).ToList().ForEach(x => playList.Add(new Song() { Location = x }));
          
          
          
        }
       
       
        void player_playEvent_thread(object obj)
        {
            double PlayedTime = (double)(this.Dispatcher.Invoke((Func<double>)(() => { return playerhandle.Position.TotalMilliseconds; })));
            List<Lynic> lstLy = player.CurrentSong.LstLynic;
            ShowLynic(lstLy, PlayedTime);
        }
        int tempValue = 0;//当达到500时触发界面更新进度
        int lastValue;
        void player_playEvent(object sender)
        {
            if (!playerhandle.NaturalDuration.HasTimeSpan)
            {
                return;
            }
            double PlayedTime = playerhandle.Position.TotalSeconds;
            double totalTime = playerhandle.NaturalDuration.TimeSpan.TotalSeconds;

            this.PresentTime.Text = playerhandle.Position.ToString("mm\\:ss");//当前播放秒数
            this.TotalTime.Text = playerhandle.NaturalDuration.TimeSpan.ToString("mm\\:ss");//最大秒数
            //this.lable.Content = string.Format("{0}/{1}", playerhandle.Position.ToString("mm\\:ss"), playerhandle.NaturalDuration.TimeSpan.ToString("mm\\:ss"));
            if (lastValue == (int)PlayedTime)
                return;
            this.trackBar.MaxValue = totalTime;
            this.trackBar.CurrentValue = PlayedTime;
            lastValue = (int)this.trackBar.CurrentValue;
        }
      
        //滚动条显示当前播放的歌
        void playerhandle_MediaOpened(object sender, EventArgs e)
        {
            this.rollingText.Text = "当前播放：" + player.CurrentSong.Name;
        }

        void playerhandle_MediaFailed(object sender, ExceptionEventArgs e)
        {

        }
        //播放模式切换
        void playerhandle_MediaEnded(object sender, EventArgs e)
        {
            if (playList.Count > 0)
            {
                if (player.CurrentSong.LstLynic != null)
                {
                    player.CurrentSong.LstLynic.ForEach(x => x.IsShow = false);
                }
                PlayNext();
               
            }
        }
        

       
        //选择一首歌然后点击播放按钮
        private void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            //1,暂停播放
            //2,从一首新歌播放
            if (currentStatus == PlayerStatus.Pause)
            {
                Song song = this.listBox.SelectedItem as Song;
                singer_get(song);
                currentIndex = this.listBox.SelectedIndex;
                if (song != player.CurrentSong)
                    player.CurrentSong = song;
                player.Play();
                currentStatus = PlayerStatus.Start;
                this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPause"]);
            }
            else if (currentStatus == PlayerStatus.NerverStart)
            {
                if (this.listBox.SelectedIndex < 0)
                {
                    this.rollingText.Text = "请选择一首歌曲然后播放！";
                    return;
                }
                Song song = this.listBox.SelectedItem as Song;
                singer_get(song);
                currentIndex = this.listBox.SelectedIndex;
                player.CurrentSong = song;
                player.Play();
                currentStatus = PlayerStatus.Start;
                this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPause"]);
            }
            else if (currentStatus == PlayerStatus.Start)
            {
                player.Pause();
                currentStatus = PlayerStatus.Pause;
                this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPlay"]);
            }

        }
       
        private void btn_Previous_Click(object sender, RoutedEventArgs e)
        {
            if (playList.Count > 0)
                PlayPrevious();
        }
        
        void PlayPrevious()
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = playList.Count - 1;
            this.listBox.SelectedIndex = currentIndex;
            player.CurrentSong = playList[currentIndex];
            player.Play();
            currentStatus = PlayerStatus.Start;
        }
        
        //播放下一首
        void PlayNext()
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            currentIndex++;
            if (currentIndex > playList.Count - 1)
                currentIndex = 0;
            this.listBox.SelectedIndex = currentIndex;
            player.CurrentSong = playList[currentIndex];
            foreach (var item in this.lynicBoard.Items)
            {
                (item as ListBoxItem).Content = "";
            }
            player.Play();
            currentStatus = PlayerStatus.Start;

        }
        
        //下一首
        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            if (playList.Count > 0)
                PlayNext();
        }
        //导入歌曲
        private void loadSong_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "请选择歌曲的文件夹";
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            string folderfName = fbd.SelectedPath;
            string[] mp3Files = EntityService.GetMusicFiles(folderfName);
            Configuration c = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            c.AppSettings.Settings["folderPath"].Value = folderfName;
            c.Save();
            if (mp3Files != null)
            {
                playList.Clear();
                mp3Files.ToList().ForEach(x => playList.Add(new Song() { Location = x }));
                //获取歌词
                foreach (Song s in playList)
                {
                    s.LstLynic = GetLynicBySong(s);
                }
            }
        }
        //删除歌曲，删除本地文件
        private void removeSong_click(object sender, RoutedEventArgs e)
        {
            
            DialogResult dr = System.Windows.Forms.MessageBox.Show("确认删除？", "此删除不可恢复", MessageBoxButtons.OKCancel);
            if (dr ==System.Windows.Forms.DialogResult.OK)
            {
                Song song = listBox.SelectedItem as Song;
                File.Delete(song.Location);
            }
            this.playList.Remove(listBox.SelectedItem as Song);
            int i = 0;
            foreach (Song s in playList)
            {
                i++;
            }
            this.num.Text = i.ToString();

        }
        
       
        //双击播放
    private void listBox_MouseDoubleClick(object sender, EventArgs e)
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            Song song = this.listBox.SelectedItem as Song;
            singer_get(song);
           
            currentIndex = this.listBox.SelectedIndex;
            player.CurrentSong = song;
            player.Play();
            currentStatus = PlayerStatus.Start;
            this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPause"]);
        }

        private void trackBar_PlayProcessChanged(double obj)
        {
            if (currentStatus == PlayerStatus.Start || currentStatus == PlayerStatus.Pause)
            {
                playerhandle.Position = TimeSpan.FromSeconds(obj);
            }
        }
       
        
        private void lb_TitleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            move = false;
        }
        private void lb_TitleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            move = true;
        }
        private void lb_TitleMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(0);
        }
        private void miniButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }
       
       
    }
}
