using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace WPF_Player
{
    //public partial class App : Application
    //{
    //    private static DispatcherOperationCallback exitFrameCallback = new DispatcherOperationCallback(ExitFrame);
    //    public static void DoEvents()
    //    {
    //        DispatcherFrame nestedFrame = new DispatcherFrame();
    //        DispatcherOperation exitOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, exitFrameCallback, nestedFrame);
    //        Dispatcher.PushFrame(nestedFrame);
    //        if (exitOperation.Status !=
    //        DispatcherOperationStatus.Completed)
    //        {
    //            exitOperation.Abort();
    //        }
    //    }

    //    private static Object ExitFrame(Object state)
    //    {
    //        DispatcherFrame frame = state as
    //        DispatcherFrame;
    //        frame.Continue = false;
    //        return null;
    //    }


    //}
    //歌单的标签和简介
    public class ListText
    {
        public ListText() { }
        public string T1;
        public string T2;
        public ListText(string t1, string t2)
        {
            T1 = t1;
            T2 = t2;
        }
        public static List<ListText> listTexts;
        public static void Export(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ListText>));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                xmlSerializer.Serialize(fs, listTexts);
            }
        }
        public static List<ListText> Import(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ListText>));
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                List<ListText> listTexts01 = (List<ListText>)xmlSerializer.Deserialize(fs);
                foreach (var od in listTexts01)
                {
                    Console.WriteLine(od);
                }
                return listTexts01;
            }
        }
    }
}
