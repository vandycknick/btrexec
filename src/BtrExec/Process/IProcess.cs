using System;
using System.IO;

namespace BtrExec
{
    public interface IProcess : IDisposable
    {
        StreamWriter Stdin { get; }
        StreamReader Stdout { get; }
        StreamReader Stderr { get; }

        int Pid { get; }
        bool HasExited { get; }
        int ExitCode { get; }

        bool Poll();
        void Signal(int signal);
        void Kill();
        void Wait();
        void Wait(int timeout);
        void SetWindowSize(int height, int width);
    }
}
