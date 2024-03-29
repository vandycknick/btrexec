using System;
using System.Runtime.InteropServices;
using static BtrExec.Native.Libraries;

namespace BtrExec.Native
{
    internal static partial class Libc
    {
        [DllImport(libutil, SetLastError = true)]
        internal extern static int forkpty(out int controller, IntPtr name, IntPtr termios, ref winsize winp);

        [DllImport(libutil, SetLastError = true)]
        internal extern static int openpty(out int controller, out int host, IntPtr name, IntPtr termios, ref winsize winp);

        [DllImport(libutil, SetLastError = true)]
        internal extern static int login_tty(int fd);
    }
}
