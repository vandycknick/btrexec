using System.Runtime.InteropServices;
using static Nvd.SubProcess.Native.Libraries;

namespace Nvd.SubProcess.Native
{
    internal static unsafe partial class Libc
    {
        [DllImport(libc)]
        internal static extern int strerror_r(int errnum, byte* buf, size_t buflen);
    }
}
