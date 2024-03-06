using Spotiyandex.Core;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Spotiyandex
{
    public partial class MainWindow : Window
    {
        BindingList<Song> playlist = new BindingList<Song>();
        BindingList<Song> OriginalPlaylist = new BindingList<Song>();
        bool isLoopOn = false;
        string SongPath;

        public MainWindow()
        {
            InitializeComponent();
            VolumeSlider.Maximum = 1;
            VolumeSlider.Minimum = 0;
            VolumeSlider.Value = 1;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (Player.NaturalDuration.HasTimeSpan)
            {
                MediaSlider.Minimum = 0;
                MediaSlider.Maximum = Player.NaturalDuration.TimeSpan.Ticks;
                MediaSlider.Value = Player.Position.Ticks;

                if (Player.Position.Seconds < 10)
                {
                    StartTime.Text = Player.Position.Minutes.ToString() + ":" + "0" + Player.Position.Seconds.ToString();
                }
                else
                {
                    StartTime.Text = Player.Position.Minutes.ToString() + ":" + Player.Position.Seconds.ToString();
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void MinBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void FullScreenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
        }

        private void ShuffleBtn_Click(object sender, RoutedEventArgs e)
        {
            BindingList<Song> ShufflePlaylist = playlist;
            if (ShuffleBtnIcon.Foreground == Brushes.Gray)
            {
                ShuffleBtnIcon.Foreground = Brushes.Yellow;

                Random random = new Random();

                for (int i = ShufflePlaylist.Count - 1; i >= 1; i--)
                {
                    int j = random.Next(i + 1);
                    var temp = ShufflePlaylist[j];
                    ShufflePlaylist[j] = ShufflePlaylist[i];
                    ShufflePlaylist[i] = temp;
                }
                playlist = new BindingList<Song>();
                foreach (Song song in ShufflePlaylist)
                    playlist.Add(song);
                SongList.ItemsSource = playlist;
            }
            else
            {
                ShuffleBtnIcon.Foreground = Brushes.Gray;
                playlist = new BindingList<Song>();
                foreach (Song song in OriginalPlaylist)
                    playlist.Add(song);
                SongList.ItemsSource = playlist;
            }
        }

        private void LoopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LoopBtnIcon.Foreground == Brushes.Gray)
            {
                LoopBtnIcon.Foreground = Brushes.Yellow;
                isLoopOn = true;
            }
            else
            {
                LoopBtnIcon.Foreground = Brushes.Gray;
                isLoopOn = false;
            }
        }



        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PlayBtnIcon.Kind == MaterialDesignThemes.Wpf.PackIconKind.Pause)
            {
                PlayBtnIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                Player.Pause();
            }
            else
            {
                PlayBtnIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                Player.Play();
            }
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SongList.SelectedIndex < playlist.Count)
            {
                SongList.SelectedIndex++;
            }
            else
            {
                SongList.SelectedIndex = 0;
            }
        }

        private void PreviousBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SongList.SelectedIndex > 0)
            {
                SongList.SelectedIndex--;
            }
            else
            {
                SongList.SelectedIndex = playlist.Count;
            }
        }

        private void SongList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Song song in playlist)
                song.isPlaying = false;
            try
            {
                playlist[SongList.SelectedIndex].isPlaying = true;
                PlayBtnIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                SongPath = playlist[SongList.SelectedIndex].FilePath;
                Player.Source = new Uri(SongPath);
                MediaSlider.Value = 0;
                PlayBtn_Click(sender, e);
            }
            catch { }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            OriginalPlaylist = new BindingList<Song>();
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            dialog.Title = "Выберите папку с музыкой";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var fileExtensions = new List<string> { ".mp3", ".wav" };

                foreach (string path in Directory.GetFiles(dialog.FileName))
                {
                    OriginalPlaylist.Add(new Song(path));
                }

                foreach (Song song in OriginalPlaylist)
                {
                    if (song.Cover.Length > 0)
                    {
                        var picture = song.Cover[0];
                        using (var stream = new MemoryStream(picture.Data.Data))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            song.Image = bitmap;
                        }
                    }
                    else
                    {
                        BitmapImage bitmap1 = new BitmapImage();
                        bitmap1.BeginInit();
                        bitmap1.UriSource = new Uri(@"/Assets/musicIcon.png", UriKind.RelativeOrAbsolute);
                        bitmap1.EndInit();
                        song.Image = bitmap1;
                    }
                }

                playlist = new BindingList<Song>();
                foreach (Song song in OriginalPlaylist)
                    playlist.Add(song);
                SongList.ItemsSource = playlist;

            }
        }


        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.Volume = (double)VolumeSlider.Value;
        }

        private void MediaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.Position = new TimeSpan(Convert.ToInt64(MediaSlider.Value));
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (Player.NaturalDuration.HasTimeSpan) MediaSlider.Maximum = Player.NaturalDuration.TimeSpan.Ticks;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            if (Player.NaturalDuration.TimeSpan.Seconds < 10)
            {
                EndTime.Text = Player.NaturalDuration.TimeSpan.Minutes.ToString() + ":" + "0" + Player.NaturalDuration.TimeSpan.Seconds.ToString();
            }
            else
            {
                EndTime.Text = Player.NaturalDuration.TimeSpan.Minutes.ToString() + ":" + Player.NaturalDuration.TimeSpan.Seconds.ToString();
            }

            if (Player.Position.Seconds < 10)
            {
                StartTime.Text = Player.Position.Minutes.ToString() + ":" + "0" + Player.Position.Seconds.ToString();
            }
            else
            {
                StartTime.Text = Player.Position.Minutes.ToString() + ":" + Player.Position.Seconds.ToString();
            }

            MediaSlider.Value = Player.Position.Ticks;
            AlbumCover.Source = playlist[SongList.SelectedIndex].Image;
            SongName.Text = playlist[SongList.SelectedIndex].SongName;
            Artist.Text = playlist[SongList.SelectedIndex].Artist;
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            NextBtn_Click(sender, e);
            if (isLoopOn) PreviousBtn_Click(sender, e);
        }
    }
}
