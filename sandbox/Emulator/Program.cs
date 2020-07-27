using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using BtrExec;

namespace Emulator
{
    static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var attr = new ProcAttr
            {
                RedirectStdin = true,
                RedirectStdout = true,
                RedirectStderr = true,
                Sys = new SysProcAttr
                {
                    UseTty = true,
                },
                Dir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            };
            var proc = Process.Start(Process.GetDefaultShell(), Array.Empty<string>(), attr);

            while (!proc.IsRunning)
            {
                await Task.Yield();
            }

            proc.SetWindowSize(Console.WindowHeight, Console.WindowWidth);

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                if (proc.IsRunning) proc.Kill();
            };

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                proc.Stdin.Write('\u0003');
                proc.Stdin.Flush();
            };

            Console.WriteLine($"Starting process with pid {proc.Pid}...");

            var inputTask = RedirectConsoleToProcessInput(proc);
            var outputTask = RedirectProcessOutputToConsole(proc);

            await proc.WaitAsync();

            Console.WriteLine($"ExitCode: {proc.ExitCode}.");
            return proc.ExitCode;
        }

        private static async Task RedirectConsoleToProcessInput(IProcess proc)
        {
            await Task.Yield();

            while (proc.IsRunning)
            {
                var key = Console.ReadKey(intercept: true);
                var data = ConsoleKeyToAnsiEscape(key);

                await proc.Stdin.WriteAsync(data);
                await proc.Stdin.FlushAsync();
            }
        }

        private static async Task RedirectProcessOutputToConsole(IProcess proc)
        {
            await Task.Yield();

            var pool = ArrayPool<char>.Shared;
            while (proc.IsRunning)
            {
                var buffer = pool.Rent(1024);
                var read = await proc.Stdout.ReadAsync(buffer.AsMemory());
                Console.Write(buffer, 0, read);
            }
        }

        
        private static string ConsoleKeyToAnsiEscape(ConsoleKeyInfo key) =>
            key.Key switch
            {
                ConsoleKey.UpArrow => "\x1bOA",
                ConsoleKey.DownArrow => "\x1bOB",
                ConsoleKey.LeftArrow => "\x1bOD",
                ConsoleKey.RightArrow => "\x1bOC",
                ConsoleKey.Home => "\x1bOH",
                ConsoleKey.End => "\x1bOF",
                ConsoleKey.Delete => "\x1b[3~",
                ConsoleKey.Insert => "\x1b[2~",
                ConsoleKey.PageUp => "\x1b[5~",
                ConsoleKey.PageDown => "\x1b[6~",
                ConsoleKey.F1 => "\x1bOP",
                ConsoleKey.F2 => "\x1bOQ",
                ConsoleKey.F3 => "\x1bOR",
                ConsoleKey.F4 => "\x1bOS",
                ConsoleKey.F5 => "\x1b[15~",
                ConsoleKey.F6 => "\x1b[17~",
                ConsoleKey.F7 => "\x1b[18~",
                ConsoleKey.F8 => "\x1b[19~",
                ConsoleKey.F9 => "\x1b[20~",
                ConsoleKey.F10 => "\x1b[21~",
                ConsoleKey.F11 => "\x1b[23~",
                ConsoleKey.F12 => "\x1b[24~",
                _ => $"{key.KeyChar}",
            };

        public static Task WaitAsync(this IProcess proc)
        {
            var source = new TaskCompletionSource<object>();

            var start = new ThreadStart(() =>
            {
                proc.Wait();
                source.SetResult(null);
            });
            var thread = new Thread(start);
            thread.Start();

            return source.Task;
        }
    }
}
