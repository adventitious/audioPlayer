﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace mp3player
{
    public class Track
    {
        public String Path;
        public String Filename;
        String Title;
        String Artist;
        public double LengthSeconds;
        public MediaPlayer mediaPlayer2 = new MediaPlayer();

        public Track( string s )
        {
            Path = s;
            Filename = System.IO.Path.GetFileName(Path);

            try
            {
                mediaPlayer2.Open(new System.Uri(Path));
                Thread.Sleep(20);

                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(100);
                    if (mediaPlayer2.NaturalDuration.HasTimeSpan)
                    {
                        double t1 = mediaPlayer2.NaturalDuration.TimeSpan.TotalSeconds;
                        LengthSeconds = t1;
                        i = 10;
                    }
                }
            }
            catch( Exception ee )
            {
                MessageBox.Show( ee.Message );
            }
        }


        public Track(string path, int seconds, string filename )
        {
            Path = path;
            Filename = filename;
            LengthSeconds = seconds;
        }

        public override string ToString()
        {
            return Math.Floor( LengthSeconds ) + "\t" + Filename; 
        }


    }
    public partial class Playlist : Window
    {
        MainWindow MainWindow;
        public List<Track> Tracks = new List<Track>();
        Track NowPlaying;
        public bool PlaylistChanged = false;
        private const string PlaylistName = "playlist.m3u";

        public Playlist(MainWindow mainWindow)
        {
            InitializeComponent();
            MainWindow = mainWindow;
            MakeTracksFromM3U();

            Lsb_Files.ItemsSource = Tracks;
            Lsb_Files.Items.Refresh();

            // MessageBox.Show("new pls");
        }

        public void WriteCloseM3U()
        {
            //MessageBox.Show("WriteCloseM3U");
            if (PlaylistChanged)
            {
                string s = "#EXTM3U";
                s += "\r\n";

                foreach (Track t in Tracks)
                {
                    s += "#EXTINF:";
                    s += (int)t.LengthSeconds;
                    s += ",";
                    s += t.Filename;
                    s += "\r\n";
                    s += "file" + ":///";
                    s += t.Path;
                    s += "\r\n";
                }

                // string s = "Hello and Welcome3" + Environment.NewLine;
                File.WriteAllText(PlaylistName, s);
            }
        }



        public void MakeTracksFromM3U()
        {
            string m3u = File.ReadAllText(PlaylistName);

            string[] lines = m3u.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            //MessageBox.Show(s111[0]);

            string s = "#EXTM3U";

            if (lines.Length > 1 && lines[0] != s)
            {
                // MessageBox.Show("!!!" + s111[0] + "!!!");
                return;
            }

            // MessageBox.Show(s111[1]);


            /*

            #EXTINF:224,Linton Kwesi Johnson - Street 66
            file:///C:/Users/uservt/Downloads/1980%20-%20Bass%20Culture/02%20-%20Street%2066.mp3

            */

            for (int i = 1; i + 1 < lines.Length;)
            {
                //MessageBox.Show( "start for " + i );
                string lineA = lines[i];
                string[] lineAByColon = lineA.Split(':');
                //MessageBox.Show("a " + a2[0]);
                if (lineAByColon[0] == "#EXTINF")
                {
                    //MessageBox.Show("a ok" + a2[0]);

                    string[] lineARightByComma = lineAByColon[1].Split(',');
                    try
                    {
                        int seconds = int.Parse(lineARightByComma[0]);
                        string filename = lineARightByComma[1];
                        //MessageBox.Show(seconds + "___" + filename);
                        string path = lines[i + 1];
                        path = path.Substring( 8, path.Length - 8);
                        Track t = new Track( path, seconds, filename );
                        Tracks.Add(t);
                        i++;
                        i++;
                        //MessageBox.Show("end for " + i);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message + "exc " + lineARightByComma[0] + i);
                        i++;
                    }
                }
                else
                {
                    //MessageBox.Show( lineAByColon[0] + "   not # " + i + " " + lineAByColon[0].Length);

                    if (lineAByColon[0] == "#EXTINF")
                    {
                        MessageBox.Show("!!!" + lineAByColon[0] + "!!!");
                    }
                    i++;
                }
            }
        }



        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach( string s in files )
            {
                Track t = new Track(s);
                Tracks.Add(t);
            }

            Lsb_Files.ItemsSource = Tracks;
            Lsb_Files.Items.Refresh();

            PlaylistChanged = true;
        }

        private void Lsb_Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Lsb_Files_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Track t = (Track)Lsb_Files.SelectedItem;
            MainWindow.Txb_File.Text = t.Path;
            MainWindow.Open(t.Path);
            MainWindow.Play();
            NowPlaying = t;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //MessageBox.Show("new Window_Closing");


            e.Cancel = true;
            Hide();
        }

        public void NextTrack()
        {
            int nowPlaying = Tracks.IndexOf( NowPlaying );
            if( nowPlaying < Tracks.Count )
            {
                Track t = Tracks.ElementAt( nowPlaying + 1 ) ;
                MainWindow.Txb_File.Text = t.Path;
                MainWindow.Open(t.Path);
                MainWindow.Play();
                NowPlaying = t;
            }

        }
    }
}
