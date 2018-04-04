using JetBrains.Annotations;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;

namespace Reminder
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region Fields
        public ObservableCollection<Tache> TacheCollection { get; set; } = new ObservableCollection<Tache>();
        public ObservableCollection<User> UserCollection { get; } = new ObservableCollection<User>();
        public ObservableCollection<Report> ReportCollection { get; set; } = new ObservableCollection<Report>();
        public static string PlanningPath { get; set; }
        public bool RunProgressRing { get; set; }

        private User selectedUserWhoRemind;
        #endregion


        public ViewModel()
        {
            //affiche dans le XAML le ProgressRing
            RunProgressRing = true;


            PlanningPath = Properties.Settings.Default.Path;

            //tâches supprimées localement par l'utilisateur
            LoadReports();

            //Va lire la BD et retire les taches dont l'utilisateur a reporter l'alarme
            LoadTaches();

            //--Liste des utilisateurs
            var ListUsers = Requetes.GetListUser();
            UserCollection = new ObservableCollection<User>(ListUsers);

            //Ajoute "Tous les users" à la liste
            var _item = new User {
                Nom = Properties.Resources.Everybody
            };
            UserCollection.Add(_item);

            var IDuser = Properties.Settings.Default.IDuser;
            selectedUserWhoRemind = UserCollection.SingleOrDefault(i => i.ID == IDuser);

            

            //Hide the progressRing
            RunProgressRing = false;


        }


        public Boolean IsDBUpdated()
        {
            return Requetes.IsDBUpdated();
        }


        #region CommandSupprimer
        Tache _selectedTask;
        public Tache SelectedTask
        {
            get { return _selectedTask; }
            set
            {
                _selectedTask = value;
                OnPropertyChanged("SelectedTask");
            }
        }

        public ICommand DeleteCommand => new SimpleCommand(DeleteTache);
        public ICommand ReportCommand => new ArgumentCommand(ReportAlarm);

        private void DeleteTache()
        {
            if (SelectedTask != null)
            {
                if (ReportCollection == null) ReportCollection = new ObservableCollection<Report>();
                //si la task n'existe pas déjà, on l'ajoute
                if (!ReportCollection.Any(i => i.TaskID== SelectedTask.ID))
                {
                    var _report = new Report
                    {
                        TaskID = SelectedTask.ID,
                        AlarmDiseable = true
                    };
                    ReportCollection.Add(_report);
                }
                TacheCollection.Remove(SelectedTask);
            }
        }

        private void ReportAlarm(object CommandParameter)
        {
            if (SelectedTask != null)
            {
                ListBoxEntry delay = CommandParameter as ListBoxEntry;

                //si la task n'existe pas déjà, on l'ajoute
                if (ReportCollection == null) ReportCollection = new ObservableCollection<Report>();
                if (!ReportCollection.Any(i => i.TaskID == SelectedTask.ID))
                {
                    var _report = new Report
                    {
                        TaskID = SelectedTask.ID,
                        DateModified = DateTime.Now,
                        AlarmDiseable = false
                    };
                    ReportCollection.Add(_report);
                }

                var item = ReportCollection.FirstOrDefault(i => i.TaskID == SelectedTask.ID);
                if (item != null)
                {
                    item.ExpirationDate = SelectedTask.datStart.AddMinutes(-SelectedTask.MinuteRemind + delay.MinutesData);
                }
            }
        }


        public class ArgumentCommand : ICommand
        {
            Action<object> _execute;

            public ArgumentCommand(Action<object> execute)
            {
                this._execute = execute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                _execute(parameter);
            }
        }


        public class SimpleCommand : ICommand
        {
            Action _execute;

            public SimpleCommand(Action execute)
            {
                this._execute = execute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                _execute();
            }
        }
        #endregion


       

        public void LoadTaches()
        {
#if DEBUG
            //data fake
            //ReportCollection.Add(new Report { TaskID = 1 });
            //ReportCollection.Add( new Report { TaskID = 24 });
#endif
            //L'utilisateur s'est-il identifier ?
            var IDuser = Properties.Settings.Default.IDuser;
            
            var ListTaches = Requetes.GetListTaches(IDuser);

            
            //retire de la liste des tâches 
            if (ReportCollection != null && ListTaches != null)
            {
                var result = ListTaches.ToList(); 
                result.RemoveAll(x => ReportCollection.Any(d => d.TaskID == x.ID)); 
                
                //var result = ListTaches.Where(x => ReportCollection.Any(d => d.TaskID != x.ID));
                TacheCollection = new ObservableCollection<Tache>(result);
            }
            else
            {
                TacheCollection = new ObservableCollection<Tache>(ListTaches);
            }

            
            OnPropertyChanged("TacheCollection");
        }



        public List<Tache> GetAlarm(long IDUser)
        {
            List<Tache> alarms = new List<Tache>();
            foreach(Tache t in TacheCollection)
            {
                // AND que l'alarme ne date pas de moins de 7 jours
                if  ( DateTime.Now>=t.datStart.AddMinutes(-t.MinuteRemind) & t.datEnd.AddDays(7) >= DateTime.Now) 
                {
                    alarms.Add(t);
                }
            }
            return alarms;
        }
        


        private string PathLocalData()
        {
            var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"SodeaSoft\Reminder");
            Directory.CreateDirectory(dataPath);
            dataPath = Path.Combine(dataPath, "ssrm_report.json");
            return dataPath;
        }

        public void LoadReports()
        {
            if (File.Exists(PathLocalData())) {
                var fs = new FileStream(PathLocalData(), FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

                string content;
                using (StreamReader reader = new StreamReader(fs))
                {
                    content = reader.ReadToEnd();
                }
                ReportCollection = JsonConvert.DeserializeObject<ObservableCollection<Report>>(content);
            }
            else
            {
                ReportCollection = new ObservableCollection<Report>();
            }
            
            OnPropertyChanged("ReportCollection");
        }

        public void SaveReports()
        {
            File.WriteAllText(PathLocalData(), JsonConvert.SerializeObject(ReportCollection));
        }



        public User SelectedUserWhoRemind
        {
            get { return selectedUserWhoRemind; }

            set
            {
                if (selectedUserWhoRemind != value)
                {
                    selectedUserWhoRemind = value;
                    OnPropertyChanged("SelectedUserWhoRemind");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
