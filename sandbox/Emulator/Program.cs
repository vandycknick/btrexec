using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Nvd.SubProcess;

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

            var inputTask = RedirectConsoleToProcessInput(proc);
            var outputTask = RedirectProcessOutputToConsole(proc);

            Console.WriteLine($"Starting process with pid {proc.Pid}...");

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

        
        private static string ConsoleKeyToAnsiEscape(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    return "\x1bOA";
                case ConsoleKey.DownArrow:
                    return "\x1bOB";
                case ConsoleKey.LeftArrow:
                    return "\x1bOD";
                case ConsoleKey.RightArrow:
                    return "\x1bOC";
                case ConsoleKey.Home:
                    return "\x1bOH";
                case ConsoleKey.End:
                    return "\x1bOF";
                case ConsoleKey.Delete:
                    return "\x1b[3~";
                case ConsoleKey.Insert:
                    return "\x1b[2~";
                case ConsoleKey.PageUp:
                    return "\x1b[5~";
                case ConsoleKey.PageDown:
                    return "\x1b[6~";
                case ConsoleKey.F1:
                    return "\x1bOP";
                case ConsoleKey.F2:
                    return "\x1bOQ";
                case ConsoleKey.F3:
                    return "\x1bOR";
                case ConsoleKey.F4:
                    return "\x1bOS";
                case ConsoleKey.F5:
                    return "\x1b[15~";
                case ConsoleKey.F6:
                    return "\x1b[17~";
                case ConsoleKey.F7:
                    return "\x1b[18~";
                case ConsoleKey.F8:
                    return "\x1b[19~";
                case ConsoleKey.F9:
                    return "\x1b[20~";
                case ConsoleKey.F10:
                    return "\x1b[21~";
                case ConsoleKey.F11:
                    return "\x1b[23~";
                case ConsoleKey.F12:
                    return "\x1b[24~";
                default:
                    return $"{key.KeyChar}";
            }
        }

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
