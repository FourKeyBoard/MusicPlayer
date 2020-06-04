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
using Shell32;
using System.Xml;

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
        enum PlayerModel
        {
            sequence = 1,
            looping,
            random

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
        //将XML文件中的内容，保存到texts01中
        public static string s1 = "s.xml";
        public static List<ListText> texts01 = ListText.Import(s1);
        public static string s2;
        //第一次输出XML文件
        //public List<ListText> texts = new List<ListText>
        //{
        //    new ListText("默认", "导入歌曲默认歌单"),
        //    new ListText("下载", "下载歌曲默认歌单"),
        //    new ListText("喜欢", "我喜欢的音乐")
        //};
        public string t01;
        public string t02;
        public lynic lynic01 = new lynic();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.rollingText.Text = "音乐播放器处于暂停状态";
            //第一次生成XML文件，第二次注释掉
            //ListText.listTexts = texts;
            //string s1 = "s.xml";
            //ListText.Export(s1);
            //List<ListText> texts01 = ListText.Import(s1);
            //System.Windows.MessageBox.Show(texts01[0].T1);
            SearchText.DataContext = this;
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
            //获取歌词和歌曲数
            int i = 0;
            foreach (Song s in playList)
            {
                singer_get(s);
                s.LstLynic = GetLynicBySong(s);
                i++;
            }
            this.songSheet.Text = "默认歌单";
            s2 = this.songSheet.Text;
            this.num.Text = i.ToString();//设置歌曲数
            this.text01.Text = texts01[0].T1;//设置歌单标签
            this.text02.Text = texts01[0].T2;//设置歌单简介
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
        //显示歌词
        private void ShowLynic(List<Lynic> lstLy, double playTime)
        {

            if (lstLy == null || lstLy.Count == 0)
            {
                return;
            }
            int index = player.CurrentSong.LstLynic.FindIndex(0, lstLy.Count, x => playTime - x.MiniSencond < 100 && playTime - x.MiniSencond >= 0);
            if (index == -1)
                return;
            else//满足切换歌词的条件
            {
                #region ~~~
                this.Dispatcher.Invoke((Action)(() =>
                {            
                    //第二行歌词
                    System.Windows.Controls.ListBoxItem item2 = lynicBoard.Items[2] as System.Windows.Controls.ListBoxItem;
                    item2.Content = lstLy[index].Content;
                    lynic.s = lstLy[index].Content;
                    //lynic01.rollingText.Content = lstLy[index].Content;
                    if (index - 1 >= 0)
                    {
                        //第1行歌词
                        System.Windows.Controls.ListBoxItem item1 = lynicBoard.Items[1] as System.Windows.Controls.ListBoxItem;
                        item1.Content = lstLy[index - 1].Content;
                    }
                    if (index - 2 >= 0)
                    {
                        //第0行歌词
                        System.Windows.Controls.ListBoxItem item0 = lynicBoard.Items[0] as System.Windows.Controls.ListBoxItem;
                        item0.Content = lstLy[index - 2].Content;
                    }
                    if (index + 1 < lstLy.Count)
                    {
                        //第3行歌词
                        System.Windows.Controls.ListBoxItem item3 = lynicBoard.Items[3] as System.Windows.Controls.ListBoxItem;
                        item3.Content = lstLy[index + 1].Content;
                    }
                    if (index + 2 < lstLy.Count)
                    {
                        //第4行歌词
                        System.Windows.Controls.ListBoxItem item4 = lynicBoard.Items[4] as System.Windows.Controls.ListBoxItem;
                        item4.Content = lstLy[index + 2].Content;
                    }
                    if (index == lstLy.Count - 1)//歌词到了最后一行
                    {
                        System.Windows.Controls.ListBoxItem item3 = lynicBoard.Items[3] as System.Windows.Controls.ListBoxItem;
                        System.Windows.Controls.ListBoxItem item4 = lynicBoard.Items[4] as System.Windows.Controls.ListBoxItem;
                        item3.Content = "";
                        item4.Content = "";
                    }
                    if (index == lstLy.Count - 2)//歌词到了倒数第二行
                    {

                        System.Windows.Controls.ListBoxItem item4 = lynicBoard.Items[4] as System.Windows.Controls.ListBoxItem;
                        item4.Content = "";
                    }
                }));

                #endregion
            }
        }
        //滚动条显示当前播放的歌
        void playerhandle_MediaOpened(object sender, EventArgs e)
        {
            this.SingerTextBlock.Text = player.CurrentSong.Singer;
            this.SongNameTextBlock.Text = player.CurrentSong.Name;
            this.rollingText.Text = "当前播放：" + player.CurrentSong.Name+"-"+ player.CurrentSong.Singer;
            lynic.s = this.rollingText.Text;
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
                if (currentModel == PlayerModel.sequence) { PlayNext(); }
                else if (currentModel == PlayerModel.looping) { PlayAgain(); }
                else { PlayRandomly(); }
            }
        }
        //获取歌手
        private void singer_get(Song song)
        {
            string filename = song.Location;
            MP3 mp3 = new MP3();
            Mp3Info mp3Info = mp3.getMP3Info(filename);
            if (mp3Info.Title != "")
            {
                song.Name = mp3Info.Title;
            }
            if (mp3Info.Artist != "")
            {
                song.Singer = mp3Info.Artist;
            }
            else
            {
                song.Singer = "未知歌手";
            }
            if (mp3Info.Album != "")
            {
                song.Album = mp3Info.Album;
            }
            else
            {
                song.Album = "未知专辑";
            }
        }


        private void SearchClick(object sender, RoutedEventArgs e) {
            text = SearchText.Text;
           
            
         ObservableCollection<Song>   playList2 = new ObservableCollection<Song>();
            foreach (Song s in playList) {
                if (s.Name.IndexOf(""+text)!=-1) {
                    playList2.Add(s);
                }
            }
          
            playList.Clear();
            foreach (Song s in playList2) { playList.Add(s); }
           
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
        //随机播放
        private void btn_Previous_Click(object sender, RoutedEventArgs e)
        {
            if (playList.Count > 0)
                PlayPrevious();
        }
        //循环播放
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
        //单曲循环
        void PlayAgain()
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            this.listBox.SelectedIndex = currentIndex;
            player.CurrentSong = playList[currentIndex];
            foreach (var item in this.lynicBoard.Items)
            {
                (item as ListBoxItem).Content = "";
            }
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
        //随机播放
        void PlayRandomly()
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            Random rd = new Random();
            currentIndex = rd.Next(0, playList.Count);
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
                    singer_get(s);
                    s.LstLynic = GetLynicBySong(s);
                }
            }
        }
        //删除歌曲，删除本地文件
        private void removeSong_click(object sender, RoutedEventArgs e)
        {

            DialogResult dr = System.Windows.Forms.MessageBox.Show("确认删除？", "此删除不可恢复", MessageBoxButtons.OKCancel);
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                Song song = listBox.SelectedItem as Song;
                singer_get(song);
                File.Delete(song.Location);
                //同时删除xml中的节点
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("Songs.xml");
                XmlNode rootNode = xmlDoc.SelectSingleNode("ArrayOfSong");
                foreach (XmlNode xmlNode in rootNode.ChildNodes)
                {
                    if (xmlNode.SelectSingleNode("Name").InnerText == song.Name)
                    {
                        xmlNode.ParentNode.RemoveChild(xmlNode);
                    }
                }
                xmlDoc.Save("Songs.xml");
            }
            this.playList.Remove(listBox.SelectedItem as Song);
            int i = 0;
            foreach (Song s in playList)
            {
                i++;
            }
            this.num.Text = i.ToString();

        }
        //加载默认歌单
        private void loadmymusiclist(object sender, RoutedEventArgs e)
        {

            string folderPath = ConfigurationManager.AppSettings["folderPath"];
            folderPath = IOPath.GetFullPath(folderPath);

            string[] mp3Files = EntityService.GetMusicFiles(folderPath);
            if (mp3Files != null)
            {
                playList.Clear();
                mp3Files.ToList().ForEach(x => playList.Add(new Song() { Location = x }));
                //获取歌词和歌曲数
                int i = 0;
                foreach (Song s in playList)
                {
                    singer_get(s);
                    s.LstLynic = GetLynicBySong(s);
                    i++;
                }
                this.num.Text = i.ToString();
            }
            this.songSheet.Text = "默认歌单";
            s2 = this.songSheet.Text;

            this.text01.Text = texts01[0].T1;//设置歌单标签
            this.text02.Text = texts01[0].T2;//设置歌单简介

        }
        //加载我的下载歌单
        public void loadMydownload(object sender, RoutedEventArgs e)
        {

            string folderPath = ConfigurationManager.AppSettings["Mydownload"];
            folderPath = IOPath.GetFullPath(folderPath);

            string[] mp3Files = EntityService.GetMusicFiles(folderPath);
            if (mp3Files != null)
            {
                playList.Clear();
                mp3Files.ToList().ForEach(x => playList.Add(new Song() { Location = x }));
                //获取歌词和歌曲数
                int i = 0;
                foreach (Song s in playList)
                {
                    //singer_get(s);
                    s.LstLynic = GetLynicBySong(s);
                    i++;
                }
                this.num.Text = i.ToString();
            }
            this.songSheet.Text = "我的下载";
            s2 = this.songSheet.Text;

            this.text01.Text = texts01[1].T1;//设置歌单标签
            this.text02.Text = texts01[1].T2;//设置歌单简介
        }
        //加载我喜欢的音乐歌单
        public void loadMyfavorite(object sender, RoutedEventArgs e)
        {

            string folderPath = ConfigurationManager.AppSettings["Myfavorite"];
            folderPath = IOPath.GetFullPath(folderPath);

            string[] mp3Files = EntityService.GetMusicFiles(folderPath);
            if (mp3Files != null)
            {
                playList.Clear();
                mp3Files.ToList().ForEach(x => playList.Add(new Song() { Location = x }));
                //获取歌词和歌曲数
                int i = 0;
                foreach (Song s in playList)
                {
                    singer_get(s);
                    s.LstLynic = GetLynicBySong(s);
                    i++;
                }
                this.num.Text = i.ToString();
            }
            this.songSheet.Text = "我喜欢的音乐";
            s2 = this.songSheet.Text;

            this.text01.Text = texts01[2].T1;//设置歌单标签
            this.text02.Text = texts01[2].T2;//设置歌单简介
        }
        //修改歌单信息
        private void modifySongList_click(object sender, RoutedEventArgs e)
        {
            ModifySongList modifySongList = new ModifySongList();
            //modifySongList.textbox01.Text = this.text01.Text;
            //modifySongList.textbox02.Text = this.text02.Text;
            modifySongList.Owner = this;
            modifySongList.text01 = this.text01.Text;
            modifySongList.text02 = this.text02.Text;
            //this.Close();
            modifySongList.ShowDialog();
        }
        //根据歌曲，获取歌词
        private List<Lynic> GetLynicBySong(Song s)
        {
            string lynicFile1 = IOPath.Combine(IOPath.GetDirectoryName(s.Location), IOPath.GetFileNameWithoutExtension(s.Location) + ".lrc");
            string lynicFile2 = IOPath.Combine(IOPath.GetDirectoryName(s.Location), "Lynic", IOPath.GetFileNameWithoutExtension(s.Location) + ".lrc");
            if (File.Exists(lynicFile1))
            {
                return EntityService.GetLynics(lynicFile1);
            }
            else if (File.Exists(lynicFile2))
            {
                return EntityService.GetLynics(lynicFile2);
            }
            else
                return null;
        }
        //收藏歌曲
        private void starmusic(object sender, EventArgs e)
        {

            Song song = this.listBox.SelectedItem as Song;
            string filename = song.Location;
            //System.Windows.MessageBox.Show(filename);
            string path = filename.Substring(0, filename.LastIndexOf("\\"));
            string file = filename.Substring(filename.LastIndexOf("\\") + 1);

            string folderPath = ConfigurationManager.AppSettings["Myfavorite"];
            folderPath = System.IO.Path.GetFullPath(folderPath);

            string destPath = folderPath + "\\" + file;
            if (!File.Exists(destPath))
            {
                File.Copy(song.Location, destPath);
            }
        }
        //双击播放
    private void listBox_MouseDoubleClick(object sender, EventArgs e)
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            Song song = this.listBox.SelectedItem as Song;
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
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = combobox.SelectedIndex;

            if (index == 0) { currentModel = PlayerModel.looping; }
            if (index == 1) { currentModel = PlayerModel.sequence; }
            if (index == 2) { currentModel = PlayerModel.random; }
        }
        //歌词
        void ShowLynic(List<Lynic> lstLynic)
        {

            new Thread(() =>
            {
                int t = System.Environment.TickCount;
                int notRunningTime = 0;
                for (int i = 0; i < lstLynic.Count; i++)
                {
                    while (currentStatus == PlayerStatus.Pause)
                    {
                        notRunningTime++;//如果暂停，则将暂停的毫秒数记录下来。
                        Thread.Sleep(1);
                    }
                    while (System.Environment.TickCount - t - notRunningTime < lstLynic[i].MiniSencond)
                    {
                        Thread.Sleep(1);
                    }
                    //Console.WriteLine(lstLynic[i].Content);
                    this.Dispatcher.Invoke((Action)(() =>
                        {
                            System.Windows.Controls.ListBoxItem item2 = lynicBoard.Items[2] as System.Windows.Controls.ListBoxItem;
                            item2.Content = lstLynic[i].Content;
                            if (i - 1 >= 0)
                            {
                                System.Windows.Controls.ListBoxItem item1 = lynicBoard.Items[1] as System.Windows.Controls.ListBoxItem;
                                item1.Content = lstLynic[i - 1].Content;
                            }
                            if (i - 2 >= 0)
                            {
                                System.Windows.Controls.ListBoxItem item0 = lynicBoard.Items[0] as System.Windows.Controls.ListBoxItem;
                                item0.Content = lstLynic[i - 2].Content;
                            }
                            if (i + 1 < lstLynic.Count)
                            {
                                System.Windows.Controls.ListBoxItem item3 = lynicBoard.Items[3] as System.Windows.Controls.ListBoxItem;
                                item3.Content = lstLynic[i + 1].Content;
                            }
                            if (i + 2 < lstLynic.Count)
                            {
                                System.Windows.Controls.ListBoxItem item4 = lynicBoard.Items[4] as System.Windows.Controls.ListBoxItem;
                                item4.Content = lstLynic[i + 2].Content;
                            }
                            if (i == lstLynic.Count - 1)//歌词到了最后一行
                            {
                                System.Windows.Controls.ListBoxItem item3 = lynicBoard.Items[3] as System.Windows.Controls.ListBoxItem;
                                System.Windows.Controls.ListBoxItem item4 = lynicBoard.Items[4] as System.Windows.Controls.ListBoxItem;
                                item3.Content = "";
                                item4.Content = "";
                            }
                            if (i == lstLynic.Count - 2)//歌词到了倒数第二行
                            {

                                System.Windows.Controls.ListBoxItem item4 = lynicBoard.Items[4] as System.Windows.Controls.ListBoxItem;
                                item4.Content = "";
                            }
                        }));
                    lstLynic[i].IsShow = true;
                }
            }).Start();
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
        private void ci_Checked(object sender, RoutedEventArgs e)
        {
            listBox.Visibility = Visibility.Collapsed;
            lynicBoard.Visibility = Visibility.Visible;
            SearchBlock.Visibility = Visibility.Collapsed;

            string picurl;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Song>));
            using (FileStream fs = new FileStream("Songs.xml", FileMode.Open))
            {
                List<Song> songs = (List<Song>)xmlSerializer.Deserialize(fs);

                bool flag = false;
                Song song2 = this.listBox.SelectedItem as Song;
                if (song2 != null)
                {
                    foreach (Song s in songs)
                    {
                        if (s.Name == song2.Name)
                        {
                            flag = true;
                            picurl = s.PicUrl;
                            image1.ImageSource = BitmapFrame.Create(new Uri(picurl, false), BitmapCreateOptions.None, BitmapCacheOption.Default);
                        }
                    }
                }
                if (flag == false) { image1.ImageSource = new BitmapImage(
                    new Uri("Pictures\\歌词界面.jpg", UriKind.Relative)
                );
                }
            }

        }
        private void ci_Unchecked(object sender, RoutedEventArgs e)
        {
            listBox.Visibility = Visibility.Visible;
            lynicBoard.Visibility = Visibility.Collapsed;
            SearchBlock.Visibility = Visibility.Visible;
        }
        //下载歌曲
        private void DownloadMusic_Click(object sender, RoutedEventArgs e)
        {
            DownloadMusic downloadMusicWindow = new DownloadMusic(ref player);
            downloadMusicWindow.Title = "下载音乐";
            downloadMusicWindow.Owner = this;
            downloadMusicWindow.ShowDialog();
        }
        private void OpenDesktopLynic_Click(object sender, RoutedEventArgs e)
        {
            lynic lynic1 = new lynic();
            lynic1.Show();
        }
    }
}
