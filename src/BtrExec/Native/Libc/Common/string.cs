using System.Runtime.InteropServices;
using static BtrExec.Native.Libraries;

namespace BtrExec.Native
{
    internal static unsafe partial class Libc
    {
        [DllImport(libc)]
        internal static extern int strerror_r(int errnum, byte* buf, size_t buflen);
    }
}
