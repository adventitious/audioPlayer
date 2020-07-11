using System;
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
        public String Title;
        public String Artist;
        public MediaPlayer mediaPlayer2 = new MediaPlayer();
        public double LengthSeconds { get; set; }
        public string ListString
        { get; set; }

        public string LengthMins
        {
            get
            {
                return MainWindow.SecondsToText((int)LengthSeconds) ;
            } 
        }
        public string IsPlaying { get; set; }
        public DateTime Birthday { get; set; }


        public Track()
        {
            IsPlaying = "";
        }

        public Track(string s)
        {
            Path = s;
            //Filename = System.IO.Path.GetFileName(Path);


            var tfile = TagLib.File.Create(s);
            string title = tfile.Tag.Title;
            TimeSpan duration = tfile.Properties.Duration;
            //MessageBox.Show("cccccc" + title);

            LengthSeconds = duration.TotalSeconds;
            Title = tfile.Tag.Title;
            Artist = tfile.Tag.FirstPerformer;
            ListString = Artist + " - " + Title;
            IsPlaying = "";

            // MessageBox.Show("cc " + tfile.Properties.AudioBitrate + " ww " + tfile.Properties.AudioSampleRate + " ww " + tfile.Properties.AudioChannels + " ww " );
        }


        public Track(string path, int seconds, string listString)
        {
            Path = path;
            ListString = listString;
            LengthSeconds = seconds;
            IsPlaying = "";
        }

        public override string ToString()
        {
            string x = MainWindow.SecondsToText((int)Math.Floor(LengthSeconds));
            return x + "\t" + IsPlaying + ListString;
        }
    }

    public partial class Playlist : Window
    {
        MainWindow MainWindow;
        public List<Track> Tracks = new List<Track>();
        public Track NowPlaying;
        public bool PlaylistChanged = false;
        private const string PlaylistName = "playlist.m3u";

        public Playlist(MainWindow mainWindow)
        {
            InitializeComponent();
            MainWindow = mainWindow;
            MakeTracksFromM3U();

            // MessageBox.Show("new pls");

            Track s = Tracks.ElementAt(0);
            
            Dg_Playlist.ItemsSource = Tracks;
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
                    s += t.ListString;
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
                        string listString = lineARightByComma[1];
                        //MessageBox.Show(seconds + "___" + filename);
                        string path = lines[i + 1];
                        path = path.Substring( 8, path.Length - 8);
                        Track t = new Track( path, seconds, listString );
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

            Dg_Playlist.ItemsSource = Tracks;
            Dg_Playlist.Items.Refresh();

            PlaylistChanged = true;
        }

        private void Lsb_Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Lsb_Files_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Track t = (Track)Dg_Playlist.SelectedItem;
                PlayTrack(t);
            }
            catch( Exception ex )
            {
                MessageBox.Show("Lsb_Files_MouseDoubleClick err " + ex.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //MessageBox.Show("new Window_Closing");

            e.Cancel = true;
            Hide();
        }

        /*
      
                                <Setter TargetName="_Border" Property="Background" Value="Yellow"/>
                                <Setter Property="Foreground" Value="Red"/>  
    */
        public void PlayTrack( Track t )
        {
            if (NowPlaying != null)
            {
                NowPlaying.IsPlaying = "";
            }
            MainWindow.Open(t.Path);
            MainWindow.Play();
            MainWindow.Txb_File.Text = t.ToString();
            t.IsPlaying = "|>";
            NowPlaying = t;
            Dg_Playlist.Items.Refresh();
            // Dg_Playlist.Items.Refresh();
        }

        public void BackTrack()
        {
            int nowPlaying = Tracks.IndexOf(NowPlaying);
            if (nowPlaying > 0 )
            {
                Track t = Tracks.ElementAt(nowPlaying - 1);
                PlayTrack(t);
            }
        }
        public void NextTrack()
        {
            int nowPlaying = Tracks.IndexOf(NowPlaying);
            if (nowPlaying < Tracks.Count - 1)
            {
                Track t = Tracks.ElementAt(nowPlaying + 1);
                PlayTrack(t);
            }
        }
        public void TrackOne()
        {
            if ( Tracks.Count > 0 )
            {
                Track t = Tracks.ElementAt( 0 );
                PlayTrack(t);
            }
        }
    }
}
