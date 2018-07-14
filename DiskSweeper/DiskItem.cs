using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskSweeper
{
    public class DiskItem : INotifyPropertyChanged
    {
        public DiskItemType Type { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public bool IsCalculationDone { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public DiskItem(FileSystemInfo info)
        {
            if (info is FileInfo fileInfo)
            {
                this.Type = DiskItemType.File;
                this.Name = fileInfo.Name;
                this.Size = fileInfo.Length;
            }
            else if (info is DirectoryInfo directoryInfo)
            {
                this.Type = DiskItemType.Directory;
                this.Name = directoryInfo.Name;
                this.Size = 0;
            }
        }

        public async Task Start()
        {

        }

        private void NotifyPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyPropertyChanged()
        {
            this.NotifyPropertyChanged(nameof(this.Size));
        }
    }

    public enum DiskItemType
    {
        File,
        Directory
    }
}
