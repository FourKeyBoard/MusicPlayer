using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace WPF_Player
{
    /// <summary>
    /// Download.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class DownloadMusic : Window
    {
        DispatcherTimer timer = new DispatcherTimer();//定时器

        bool move;
        public string text { set; get; }
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        private ObservableCollection<Song> songSearchResults = new ObservableCollection<Song>();
        private System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
        private MyPlayer myPlayer = new MyPlayer();
        public DownloadMusic(ref MyPlayer myPlayer)
        {
            InitializeComponent();
            this.myPlayer = myPlayer;
            searchText.DataContext = this;
            this.listview.DataContext = songSearchResults;
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);

            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = TimeSpan.FromSeconds(0.1);   //设置刷新的间隔时间
            timer.Start();
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
        private void listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            
                if (backgroundWorker.CancellationPending)
                    return;
                WebClient wb = new WebClient();
                byte[] data=null;
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    data = wb.DownloadData("http://s.music.163.com/search/get/?version=1&src=lofter&type=1&s=" + searchText.Text.Trim() + "&limit=90");
                }));
                string urlym = Encoding.GetEncoding("utf-8").GetString(data);

                if (urlym == "{\"code\":200}")
                {
                    if (songSearchResults.Count == 0)
                        MessageBox.Show("没有找到匹配的音乐!");
                    return;
                }
                else
                {
                    dynamic wyyms = JsonConvert.DeserializeObject(urlym);
                    dynamic list = wyyms.result.songs;
                    foreach (dynamic s in list)
                    {
                        Song newSong = new Song
                        {
                            Location = s.audio.ToString(),
                            //  Name = s.album.name.ToString(),
                            Name = s.name.ToString(),
                            Singer = s.artists[0].name.ToString(),
                            Album= s.album.name.ToString(),
                            PicUrl=s.album.picUrl.ToString()
                        };
                        this.listview.Dispatcher.Invoke(DispatcherPriority.Normal,(Action)(() =>
                        {
                            songSearchResults.Add(newSong);
                            listview.Items.Refresh();
                        }));
                    }
                }
        }
        public void downfile(string url, string filename)
        {
            WebClient web = new WebClient();
            web.DownloadFileCompleted += new AsyncCompletedEventHandler(web_DownloadFileCompleted);
            web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(web_DownloadProgressChanged);
            web.DownloadFileAsync(new Uri(url), filename);            
        }
        private void DownloadMusicButton_Click(object sender, EventArgs e)
        {
            if (listview.SelectedItems.Count == 1)
            {
                Song song = listview.SelectedItem as Song;
                string url = "";
                //下载歌曲到项目指定文件夹
                saveFileDialog.Title = "音乐下载";
                saveFileDialog.FileName = song.Name+"-"+song.Singer;
                saveFileDialog.Filter = "mp3文件|*.mp3";
            
                if (saveFileDialog.ShowDialog().ToString() == "OK"&&song!=null&&song is Song)
                {
                    downfile(song.Location, saveFileDialog.FileName);

                    string filename = saveFileDialog.FileName;
                    string path = filename.Substring(0, filename.LastIndexOf("\\"));
                    string file = filename.Substring(filename.LastIndexOf("\\") + 1);
                    
                     string folderPath = ConfigurationManager.AppSettings["Mydownload"];
                    folderPath = System.IO.Path.GetFullPath(folderPath);
                    
                    string destPath = folderPath + "\\" + file;
                    
                    downfile(song.Location, destPath);
                    url = destPath;
                    //File.Copy(saveFileDialog.FileName,destPath);
                }
                //将下载的歌曲写入xml
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<Song>));
                if (!File.Exists("Songs.xml"))
                {
                    ObservableCollection<Song> songs = new ObservableCollection<Song>();
                    song.Location = url;
                    songs.Add(song);
                    using (FileStream fs = new FileStream("Songs.xml", FileMode.Create))
                    {
                        xmlSerializer.Serialize(fs, songs);
                    }
                }
                else
                {
                    ObservableCollection<Song> songs = new ObservableCollection<Song>();
                    using (FileStream fs = new FileStream("Songs.xml", FileMode.Open))
                    {
                         songs=(ObservableCollection<Song>)xmlSerializer.Deserialize(fs);
                         song.Location = url;
                         songs.Add(song);
                    }
                    File.Delete("Songs.xml");
                    using (FileStream fs = new FileStream("Songs.xml", FileMode.Create))
                    {
                        xmlSerializer.Serialize(fs, songs);
                    }
                }
            }
            else if (listview.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择一首歌！");
            }
            else
            {
                MessageBox.Show("只能选择一首歌下载！");
            }
        }
        int mm = 1;
        private void timer_Tick(object sender, EventArgs e)
        {
            mm++;
        }
        //显示下载状态
        void web_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.label.Foreground = new SolidColorBrush(Colors.Red);
                this.label.Content = "下载取消";
            }
            else
            {
                this.label.Foreground = new SolidColorBrush(Colors.Green);
                this.label.Content = "下载完成";
            }
            timer.Stop();
            mm = 0;
        }
        //显示下载速度
        void web_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (((double)e.BytesReceived / mm / 1024) > 1000)
            {
                this.label.Content = "下载速度：" + ((double)e.BytesReceived / mm / 1024 / 1024).ToString("F2") + "mb/s";
            }
            else
            {
                this.label.Content = "下载速度：" + e.BytesReceived / mm / 1024 + "kb/s";
            }
        }


        private void SearchMusicOnNetwork_Click(object sender, EventArgs e)
        {
            if (this.searchText.Text == "")
                return;
            if (backgroundWorker.IsBusy)
            {
                this.backgroundWorker.CancelAsync();
            }
            else
            {
                songSearchResults.Clear();
                listview.Items.Refresh();
                backgroundWorker.RunWorkerAsync();
            }
        }
        //试听音乐
        private void TryToListenSelectedMusic_Click(object sender, EventArgs e)
        {
            if (listview.SelectedItems.Count == 1)
            {
                Song song = listview.SelectedItem as Song;
                TryToListen tryToListen = new TryToListen();
                tryToListen.mp3url = song.Location;
                tryToListen.name = song.Name;
                tryToListen.singer = song.Singer;
                tryToListen.ShowDialog();
            }
            else if (listview.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择一首歌！");
            }
            else
            {
                MessageBox.Show("只能选择一首歌试听！");
            }
        }
        //按回车键也可以开始搜索
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                if (backgroundWorker.IsBusy)
                {
                    this.backgroundWorker.CancelAsync();
                }
                SearchMusicOnNetwork_Click(sender, new EventArgs());
            }
        }

    }
}
