﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace WPF_Player
{
    public class Song:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private string _name;
        private string _location;

        public string PicUrl { get; set; }
        public string Singer { get; set; }
        public string Album { get; set; }
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }
        public string Location
        {
            get { return _location; }
            set { _location = value; OnPropertyChanged("Location"); Name = Path.GetFileName(value); }
        }
        private List<Lynic> lstLynic = new List<Lynic>();
        /// <summary>
        /// 歌词
        /// </summary>
        public List<Lynic> LstLynic
        {
            get { return lstLynic; }
            set { lstLynic = value; }
        }
    }
}
