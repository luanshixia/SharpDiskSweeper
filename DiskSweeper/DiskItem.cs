using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskSweeper
{
    public class DiskItem
    {
        public DiskItemType Type { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
    }

    public enum DiskItemType
    {
        Unknown,
        File,
        Directory
    }
}
