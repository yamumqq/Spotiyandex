using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TagLib;

namespace Spotiyandex.Core
{
    class Song
    {
        public string FilePath { get; set; }
        public TagLib.File File { get; set; }
        public string SongName { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public BitmapImage Image { get; set; }
        public BitmapImage IsPlayingImage { get; set; }
        public IPicture[] Cover { get; set; }
        public bool isPlaying { get; set; }

        public Song(string filePath)
        {
            FilePath = filePath;
            File = TagLib.File.Create(FilePath);
            SongName = File.Tag.Title ?? Path.GetFileNameWithoutExtension(FilePath);
            Artist = File.Tag.FirstPerformer ?? "Unkown Artist";
            Album = File.Tag.Album;
            isPlaying = false;
            Cover = File.Tag.Pictures;
        }
    }
}