using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
namespace password_gen {
    internal class Program {

        #region clipboaed
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
        #endregion

        Random random = new Random();

        static void Main(string[] args) {

            if (args.Length == 0) {
                print_help();
                System.Environment.Exit(0);
            }
            string temp;
            if (args[0] == "-D") {
                temp = generate(15);
                Console.WriteLine(temp);
            } else {
                if (convert_to_int(args[0]) is int length) {
                    temp = generate(length);
                    Console.WriteLine(temp);
                }
            }
            copy_to_clipboard(temp);
        }

        static internal string generate(int length) {
            int min_value = 33;
            int max_value = 126;
            int range = max_value - min_value + 1;
            byte[] random_bytes = new byte[length * 2]; // Double the size for better distribution

            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider()) {
                rng.GetBytes(random_bytes);
            }
            StringBuilder output = new StringBuilder(length);

            for (int i = 0; i < length; i++) {
                int randomIndex = BitConverter.ToUInt16(random_bytes, i * 2) % range;
                int random_char = min_value + randomIndex;
                output.Append((char)random_char);
            }

            return output.ToString();
        }

        static int convert_to_int(string input) {
            int output = 0;
            try {
                output = Convert.ToInt32(input);
            } catch (FormatException) {
                Console.WriteLine($"Unable to convert '{input}' to an integer.");
                System.Environment.Exit(0);
            } catch (OverflowException) {
                Console.WriteLine($"'{input}' is outside the range of an integer.");
                System.Environment.Exit(0);
            }
            return output;
        }
        static void copy_to_clipboard(string text) {
            if (OpenClipboard(IntPtr.Zero) && EmptyClipboard()) {
                try { SetClipboardData(13 /*CF_UNICODETEXT*/, Marshal.StringToHGlobalUni(text)); } finally { CloseClipboard(); }
            }
        }
        static void print_help() {
            Console.WriteLine("Generates a random password of the specified length.");
            Console.WriteLine("Usage: pgen [length]");
            Console.WriteLine("generate random string with default length(15 chars): pgen -D");
        }

    }
}
