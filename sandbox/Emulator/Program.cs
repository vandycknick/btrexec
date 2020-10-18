using System;
using System.Buffers;
using System.IO;
using System.Text;
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

            proc.SetWindowSize(Console.WindowHeight, Console.WindowWidth);

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                if (!proc.HasExited) proc.Kill();
            };
            Console.TreatControlCAsInput = true;    

            Console.WriteLine($"Starting process with pid {proc.Pid}...");
            
            var source = new CancellationTokenSource();
            var inputTask = RedirectConsoleToStream(proc.Stdin.BaseStream, source.Token);
            var outputTask = proc.Stdout.BaseStream.CopyToAsync(Console.OpenStandardOutput(), source.Token);
            var errTask = proc.Stderr.BaseStream.CopyToAsync(Console.OpenStandardError(), source.Token);

            await Task.WhenAny(inputTask, outputTask, errTask);

            source.Cancel();
            proc.Wait();

            Console.WriteLine($"ExitCode: {proc.ExitCode}.");
            return proc.ExitCode;
        }

        private static async Task RedirectConsoleToStream(Stream stream, CancellationToken token)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(10);

            try
            {
                await Task.Yield(); // Need to make sure the rest immediatly runs on a seperate thread, otherwise Readkey will block
         
                while (true)
                {
                    var key = Console.ReadKey(intercept: true);
                    var data = ConsoleKeyToAnsiEscape(key);
                    var written = Encoding.UTF8.GetBytes(data, buffer);

                    if (written == 0) break;

                    await stream.WriteAsync(buffer, 0, written, token);
                    await stream.FlushAsync(token);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
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
    }
}
