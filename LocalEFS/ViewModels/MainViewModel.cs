using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using LocalEFS.Annotations;

namespace LocalEFS
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private List<File> reader = new List<File>();
        private Thread collectorThread;

        private ThreadStart collectorFunction;


        private string status;

        public string Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        private ObservableCollection<File> files = new ObservableCollection<File>();

        public ObservableCollection<File> Files
        {
            get => files;
            set
            {
                files = value;
                OnPropertyChanged(nameof(Files));
            }
        }

        private File f;

        public File SelectedFile
        {
            get => f;
            set
            {
                f = value;
                OnPropertyChanged(nameof(SelectedFile));
            }
        }

        private string path;

        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged(nameof(Path));
                if (collectorThread != null && collectorThread.IsAlive)
                {
                    Status = "Restarting... ";
                    collectorThread.Abort();
                    collectorThread = new Thread(collectorFunction);
                    collectorThread.Start();
                }
            }
        }


        public MainViewModel()
        {
            this.Path = "*";
            collectorFunction = () =>
            {
                Thread.Sleep(1000);
                //timer = new Timer(y =>
                //{
                //    Application.Current.Dispatcher.Invoke(() => Files = new ObservableCollection<File>(reader));
                //}, null, TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1));
                GetFiles(@"C:\", path);
            };
            collectorThread = new Thread(collectorFunction);
            collectorThread.Start();

        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void GetFiles(string searchPath, string patterns = "*", bool notrecurse = true)
        {
            if (notrecurse)
            {
                Files = new ObservableCollection<File>();
            }

            DirectoryInfo info = new DirectoryInfo(searchPath);
            try
            {
                var file = info.GetFiles(patterns, SearchOption.TopDirectoryOnly);
                foreach (var fileInfo in file)
                {
                    reader.Add(new File(fileInfo));
                }
                var folders = info.GetDirectories();
                foreach (var folder in folders)
                {
                    GetFiles(folder.FullName, patterns, false);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }

            Status = $"Status: parsing files/folders in folder {info.FullName} done";
        }

        private bool access(DirectoryInfo dir)
        {
            DirectorySecurity acl = dir.GetAccessControl(AccessControlSections.All & ~AccessControlSections.Audit);
            AuthorizationRuleCollection rules = acl.GetAccessRules(true, true, typeof(NTAccount));

            //Go through the rules returned from the DirectorySecurity
            foreach (AuthorizationRule rule in rules)
            {
                //If we find one that matches the identity we are looking for
                if (rule.IdentityReference.Value.Equals(Environment.UserName, StringComparison.CurrentCultureIgnoreCase))
                {
                    var filesystemAccessRule = (FileSystemAccessRule)rule;

                    //Cast to a FileSystemAccessRule to check for access rights
                    if ((filesystemAccessRule.FileSystemRights & FileSystemRights.WriteData) > 0 && filesystemAccessRule.AccessControlType != AccessControlType.Deny)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ReleaseUnmanagedResources()
        {
            collectorThread.Abort();
            collectorThread = null;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~MainViewModel()
        {
            ReleaseUnmanagedResources();
        }
    }
}
