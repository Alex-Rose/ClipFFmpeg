using System.Windows;
using System.Windows.Controls;

namespace ClipFFmpeg
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CredentialWindow : Window
    {
        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Username.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(CredentialWindow), new PropertyMetadata(""));

        public bool SaveCreds
        {
            get { return (bool)GetValue(SaveCredsProperty); }
            set { SetValue(SaveCredsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SaveCreds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SaveCredsProperty =
            DependencyProperty.Register("SaveCreds", typeof(bool), typeof(CredentialWindow), new PropertyMetadata(true));



        public CredentialWindow()
        {
            this.DataContext = this;
            InitializeComponent();
            if (CredentialManager.Instance.IsSet)
            {
                Username = CredentialManager.Instance.Username;
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            CredentialManager.Instance.Username = Username;
            CredentialManager.Instance.Password = PasswordBox.Password;
            CredentialManager.Instance.IsSet = true;

            if (SaveCreds)
            {
                CredentialManager.Instance.PersistCredentials();
            }

            Close();
        }
    }
}
