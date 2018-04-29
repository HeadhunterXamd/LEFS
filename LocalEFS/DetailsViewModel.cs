using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LocalEFS.Annotations;

namespace LocalEFS
{
    public class DetailsViewModel : INotifyPropertyChanged
    {
        private File file;

        public DetailsViewModel(File file)
        {
            File = file;
        }

        public File File
        {
            get => file;
            set
            {
                file = value;
                OnPropertyChanged(nameof(File));
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
