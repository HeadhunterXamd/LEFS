using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using LocalEFS.Annotations;
using LocalEFS.Images;

namespace LocalEFS
{
    public class Tag
    {
        private string tag;
        public string TagName { get => tag; set => tag = value; }

        public Tag(string t)
        {
            tag = t;
        }

    }

    public class File : INotifyPropertyChanged
    {
        private FileInfo source;
        private Tag[] tags;
        private string path;
        private string name;
        private string extension;
        //private Icon icon;
        //public ImageSource ImageSource { get => icon.ToImageSource(); }
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Path { get => path; set => path = value; }
        public string Extension
        {
            get => extension;
            set
            {
                extension = value;
                OnPropertyChanged(nameof(Extension));
            }
        }

        public Tag[] Tags { get => tags; set => tags = value; }

        public File(FileInfo file)
        {
            source = file;
            Path = file.FullName.Substring(0, file.FullName.LastIndexOf(System.IO.Path.DirectorySeparatorChar)+1);
            Name = file.Name;
            //Console.WriteLine(path.Substring(0, path.LastIndexOf(System.IO.Path.DirectorySeparatorChar)));
            Extension = file.Extension;
            //icon = Icon.ExtractAssociatedIcon(file.FullName);
            Tags = Path.Split(System.IO.Path.PathSeparator).Select(x => new Tag(x)).ToArray();
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals(nameof(Name)))
                {
                    try
                    {
                        System.IO.File.Move(source.FullName, path + name);
                        source = new FileInfo(path + name);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        Name = source.Name;
                    }
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
