using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;
//using System.Reflection;
//using System.Threading.Tasks;
//using Reminder.Toast; //https://github.com/emoacht/DesktopToast
using System.IO;
using System.Windows.Threading;
using System.Collections.Generic;
using Ookii.Dialogs.Wpf;
using System.Reflection;

namespace Reminder
{

    public partial class MainWindow : MetroWindow
    {
        #region Fields
        private Int64 _IDuser;
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private ViewModel viewModel;
        private DispatcherTimer AlarmCheckTimer = new DispatcherTimer();
        private DispatcherTimer UpdateDBTimer = new DispatcherTimer();
        private NotifiedToday checkIfNotified = new NotifiedToday();
        private NotifyIcon notifyIcon;
        #endregion


        public MainWindow()
        {
            InitializeComponent();

//#if DEBUG
            this.Topmost = false;
//#else
//            this.Topmost = true;
//            this.WindowState = WindowState.Minimized;
//#endif
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            version.Content = getRunningVersion();

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            cboRepeat.ItemsSource= ListBoxEntry.GetDelay();
            cboRepeat.SelectedIndex = 0;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            viewModel.SaveReports();

            e.Cancel = true;
            this.WindowState = WindowState.Minimized;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }
        
        //private void log(string msg)
        //{
        //    System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //    sb.Append(msg);
        //    var datafile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"reminderlog.txt");
        //    File.AppendAllText(datafile, sb.ToString() + Environment.NewLine);
        //    sb.Clear();
        //}

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run background tasks here
            viewModel = new ViewModel();
        }

        private void Worker_RunWorkerCompleted(object sender,RunWorkerCompletedEventArgs e)
        {
            //update UI once worker complete his work
            this.DataContext = viewModel;


            AlarmCheckTimer.Tick += DispatcherTimer_Tick;
#if DEBUG
            AlarmCheckTimer.Interval = new TimeSpan(0,0, 45);
#else
            AlarmCheckTimer.Interval = new TimeSpan(0,1, 0);
#endif
            AlarmCheckTimer.Start();

            // SQLITE n'ayant pas de système de notifications,
            // Le logiciel va vérifier régulièrement
            UpdateDBTimer.Tick += UpdateDBTimer_Tick;
#if DEBUG
            UpdateDBTimer.Interval = new TimeSpan(0,0, 10);
#else
            UpdateDBTimer.Interval = new TimeSpan(0,1,0);
#endif
            UpdateDBTimer.Start();
        }


        private void UpdateDBTimer_Tick(object sender, EventArgs e)
        {
            UpdateDBTimer.Stop();
            if (viewModel.IsDBUpdated())
            {
                viewModel.LoadTaches();
            }
            UpdateDBTimer.Start();
        }


        private  void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            AlarmCheckTimer.Stop();

            List<Tache> alarms = viewModel.GetAlarm(_IDuser);

            if (alarms.Count > 5)
                Notification("SodeaSoft Reminder", Properties.Resources.ManyAlarms);
            else
            {
                foreach (Tache t in alarms)
                {
                    //tag cette alarm pour plus l'avoir en notif
                    if (!checkIfNotified.IsNotified(t))
                    {
                        Notification(t.Caption, t.datStart.ToLongDateString());
                    }

                }
            }

            //if (alarms.Count>5)
            //{
            //    ToastResult = await ShowToastAsync("SodeaSoft Reminder", Properties.Resources.ManyAlarms);
            //}
            //else
            //{
            //    foreach (Tache t in alarms)
            //    {
            //        //tag cette alarm pour plus l'avoir en notif
            //        if (!checkIfNotified.IsNotified(t))
            //        {
            //            ToastResult = await ShowToastAsync(t.Caption, t.datStart.ToLongDateString());
            //        }

            //    }
            //}

            AlarmCheckTimer.Start();
        }

        private void systray_MouseClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void Notification(string Title, string Body)
        {
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Assets/ssrm_256.ico")).Stream;

            notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon(iconStream),
                Text = string.Format("Reminder", "me"),
                Visible = true
            };
            
            notifyIcon.BalloonTipClicked += BalloonClicked;
            notifyIcon.Click += BalloonClicked;
            notifyIcon.BalloonTipClosed += NotifyIcon_BalloonTipClosed;
            notifyIcon.ShowBalloonTip(5000, Title, Body, ToolTipIcon.None);
            
        }


        private void NotifyIcon_BalloonTipClosed(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }

        private void BalloonClicked(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            this.Show();
            this.WindowState = WindowState.Normal;
        }


        private void ButtonRepeat_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnFolder_Click(object sender, RoutedEventArgs e)
        {
            var folder = new VistaFolderBrowserDialog();
            folder.Description = Properties.Resources.FolderDialogTitle;
            folder.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
            //if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
            //    MessageBox.Show(this, "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "Sample folder browser dialog");

            if ((bool)folder.ShowDialog(this))
            {
                var dbPath = Path.Combine(folder.SelectedPath, "planningpro.db");
                if (File.Exists(dbPath))
                {
                    ViewModel.PlanningPath = folder.SelectedPath;
                    Properties.Settings.Default.Path = folder.SelectedPath;
                }
                else
                {
                    System.Windows.MessageBox.Show(Properties.Resources.FolderEmpty,"",MessageBoxButton.OK,MessageBoxImage.Exclamation);
                    Properties.Settings.Default.Path = string.Empty;
                }
                Properties.Settings.Default.Save();
            }
        }

        private void CboUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboUser.SelectedItem != null)
            {
                User _user = cboUser.SelectedItem as User;
                _IDuser = _user.ID;

                Properties.Settings.Default.IDuser = (int)_IDuser;
                Properties.Settings.Default.Save();

                viewModel.LoadTaches();
            }
        }


        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            deleteButton.IsEnabled = true;
        }



#region Notification
        //27/03/2018 Afin d'être compatible Windows 7, je retire cette partie

        //public string ToastResult
        //{
        //    get { return (string)GetValue(ToastResultProperty); }
        //    set { SetValue(ToastResultProperty, value); }
        //}
        //public static readonly DependencyProperty ToastResultProperty =
        //    DependencyProperty.Register(
        //        nameof(ToastResult),
        //        typeof(string),
        //        typeof(MainWindow),
        //        new PropertyMetadata(string.Empty));

        //private async Task<string> ShowToastAsync(string Title,string Body)
        //{
        //    var request = new ToastRequest
        //    {
        //        ToastTitle = Title,
        //        ToastBody = Body,
        //        ToastLogoFilePath = string.Format("file:///{0}", Path.GetFullPath("Assets/Bell.png")),
        //        ShortcutFileName = "SodeaSoft Reminder.lnk",
        //        ShortcutTargetFilePath = Assembly.GetExecutingAssembly().Location,
        //        AppId = "SodeaSoft Reminder",
        //        ActivatorId = typeof(NotificationActivator).GUID // For Action Center of Windows 10
        //    };

        //    var result = await ToastManager.ShowAsync(request);

        //    return result.ToString();
        //}


        //protected override void OnSourceInitialized(EventArgs e)
        //{
        //    base.OnSourceInitialized(e);

        //    // For Action Center of Windows 10
        //    NotificationActivator.RegisterComType(typeof(NotificationActivator), OnActivated);

        //    NotificationHelper.RegisterComServer(typeof(NotificationActivator), Assembly.GetExecutingAssembly().Location);
        //    //NotificationHelper.UnregisterComServer(typeof(NotificationActivator));
        //}

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    base.OnClosing(e);

        //    // For Action Center of Windows 10
        //    NotificationActivator.UnregisterComType();
        //}
        //private const string MessageId = "Message";

        //private void OnActivated(string arguments, Dictionary<string, string> data)
        //{
        //    var result = "Activated";
        //    if ((arguments?.StartsWith("action=")).GetValueOrDefault())
        //    {
        //        result = arguments.Substring("action=".Length);

        //        if ((data?.ContainsKey(MessageId)).GetValueOrDefault())
        //            Dispatcher.Invoke(() => Message = data[MessageId]);
        //    }
        //    Dispatcher.Invoke(() => ActivationResult = result);
        //}

        //public string ActivationResult
        //{
        //    get { return (string)GetValue(ActivationResultProperty); }
        //    set { SetValue(ActivationResultProperty, value); }
        //}
        //public static readonly DependencyProperty ActivationResultProperty =
        //    DependencyProperty.Register(
        //        nameof(ActivationResult),
        //        typeof(string),
        //        typeof(MainWindow),
        //        new PropertyMetadata(string.Empty));
        
        //public string Message
        //{
        //    get { return (string)GetValue(MessageProperty); }
        //    set { SetValue(MessageProperty, value); }
        //}
        //public static readonly DependencyProperty MessageProperty =
        //    DependencyProperty.Register(
        //        nameof(Message),
        //        typeof(string),
        //        typeof(MainWindow),
        //        new PropertyMetadata(string.Empty));
#endregion


#region ContextMenu
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MenuItemShow_Click(object sender, RoutedEventArgs e)
        {
            this.Activate();
        }
#endregion

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private Version getRunningVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
    }




    public class BackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte[] bytes = BitConverter.GetBytes((Int64)value);
            return new SolidColorBrush( Color.FromRgb( bytes[0], bytes[1], bytes[2]));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}


