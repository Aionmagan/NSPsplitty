using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NSPsplitty
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private string m_FileToSplit = null;
        private string m_Dir = null;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string OutputDir
        {
            get => m_Dir;
            set
            {
                if (value != m_Dir)
                {
                    m_Dir = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string InputFile
        {
            get => m_FileToSplit;
            set
            {
                if (value != m_FileToSplit)
                {
                    m_FileToSplit = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
