using System;
using System.Collections.Generic;
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
        String Filename;
        String Title;
        String Artist;
        double LengthSeconds;
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

        public Playlist(MainWindow mainWindow)
        {
            InitializeComponent();
            MainWindow = mainWindow;
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
