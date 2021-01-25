using RestSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Xabe.FFmpeg;

namespace ClipFFmepg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string PLAY = "Play";
        const string PAUSE = "Pause";
        const string ZERO_TIME = "00:00:00";

        private DispatcherTimer timer;
        private DispatcherTimer sliderTimer;
        private Timer seekTimer;
        private bool playing = true;
        private TimeSpan startTime = new TimeSpan(0);
        private TimeSpan stopTime = new TimeSpan(0);
        private TimeSpan durationTime = new TimeSpan(0);
        private bool isDragging;
        private string ffmpegTempFile;

        private event EventHandler ffmpegInitialized;

        public MainWindow()
        {
            DataContext = this;
            this.Drop += OnDrop;
            InitializeComponent();
            Player.MediaOpened += OnMediaOpened;
            Player.LoadedBehavior = MediaState.Manual;
            Player.UnloadedBehavior = MediaState.Manual;
            VideoLoaded = false;
            //FFmpeg.SetExecutablesPath(@"C:\Users\Alexandre\Downloads\ffmpeg\bin");

            if (string.IsNullOrEmpty(Properties.Settings.Default.OutputDirectory))
            {
                Properties.Settings.Default.OutputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }

            OutputDirectory = Properties.Settings.Default.OutputDirectory;

            ffmpegInitialized += OnFFmpegInitialized;
            _ = Initialize();
        }

        private void OnFFmpegInitialized(object sender, EventArgs e)
        {
            ClipEnabled = true;
            StatusText = "FFmpeg ready";
        }

        private async Task Initialize()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.FFmpegPath))
            {
                if (Directory.Exists(Properties.Settings.Default.FFmpegPath))
                {
                    FFmpeg.SetExecutablesPath(Properties.Settings.Default.FFmpegPath);
                }
                else
                {
                    Properties.Settings.Default.FFmpegPath = "";
                }
            }

            bool ffmpegFound = await TryFFmpegBinaries();
            if (!ffmpegFound)
            {
                Properties.Settings.Default.FFmpegPath = "";
                string path = LocateFFmpeg();
                if (path != null)
                {
                    FFmpeg.SetExecutablesPath(path);
                    ffmpegFound = await TryFFmpegBinaries();
                }

                if (!ffmpegFound)
                {
                    MessageBoxResult result = MessageBox.Show(
                        "FFmpeg cannot be found in your path. Would you like to download it now? the binaries will be downloaded from https://www.gyan.dev/ffmpeg/builds/ and be extracted alongside this application.",
                        "FFmpeg not found",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        DownloadFFmpeg();
                    }
                }
                else
                {
                    Properties.Settings.Default.FFmpegPath = path;
                }
            }

            if (ffmpegFound)
            {
                ffmpegInitialized?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private string LocateFFmpeg(int i = 0, string root = null)
        {
            string currentDir = root != null ? root : Environment.CurrentDirectory;

            string ffmpegPath = null;
            if (i < 2)
            {
                foreach (var dir in Directory.GetDirectories(currentDir))
                {
                    ffmpegPath = LocateFFmpeg(i++, dir);
                    if (ffmpegPath != null)
                    {
                        return ffmpegPath;
                    }
                }
            }

            string lookupFFmpeg = $"{currentDir}\\ffmpeg.exe";
            string lookupFFprobe = $"{currentDir}\\ffprobe.exe";
            if (File.Exists(lookupFFmpeg) && File.Exists(lookupFFprobe))
            {
                ffmpegPath = currentDir;
            }

            return currentDir;
        }

        private async Task<bool> TryFFmpegBinaries()
        {
            try
            {
                IConversionResult result = await FFmpeg.Conversions.New().Start("-version");
                return true;
            }
            catch (Xabe.FFmpeg.Exceptions.FFmpegNotFoundException)
            {
                return false;
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void OnMediaOpened(object sender, RoutedEventArgs e)
        {
            VideoLoaded = true;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                if (File.Exists(files.FirstOrDefault()))
                {
                    InitializeTimerIfNeeded();
                    Source = files.First();
                    Player.Play();
                    e.Handled = true;
                }
            }
        }

        private void InitializeTimerIfNeeded()
        {
            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += OnTick;
                timer.Start();
            }

            if (sliderTimer == null)
            {
                sliderTimer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(33);
                timer.Tick += OnSliderTimerTick;
                timer.Start();
            }
        }

        private void OnSliderTimerTick(object sender, EventArgs e)
        {
            if (!isDragging && Player.Source != null && Player.NaturalDuration.HasTimeSpan)
            {
                double position = (double)Player.Position.Ticks / (double)Player.NaturalDuration.TimeSpan.Ticks;
                Slider.Value = position * Slider.Maximum;
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Player.Source != null)
            {
                if (Player.NaturalDuration.HasTimeSpan)
                {
                    Timecode = String.Format("{0} / {1}", Player.Position.ToString(@"hh\:mm\:ss"), Player.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss"));
                }
            }
            else
            {
                Timecode = "No file selected...";
            }
        }

        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Player.NaturalDuration.HasTimeSpan && isDragging)
            {
                seekTimer?.Dispose();
                seekTimer = null;
                seekTimer = new Timer(obj =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        var duration = Player.NaturalDuration.TimeSpan;
                        var newPosition = new TimeSpan((long)(duration.Ticks * e.NewValue / Slider.Maximum));
                        Player.Position = newPosition;
                    });
                }, null, 250, Timeout.Infinite);
            }
        }

        private void PlayPauseButtonClick(object sender, RoutedEventArgs e)
        {
            if (playing)
            {
                Player.Pause();
                PlayPause = PLAY;
            }
            else
            {
                Player.Play();
                PlayPause = PAUSE;
            }

            playing = !playing;
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            if (Player.IsLoaded)
            {
                startTime = Player.Position;
                StartTimeLabel = startTime.ToString(@"hh\:mm\:ss");
                UpdateDuration();
            }
        }

        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            if (Player.IsLoaded)
            {
                stopTime = Player.Position;
                StopTimeLabel = stopTime.ToString(@"hh\:mm\:ss");
                UpdateDuration();
            }
        }

        private void ClipButtonClick(object sender, RoutedEventArgs e)
        {

            //Player.Stop();
            //Player.Close();
            //string output = Path.ChangeExtension(Path.GetTempFileName(), ".mkv");
            StatusText = "Initializing...";
            Progress = 0;
            string outFile = $"{OutputDirectory}\\{OutputName}{Path.GetExtension(Source)}";
            string src = Source;
            _ = Task.Run(async () =>
            {
                try
                {
                    //IConversion conversion = await FFmpeg.Conversions.FromSnippet.Split(src, outFile, startTime, durationTime);
                    IConversion conversion = FFmpeg.Conversions.New()
                        .AddParameter($"-ss {startTime.ToString("hh\\:mm\\:ss")} -t {durationTime.ToString("hh\\:mm\\:ss")} -i \"{src}\" -acodec copy -vcodec copy \"{outFile}\"");
                    conversion.OnProgress += OnConversionProgress;
                    Dispatcher.Invoke(() => StatusText = "Creating clip");
                    IConversionResult result = await conversion.Start();
                    //var snippet = await FFmpeg.Conversions.FromSnippet.Convert(src, output);
                    //IConversionResult result = await snippet.Start();
                    UploadClip(outFile);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            });
        }

        private void OnConversionProgress(object sender, Xabe.FFmpeg.Events.ConversionProgressEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                Progress = args.Percent / 2;
            });
        }

        private void UpdateDuration()
        {
            if (stopTime > startTime)
            {
                durationTime = stopTime - startTime;
                ClipDurationLabel = durationTime.ToString(@"hh\:mm\:ss");
            }
        }

        private void BrowserButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                string folder = dialog.SelectedPath;
                if (Directory.Exists(folder))
                {
                    OutputDirectory = folder;
                    Properties.Settings.Default.OutputDirectory = folder;
                }
            }
        }

        private void UploadClip(string file)
        {
            Dispatcher.Invoke(() => StatusText = "Preparing to upload...");
            if (!CredentialManager.Instance.IsSet)
            {
                var credWindow = new CredentialWindow();
                credWindow.ShowDialog();
            }

            string username = CredentialManager.Instance.Username;
            string password = CredentialManager.Instance.Password;
            string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));

            string url = "https://api.streamable.com/upload";
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", $"Basic {encoded}");
            //request.AddHeader("Cookie", "session=N46SCHDR14K5LVJ9JEDKSCCL");
            request.AddFile("file", file);
            Dispatcher.Invoke(() => StatusText = "Uploading");
            IRestResponse response = client.Execute(request);
            var streamableResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<StreamableResponse>(response.Content);

            if (!string.IsNullOrEmpty(streamableResponse?.shortcode))
            {
                string link = $"https://streamable.com/{streamableResponse.shortcode}";
                Dispatcher.Invoke(() =>
                {
                    StatusText = $"Done. Link copied to clipboard: {link}";
                    Progress = 100;
                });

                Dispatcher.Invoke(() => { Clipboard.SetText(link); });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    StatusText = "Error";
                    Progress = 100;
                });
            }

            Console.WriteLine(response.Content);
        }

        private void DownloadFFmpeg()
        {
            using (WebClient wc = new WebClient())
            {
                ffmpegTempFile = Path.GetTempFileName();
                wc.DownloadProgressChanged += DownloadProgressChanged;
                wc.DownloadFileCompleted += DownloadCompleted;
                wc.DownloadFileAsync(new System.Uri("https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"), ffmpegTempFile);
            }
        }

        private async void DownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText = "Extracting FFmpeg";
                Progress = 100;
            });

            ZipFile.ExtractToDirectory(ffmpegTempFile, Environment.CurrentDirectory);
            string path = LocateFFmpeg();
            if (path == null)
            {
                throw new Exception("FFmpeg did not download successfully");
            }

            FFmpeg.SetExecutablesPath(path);
            bool ffmpegReady = await TryFFmpegBinaries();
            if (!ffmpegReady)
            {
                throw new Exception("FFmpeg can't initialize");
            }

            Properties.Settings.Default.FFmpegPath = path;
            ffmpegInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText = "Downloading FFmpeg";
                Progress = e.ProgressPercentage;
            });
        }

        private void UpdateCredentialsClick(object sender, RoutedEventArgs e)
        {
            var credWindow = new CredentialWindow();
            credWindow.ShowDialog();
        }

        private void Slider_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDragging = true;
        }

        private void Slider_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Left)
            {
                if (Player.Source != null && Player.NaturalDuration.HasTimeSpan)
                {
                    var newPosition = new TimeSpan(Math.Max(0, Player.Position.Ticks - TimeSpan.FromSeconds(10).Ticks));
                    Player.Position = newPosition;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Right)
            {
                if (Player.Source != null && Player.NaturalDuration.HasTimeSpan)
                {
                    var newPosition = new TimeSpan(Math.Min(Player.NaturalDuration.TimeSpan.Ticks, Player.Position.Ticks + TimeSpan.FromSeconds(10).Ticks));
                    Player.Position = newPosition;
                }
            }
        }
    }
}
