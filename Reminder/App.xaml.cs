using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Reminder
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        //Une seule instance de l'app :
        private static Mutex _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "Reminder";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application  
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }



    }
}
