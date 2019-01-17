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
        public long FilesCount { get; set; }
        public long FoldersCount { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public string Highlight => this.Size > SweepEngine.P0SizeFloor
            ? "P0"
            : this.Size > SweepEngine.P1SizeFloor
                ? "P1"
                : null;

        public string SizeString => this.IsCalculationDone
            ? DiskItem.FormatSize(this.Size)
            : "..." + DiskItem.FormatSize(this.Size);

        public string SizeOnDiskString => this.IsCalculationDone
            ? DiskItem.FormatSize(this.SizeOnDisk)
            : "..." + DiskItem.FormatSize(this.SizeOnDisk);

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
                this.FilesCount = 1;
                this.FoldersCount = 0;
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
                this.FilesCount = 0;
                this.FoldersCount = 0;
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

            var engine = new SweepEngine(this.DirInfo);
            engine.ReportProgress += (sender, e) => this.ReportChanges(engine);

            await Task.Run(() => engine
                .CalculateDirectorySizeRecursivelyWithUpdateAsync(this.DirInfo, cancellationToken));

            this.IsCalculationDone = true;
            this.ReportChanges(engine);
        }

        private void ReportChanges(SweepEngine engine)
        {
            (this.Size, this.SizeOnDisk, this.FilesCount, this.FoldersCount) = engine.Result;

            this.NotifyPropertyChanged(nameof(this.Size));
            this.NotifyPropertyChanged(nameof(this.SizeString));
            this.NotifyPropertyChanged(nameof(this.SizeOnDisk));
            this.NotifyPropertyChanged(nameof(this.SizeOnDiskString));
            this.NotifyPropertyChanged(nameof(this.FilesCount));
            this.NotifyPropertyChanged(nameof(this.FoldersCount));
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
