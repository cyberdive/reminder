using System;
using System.Data;
using System.Data.SQLite;
using Apps72.Dev.Data.Sqlite;
using System.IO;

namespace Reminder
{
    public class DataService :  IDisposable
    {
        private SQLiteConnection _connection = null;
        private String _DataPath;
        private DateTime _LastWriteTime;

        public DataService()
        {
            var PlanningPath = Properties.Settings.Default.Path;
            if (!String.IsNullOrEmpty(PlanningPath))
            {
                _DataPath = Path.Combine(PlanningPath, "planningpro.db");
            }
        }

        public bool IsDirectoryWritable(string dirPath)
        {
            try
            {
                using (FileStream fs = File.Create(
                    Path.Combine(
                        dirPath,
                        Path.GetRandomFileName()
                    ),
                    1,
                    FileOptions.DeleteOnClose)
                )
                { }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Connection()
        {
            if (File.Exists(_DataPath))
            {
                
                var c = new SQLiteConnectionStringBuilder()
                {
                    DataSource = _DataPath
                };

                Uri uriAddress = new Uri(_DataPath);
                if (uriAddress.IsUnc)
                {
                    c.ConnectionString = c.ConnectionString.Replace(@"\\", @"\\\");
                }
                
                _connection = new SQLiteConnection(c.ConnectionString);
                _connection.Open();

            }
        }

        public Boolean IsDBUpdated()
        { 
            if (_DataPath == null) return false;

            if (_LastWriteTime != File.GetLastWriteTime(_DataPath))
            {
                _LastWriteTime = File.GetLastWriteTime(_DataPath);
                return true;
            } else
                return false;
        }

        public SqliteDatabaseCommand GetDatabaseCommand()
        {
            return new SqliteDatabaseCommand(_connection);
        }

        public SqliteDatabaseCommand GetDatabaseCommand(SQLiteTransaction transaction)
        {
            return new SqliteDatabaseCommand(_connection, transaction);
        }


        public ConnectionState GetConnectionState
        {
            get {
                if (_connection == null) //TODO : y'a pas moyen de faire mieux? cas où le chemin ex R:/base.db pas accessible
                {
                    return ConnectionState.Closed;
                }
                else
                {
                    return _connection.State;
                }
            }
            private set { }
            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }

        ~DataService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}   
