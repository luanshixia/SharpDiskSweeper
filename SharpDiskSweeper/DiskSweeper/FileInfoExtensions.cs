using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiskSweeper
{
    /// <summary>
    /// https://stackoverflow.com/questions/3750590/get-size-of-file-on-disk
    /// </summary>
    public static class FileInfoExtensions
    {
        public static long GetSizeOnDisk(this FileInfo info)
        {
            int result = GetDiskFreeSpaceW(
                info.Directory.Root.FullName,
                out uint sectorsPerCluster,
                out uint bytesPerSector,
                out uint dummy,
                out dummy);

            if (result == 0)
            {
                throw new Win32Exception();
            }

            uint losize = GetCompressedFileSizeW(
                info.FullName,
                out uint hosize);

            long size = (long)hosize << 32 | losize;
            uint clusterSize = sectorsPerCluster * bytesPerSector;
            return ((size + clusterSize - 1) / clusterSize) * clusterSize;
        }

        [DllImport("kernel32.dll")]
        private static extern uint GetCompressedFileSizeW(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            [Out, MarshalAs(UnmanagedType.U4)] out uint lpFileSizeHigh);

        [DllImport("kernel32.dll", SetLastError = true, PreserveSig = true)]
        private static extern int GetDiskFreeSpaceW(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName,
            out uint lpSectorsPerCluster,
            out uint lpBytesPerSector,
            out uint lpNumberOfFreeClusters,
            out uint lpTotalNumberOfClusters);
    }
}
