using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WPF_Player;
namespace WPF_Player.Services
{
    class EntityServiceImpl :EntityInterface
    {
        List<Lynic> EntityInterface.GetLynics(string path) {


            try
            {
                Regex r = new Regex("[01][0-9]:[0-9][0-9].[0-9][0-9]");
                if (string.IsNullOrEmpty(path))
                    return null;
                List<Lynic> result = new List<Lynic>();
                string[] contents = File.ReadAllLines(path, Encoding.Default);
                contents = contents.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                foreach (string i in contents)
                {
                    string[] timeStr = i.Split('[', ']');
                    Match m = r.Match(timeStr[1]);
                    if (m != null && m.Success)
                    {
                        string[] temp = m.Value.Split(':', '.');
                        Lynic lynic = new Lynic();
                        lynic.MiniSencond = (int.Parse(temp[0]) * 60 + int.Parse(temp[1]) + int.Parse(temp[2]) / 1000) * 1000;
                        lynic.Content = timeStr[2];
                        if (string.IsNullOrEmpty(timeStr[2]))
                            continue;//过滤空的歌词
                        result.Add(lynic);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
                return null;
            }
        }


        String[] EntityInterface.GetMusicFiles(string folderName) {
            if (!Directory.Exists(folderName))
                throw new Exception("文件夹:" + folderName + "不存在！");
           



            string[] fileNameX = Directory.GetFiles(folderName, "*.*");
            List<string> lists = new List<string>();
            for (int i = 0; i < fileNameX.Length; i++)
            {
                string fileNamei = fileNameX[i].ToLower();
                if (fileNamei.EndsWith(".mp3") || fileNamei.EndsWith(".flac") || fileNamei.EndsWith(".wav") || fileNamei.EndsWith(".wma"))
                {
                    lists.Add(fileNamei);
                }
            }
            Array array = Array.CreateInstance(typeof(string), lists.Count);
            lists.CopyTo((string[])array, 0);
            return (string[])array;
        }


    }
}
