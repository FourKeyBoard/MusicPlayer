using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shell32;

namespace WPF_Player
{
    public struct Mp3Info
    {
        public string Title;//歌曲名,30个字节     
        public string Artist;//歌手名,30个字节     
        public string Album;//所属唱片,30个字节      
        public string Comment;//注释,28个字节      
    }

    public class MP3
    {
        //检查字符串是否有非法字符
        public static readonly string LEGAL_CHARACTERS = "abcdefghijklmnopqrstuvwxyz0123456789.@_-";

        public static bool isLegalCharacters2(string sExt)
        {
            sExt = sExt.ToLower();
            for (int i = 0; i < sExt.Length; i++)
            {
                if (!LEGAL_CHARACTERS.Contains(sExt.Substring(i, 1)))
                {
                    return false;
                }
            }
            return true;
        }
        //检查字符串是否为全中文
        public bool IsChinese(string str_chinese)
        {
            bool b = false;
            for (int i = 0; i < str_chinese.Length; i++)
            {
                Regex reg = new Regex(@"[\u4e00-\u9fa5]");
                if (!reg.IsMatch(str_chinese[i].ToString()))
                {
                    b = true;
                    break;
                }
            }
            return b;
        }

        public Mp3Info getMP3Info(string path)
        {
            Mp3Info mp3Info = new Mp3Info();


            Shell32.Shell sh = new Shell();

            Folder dir = sh.NameSpace(System.IO.Path.GetDirectoryName(path));

            FolderItem item = dir.ParseName(System.IO.Path.GetFileName(path));

            mp3Info.Artist = dir.GetDetailsOf(item, 20);
            mp3Info.Album = dir.GetDetailsOf(item, 14);
            mp3Info.Title = dir.GetDetailsOf(item, 21);
            mp3Info.Comment = dir.GetDetailsOf(item, 24);

            return mp3Info;
            //string[] Info = new string[7];
            //Info[0] = "歌曲名：" + dir.GetDetailsOf(item, 21);   // MP3 歌曲名
            //Info[1] = "艺术家：" + dir.GetDetailsOf(item, 20);  //Authors
            //Info[2] = "专  辑：" + dir.GetDetailsOf(item, 14);  // MP3 专辑
            //Info[3] = dir.GetDetailsOf(item, 27);  // 获取歌曲时长
            //Info[3] = "时  长：" + Info[3].Substring(Info[3].IndexOf(":") + 1);
            //Info[4] = "类  型：" + dir.GetDetailsOf(item, 9);
            //Info[5] = "比特率：" + dir.GetDetailsOf(item, 22);
            //Info[6] = "备  注：" + dir.GetDetailsOf(item, 24);
        }
    }
}
