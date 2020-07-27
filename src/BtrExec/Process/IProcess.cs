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
        bool IsRunning { get; }
        int ExitCode { get; }

        void Kill();
        void Signal(int signal);
        void Wait();
        void Wait(int timeout);
        void SetWindowSize(int height, int width);
    }
}
