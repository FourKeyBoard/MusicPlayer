using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_Player;
namespace WPF_Player.Services
{
    interface EntityInterface
    {
        List<Lynic> GetLynics(string path);
        String[] GetMusicFiles(string folderName);
    }
}
