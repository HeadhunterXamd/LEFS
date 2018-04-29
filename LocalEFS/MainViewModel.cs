using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using LocalEFS.Annotations;

namespace LocalEFS
{
    public class MainViewModel : INotifyPropertyChanged
    {
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
            }
        }


        public MainViewModel()
        {
            this.Path = Environment.CurrentDirectory;
            string wildcard = "*";
            ThreadPool.QueueUserWorkItem(x =>
            {
                GetFiles(Path);
            });
            PropertyChanged += (sender, args) => PathChanged(args.PropertyName);
        }

        private void PathChanged(string propertyName)
        {
            if (propertyName.Equals(nameof(Path)))
            {
                GetFiles(Path);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void GetFiles(string path, string patterns = "*")
        {
            Files = new ObservableCollection<File>();
            DirectoryInfo info = new DirectoryInfo(path);
            var file = info.GetFiles(patterns);
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var fileInfo in file)
                {
                    Files.Add(new File(fileInfo));
                }
            }, DispatcherPriority.DataBind);
        }
    }
}
