using System.Windows;

namespace ClipFFmpeg
{
    public partial class MainWindow
    {
        #region DependencyProps
        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));


        public string Timecode
        {
            get { return (string)GetValue(TimecodeProperty); }
            set { SetValue(TimecodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Timecode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimecodeProperty =
            DependencyProperty.Register("Timecode", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));




        public string PlayPause
        {
            get { return (string)GetValue(PlayPauseProperty); }
            set { SetValue(PlayPauseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlayPause.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlayPauseProperty =
            DependencyProperty.Register("PlayPause", typeof(string), typeof(MainWindow), new PropertyMetadata(PAUSE));



        public string StartTimeLabel
        {
            get { return (string)GetValue(StartTimeLabelProperty); }
            set { SetValue(StartTimeLabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTimeLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartTimeLabelProperty =
            DependencyProperty.Register("StartTimeLabel", typeof(string), typeof(MainWindow), new PropertyMetadata(ZERO_TIME));



        public string StopTimeLabel
        {
            get { return (string)GetValue(StopTimeLabelProperty); }
            set { SetValue(StopTimeLabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StopTimeLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StopTimeLabelProperty =
            DependencyProperty.Register("StopTimeLabel", typeof(string), typeof(MainWindow), new PropertyMetadata(ZERO_TIME));



        public string ClipDurationLabel
        {
            get { return (string)GetValue(ClipDurationLabelProperty); }
            set { SetValue(ClipDurationLabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClipDurationLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClipDurationLabelProperty =
            DependencyProperty.Register("ClipDurationLabel", typeof(string), typeof(MainWindow), new PropertyMetadata(ZERO_TIME));



        public string OutputName
        {
            get { return (string)GetValue(OutputNameProperty); }
            set { SetValue(OutputNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OutputName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OutputNameProperty =
            DependencyProperty.Register("OutputName", typeof(string), typeof(MainWindow), new PropertyMetadata("New clip"));



        public string OutputDirectory
        {
            get { return (string)GetValue(OutputDirectoryProperty); }
            set { SetValue(OutputDirectoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OutputDirectory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OutputDirectoryProperty =
            DependencyProperty.Register("OutputDirectory", typeof(string), typeof(MainWindow), new PropertyMetadata(""));


        public bool VideoLoaded
        {
            get { return (bool)GetValue(VideoLoadedProperty); }
            set { SetValue(VideoLoadedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VideoLoaded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoLoadedProperty =
            DependencyProperty.Register("VideoLoaded", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));



        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(MainWindow), new PropertyMetadata(""));



        public int Progress
        {
            get { return (int)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Progress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(int), typeof(MainWindow), new PropertyMetadata(0));



        public bool ClipEnabled
        {
            get { return (bool)GetValue(ClipEnabledProperty); }
            set { SetValue(ClipEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClipEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClipEnabledProperty =
            DependencyProperty.Register("ClipEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public Visibility DebugVisibility
        {
            get { return (Visibility)GetValue(DebugVisibilityProperty); }
            set { SetValue(DebugVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DebugVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DebugVisibilityProperty =
            DependencyProperty.Register("DebugVisibility", typeof(Visibility), typeof(MainWindow), new PropertyMetadata(Visibility.Hidden));

        #endregion

    }
}
