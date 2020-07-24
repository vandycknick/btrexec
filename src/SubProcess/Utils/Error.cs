using Nvd.SubProcess.Native;

namespace Nvd.SubProcess.Utils
{
    internal class Error
    {
        public static bool ShouldRetrySyscall(int result)
        {
            if (result == -1 && Libc.errno == Libc.EINTR)
            {
                return true;
            }

            return false;
        }

        public static void ThrowExceptionFor(int errno) => new ErrnoException(errno);

        public static void ThrowExceptionForLastError() => ErrnoException.Throw();

        public static void ThrowExceptionForLastErrorIf(int retval)
        {
            if (retval == -1)
            {
                ThrowExceptionForLastError();
            }
        }
    }
}
