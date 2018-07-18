using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiskSweeper
{
    public class DiskItem : INotifyPropertyChanged
    {
        public DiskItemType Type { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public long SizeOnDisk { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public string Highlight => this.SizeOnDisk > 1024 * 1024 * 1024
            ? "P0"
            : this.SizeOnDisk > 128 * 1024 * 1024
                ? "P1"
                : null;

        public string SizeString => this.IsCalculationDone
            ? DiskItem.FormatSize(this.Size)
            : "(...)";

        public string SizeOnDiskString => this.IsCalculationDone
            ? DiskItem.FormatSize(this.SizeOnDisk)
            : "(...)";

        public event PropertyChangedEventHandler PropertyChanged;

        private bool IsCalculationDone = false;
        private readonly DirectoryInfo DirInfo;

        public DiskItem(FileSystemInfo info)
        {
            if (info is FileInfo fileInfo)
            {
                this.Type = DiskItemType.File;
                this.Name = fileInfo.Name;
                this.Size = fileInfo.Length;
                this.SizeOnDisk = fileInfo.GetSizeOnDisk();
                this.Created = fileInfo.CreationTime;
                this.Modified = fileInfo.LastWriteTime;
                this.IsCalculationDone = true;
            }
            else if (info is DirectoryInfo directoryInfo)
            {
                this.Type = DiskItemType.Directory;
                this.Name = directoryInfo.Name;
                this.Size = 0;
                this.SizeOnDisk = 0;
                this.Created = directoryInfo.CreationTime;
                this.Modified = directoryInfo.LastWriteTime;
                this.DirInfo = directoryInfo;
            }
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            if (this.Type == DiskItemType.File)
            {
                return;
            }

            (this.Size, this.SizeOnDisk) = await Task.Run(() => SweepEngine
                .CalculateDirectorySizeRecursivelyAsync(this.DirInfo, cancellationToken));

            this.IsCalculationDone = true;
            this.NotifyPropertyChanged(nameof(this.Size));
            this.NotifyPropertyChanged(nameof(this.SizeString));
            this.NotifyPropertyChanged(nameof(this.SizeOnDisk));
            this.NotifyPropertyChanged(nameof(this.SizeOnDiskString));
            this.NotifyPropertyChanged(nameof(this.Highlight));
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string FormatSize(long size)
        {
            if (size < 1024)
            {
                return size + " Byte";
            }
            else if (size < 1024 * 1024)
            {
                return (size / 1024) + " KB";
            }
            else
            {
                return (size / 1024 / 1024) + " MB";
            }
        }
    }

    public enum DiskItemType
    {
        File,
        Directory
    }
}
