using System;
using System.Windows;
using System.Windows.Media;

using System.IO;
using System.Timers;
using System.Threading;
using TagLib;
using System.Windows.Input;

namespace mp3player
{
    public partial class MainWindow : Window
    {
        public MediaPlayer mediaPlayer;
        bool Paused = false;
        bool CodeEvent = false;

        bool insideVolumeSlider = false;
        bool insideBalanceSlider = false;
        bool insideSeek = false;

        System.Timers.Timer aTimer;
        Playlist Playlist;
        bool CountUp = false;
        bool Stopped = true;
        string Txb_File_Text = "";
        int PauseBlink = 0;
        int PauseBlinkMax = 12;



        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mediaPlayer = new MediaPlayer();

            // Open the file to read from.
            //string m3u = File.ReadAllText( PlaylistName );

            /*
            using (StreamReader readtext = new StreamReader( PlaylistName ))
            {
                m3u = readtext.ReadLine();
            }
            */

            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 100;

            Playlist = new Playlist(this);
            Playlist.Show();
        }



        private void Mp3_Play_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Playlist.WriteCloseM3U();

            // MessageBox.Show("shutdown now");

            System.Windows.Application.Current.Shutdown();
            /*
            if (Playlist != null)
            {
                Playlist.Close();
            }
            */
        }

        private void Btn_Play_Click(object sender, RoutedEventArgs e)
        {
            if( Paused )
            {
                UnPause();
                return;
            }
            Open();
            Play();
        }



        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            aTimer.Enabled = false;

            mediaPlayer.Stop();
            Stopped = true; ;
            UpdateInfo();
        }

        private void Btn_Pause_Click(object sender, RoutedEventArgs e)
        {
            // aTimer.Enabled = false;

            if( Paused )
            {
                UnPause();
            }
            else
            {
                mediaPlayer.Pause();
                // aTimer.Enabled = false;
                Paused = true;
            }
            UpdateInfo();
        }

        private void Btn_Playlist_Click(object sender, RoutedEventArgs e)
        {
            if( Playlist != null )
            {
                if (Playlist.IsVisible)
                {
                    Playlist.Hide();
                }
                else
                {
                    Playlist.Show();
                }
            }
            else
            {
                Playlist.Show();
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() => {
                    // Code causing the exception or requires UI thread access
                    // https://stackoverflow.com/questions/21306810/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it
                    UpdateInfo();
                });
            }
            catch (Exception)
            {
                // MessageBox.Show(eee.Message);
            }
        }


        public void UpdateInfo()
        {
            double Position = mediaPlayer.Position.TotalSeconds;
            Duration t2 = mediaPlayer.NaturalDuration;

            /*
            if (!mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                Thread.Sleep(100);
            }
            */

            if (!t2.HasTimeSpan)
            {
                TxBk_Info.Text = ("0:00");
                return;
            }

            if ( Paused )
            {
                string countString = "";
                if ( ( PauseBlink * 2 ) < PauseBlinkMax )
                {
                    countString = " :  ";
                }
                else
                {
                    if (CountUp == true)
                    {
                        countString = (SecondsToText((int)Math.Floor(Position)));
                    }
                    else
                    {
                        countString = (SecondsToText((int)Math.Floor( t2.TimeSpan.TotalSeconds - Position)));
                    }
                }

                TxBk_Info.Text = countString;

                PauseBlink += 1;
                PauseBlink %= PauseBlinkMax;
                
                return;
            }


            try
            {
                double total = t2.TimeSpan.TotalSeconds;
                double remaining = total - Position;
                // Display the time remaining in the current media.

                if (remaining == 0 )
                {
                    // MessageBox.Show("ending");
                    Playlist.NextTrack();
                }

                string countString = "";

                if ( CountUp == true )
                {
                    countString = (SecondsToText((int)Math.Floor(Position)));
                }
                else
                {
                    countString = (SecondsToText((int)Math.Floor(remaining)));
                }

                TxBk_Info.Text = countString; //  (SecondsToText((int)Math.Floor(remaining)));

                Prg_Bar.Value = 100 / (total / Position);
                SetSlider( 100 / (total / Position) / 10 );
            }
            catch (Exception )
            {
                // MessageBox.Show(eee.Message);
            }
        }


        public static string SecondsToText( int seconds )
        {
            int seconds2 = seconds % 60;
            if ( seconds2 < 10 )
            {
                return "" + seconds / 60 + ":0" + seconds2;
            }
            return "" + seconds / 60 + ":" + seconds2;
        }


        private void UnPause()
        {
            mediaPlayer.Play();
            // aTimer.Enabled = true;
            Paused = false;
        }

        public void Open(string file)
        {
            mediaPlayer.Open(new System.Uri(file));

            // mediaPlayer.Volume = 0;
            // Slider_Vol.Value = 0;
        }

        private void Open()
        {
            Track t = Playlist.NowPlaying;
            if (t == null)
            {
                Playlist.TrackOne();
                return;
            }
            Playlist.PlayTrack(t);
        }

        public void Play()
        {
            mediaPlayer.Play();
            aTimer.Enabled = true;
            Stopped = false;

            //mediaPlayer.SpeedRatio = 1.8;

            // MessageBox.Show("cccccc"  );
            // Thread.Sleep(300);

            //UpdateInfo();
        }

        public void Seek( double sld )
        {
            // if stopped Sld_Position.Value = 0, return
            if( Stopped == true )
            {
                SetSlider(0);
                return;
            }

            double total = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            double newPos = 0.1 * sld * total;
            mediaPlayer.Position = TimeSpan.FromSeconds( newPos );

            double Position = mediaPlayer.Position.TotalSeconds;
            Duration t2 = mediaPlayer.NaturalDuration;

            Prg_Bar.Value = 100 / (total / Position);
            SetSlider(100 / (total / Position) / 10);
        }

        private void SetSlider( double newVal )
        {
            CodeEvent = true;
            Sld_Position.Value = newVal;
            CodeEvent = false;
        }

        private void Sld_Position_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if( CodeEvent )
            {
                return;
            }
            //mediaPlayer.Position = TimeSpan.FromSeconds(100);
            // MessageBox.Show("" + Sld_Position.Value);
            Seek(Sld_Position.Value);
        }

        private void Slider_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SliderStereo.Value = 0; 
        }

        private void Mp3_Play_Activated(object sender, EventArgs e)
        {
            //MessageBox.Show("a");
            if( Playlist != null)
            {
                Playlist.Focus();
                // this.Focus();
            }
        }

        private void TxBk_Info_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CountUp = !CountUp;
        }

        private void Btn_Next_Click(object sender, RoutedEventArgs e)
        {
            Playlist.NextTrack();
        }

        private void Btn_Back_Click(object sender, RoutedEventArgs e)
        {
            Playlist.BackTrack();
        }

        private void Slider_Vol_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Txb_File.Text = Txb_File_Text; 
            insideVolumeSlider = false;

        }

        private void SliderStereo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer == null)
            {
                return;
            }
            try
            {
                mediaPlayer.Balance = SliderStereo.Value / 100;
                Txb_File.Text = "balance: " + (mediaPlayer.Balance * 100);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SliderStereo_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Txb_File_Text = Txb_File.Text;
            Txb_File.Text = "balance: " + (int)(mediaPlayer.Balance * 100);
            insideBalanceSlider = true;
        }

        private void SliderStereo_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Txb_File.Text = Txb_File_Text; 
            insideBalanceSlider = false;
        }

        private void Slider_Vol_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mediaPlayer.Volume = 1;
            Slider_Vol.Value = 10;
        }

        private void Slider_Vol_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            /*
            // If the mouse wheel delta is positive, move the box up.
            if (e.Delta > 0)
            {
                Slider_Vol.Value = Slider_Vol.Value - 0.4;
            }

            // If the mouse wheel delta is negative, move the box down.
            if (e.Delta < 0)
            {
                Slider_Vol.Value = Slider_Vol.Value + 0.4;
            }
            */
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Mp3_Play_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }





        private void Slider_Vol_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer == null)
            {
                return;
            }
            try
            {
                mediaPlayer.Volume = Slider_Vol.Value / 10;
                if( insideVolumeSlider )
                {
                    Txb_File.Text = "volume: " + (int)(mediaPlayer.Volume * 100);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Slider_Vol_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Txb_File_Text = Txb_File.Text;
            Txb_File.Text = "volume: " + (int)(mediaPlayer.Volume * 100);
            insideVolumeSlider = true;
        }

        private void Mp3_Play_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int valueOfWheel = 0;

            // If the mouse wheel delta is positive, move the box up.
            if (e.Delta > 0)
            {
                valueOfWheel = 1;
            }

            // If the mouse wheel delta is negative, move the box down.
            if (e.Delta < 0)
            {
                valueOfWheel = -1;
            }

            if (insideVolumeSlider)
            {
                Slider_Vol.Value = Slider_Vol.Value + ( 0.1 * valueOfWheel );
                return;
            }
            if (insideBalanceSlider)
            {
                SliderStereo.Value = SliderStereo.Value + (3 * valueOfWheel);
                return;
            }
            if (insideSeek)
            {
                skipSeek( 5 * valueOfWheel);
                // mediaPlayer.
                // Sld_Position.Value = Sld_Position.Value + 0.1;
                // SliderStereo.Value = SliderStereo.Value + (3 * valueOfWheel);
                return;
            }

            Slider_Vol.Value = Slider_Vol.Value + (0.5 * valueOfWheel);


        }

        private void skipSeek( double seconds )
        {
            double total = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            double pos1 = mediaPlayer.Position.TotalSeconds;
            double newPos = pos1 + seconds;
            mediaPlayer.Position = TimeSpan.FromSeconds(newPos);

            double Position = mediaPlayer.Position.TotalSeconds;
            // Duration t2 = mediaPlayer.NaturalDuration;

            Prg_Bar.Value = 100 / (total / Position);
            SetSlider(100 / (total / Position) / 10);
        }

        private void Prg_Bar_MouseEnter(object sender, MouseEventArgs e)
        {
            insideSeek = true;
        }

        private void Prg_Bar_MouseLeave(object sender, MouseEventArgs e)
        {
            insideSeek = false;
        }

        private void Sld_Position_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void Sld_Position_MouseLeave(object sender, MouseEventArgs e)
        {

        }
    }
}

