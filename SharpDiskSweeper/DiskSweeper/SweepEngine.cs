using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<string, (long, long, long, long)> ResultCache = new Dictionary<string, (long, long, long, long)>();

        private readonly DirectoryInfo Directory;
        private const int ProgressReportInterval = 200;

        private long TotalSize;
        private long TotalSizeOnDisk;
        private long TotalFilesCount;
        private long TotalFoldersCount;

        private long ChangesCount;

        public event EventHandler ReportProgress;

        public (long, long, long, long) Result => (this.TotalSize, this.TotalSizeOnDisk, this.TotalFilesCount, this.TotalFoldersCount);

        public static long P0SizeFloor => ConfigurationHelper.GetConfigurationInt64(
            settingName: "DiskSweeper.Highlights.P0.SizeFloor",
            defaultValue: 1073741824L);

        public static long P1SizeFloor => ConfigurationHelper.GetConfigurationInt64(
            settingName: "DiskSweeper.Highlights.P1.SizeFloor",
            defaultValue: 134217728L);

        public SweepEngine(DirectoryInfo dir)
        {
            this.Directory = dir;
        }

        public async Task CalculateDirectorySizeRecursivelyWithUpdateAsync(DirectoryInfo directory, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var childDirectory in directory.GetDirectories())
                {
                    await this.CalculateDirectorySizeRecursivelyWithUpdateAsync(childDirectory, cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }

                var files = directory.GetFiles();
                this.TotalSize += files.Sum(file => file.Length);
                //this.TotalSizeOnDisk += files.Sum(file => file.GetSizeOnDisk());
                this.TotalFilesCount += files.Count();
                this.TotalFoldersCount += 1;

                this.CountChanges();
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.WriteLine(ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private async Task<(long, long, long, long)> GetDirectorySizeRecursivelyAsync(DirectoryInfo directory, CancellationToken cancellationToken)
        {
            try
            {
                var childDirResults = await Task.WhenAll(directory.GetDirectories()
                    .Select(childDirectory => this.GetDirectorySizeRecursivelyAsync(childDirectory, cancellationToken)));

                var files = directory.GetFiles();

                var result = (
                    childDirResults.Sum(r => r.Item1) + files.Sum(file => file.Length), 
                    childDirResults.Sum(r => r.Item2), 
                    childDirResults.Sum(r => r.Item3) + files.Length, 
                    childDirResults.Sum(r => r.Item4) + 1);

                SweepEngine.ResultCache[directory.FullName] = result;

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.WriteLine(ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return (0, 0, 0, 0);
        }

        private void CountChanges()
        {
            this.ChangesCount++;
            if (this.ChangesCount % SweepEngine.ProgressReportInterval == 0)
            {
                this.ReportProgress?.Invoke(this, null);
            }
        }
    }
}
