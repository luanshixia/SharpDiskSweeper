using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiskSweeper
{
    public class SweepEngine
    {
        private readonly DirectoryInfo Directory;

        public SweepEngine(string path)
        {
            this.Directory = new DirectoryInfo(path);
        }

        public ObservableCollection<DiskItem> GetDiskItems()
        {
            return new ObservableCollection<DiskItem>(
                collection: this.Directory
                    .GetFileSystemInfos()
                    .Select(info => new DiskItem(info)));
        }

        public static async Task<(long, long)> CalculateDirectorySizeRecursivelyAsync(DirectoryInfo directory, CancellationToken cancellationToken)
        {
            var totalSize = 0L;
            var totalSizeOnDisk = 0L;

            try
            {
                foreach (var childDirectory in directory.GetDirectories())
                {
                    var (size, sizeOnDisk) = await SweepEngine.CalculateDirectorySizeRecursivelyAsync(childDirectory, cancellationToken);
                    totalSize += size;
                    totalSizeOnDisk += sizeOnDisk;

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }

                totalSize += directory.GetFiles().Sum(file => file.Length);
                totalSizeOnDisk += directory.GetFiles().Sum(file => file.GetSizeOnDisk());
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.WriteLine(ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return (totalSize, totalSizeOnDisk);
        }
    }
}
