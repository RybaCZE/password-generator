using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace password_gen {
    internal class Program {
        #region Clipboard

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool IsClipboardFormatAvailable(uint format);

        const uint CF_UNICODETEXT = 13;

        #endregion

        static void Main(string[] args) {
            if (args.Length == 0) {
                PrintHelp();
                Environment.Exit(0);
            }

            string temp;
            if (args[0] == "-D") {
                temp = Generate(15);
                Console.WriteLine(temp);
            } else {
                if (ConvertToInt(args[0]) is int length) {
                    temp = Generate(length);
                    Console.WriteLine(temp);

                    // Copy to clipboard only on Windows
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                        CopyToClipboard(temp);
                    } else {
                        Console.WriteLine("Clipboard functionality not supported on this platform.");
                    }
                }
            }
        }

        static internal string Generate(int length) {
            int minValue = 33;
            int maxValue = 126;
            int range = maxValue - minValue + 1;
            byte[] randomBytes = new byte[length * 2]; // Double the size for better distribution

            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider()) {
                rng.GetBytes(randomBytes);
            }

            StringBuilder output = new StringBuilder(length);

            for (int i = 0; i < length; i++) {
                int randomIndex = BitConverter.ToUInt16(randomBytes, i * 2) % range;
                int randomChar = minValue + randomIndex;
                output.Append((char)randomChar);
            }

            return output.ToString();
        }

        static int ConvertToInt(string input) {
            int output = 0;
            try {
                output = Convert.ToInt32(input);
            } catch (FormatException) {
                Console.WriteLine($"Unable to convert '{input}' to an integer.");
                Environment.Exit(0);
            } catch (OverflowException) {
                Console.WriteLine($"'{input}' is outside the range of an integer.");
                Environment.Exit(0);
            }

            return output;
        }

        static void CopyToClipboard(string text) {
            if (OpenClipboard(IntPtr.Zero) && EmptyClipboard()) {
                try {
                    SetClipboardData(CF_UNICODETEXT, Marshal.StringToHGlobalUni(text));
                } finally {
                    CloseClipboard();
                }
            }
        }

        static void PrintHelp() {
            Console.WriteLine("Generates a random password of the specified length.");
            Console.WriteLine("Usage: pgen [length]");
            Console.WriteLine("generate random string with default length(15 chars): pgen -D");
        }
    }
}
