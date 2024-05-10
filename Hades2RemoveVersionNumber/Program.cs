using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.Steam;
using GameFinder.StoreHandlers.Steam.Models.ValueTypes;
using NexusMods.Paths;

namespace Hades2RemoveVersionNumber
{
    class Program
    {
        public static void Main(string[] args)
        {
            var handler = new SteamHandler(FileSystem.Shared, OperatingSystem.IsWindows() ? WindowsRegistry.Shared : null);
            var game = handler.FindOneGameById(AppId.From(1145350), out var errors);
            Console.WriteLine("[INFO] Attempting to find Hades II file path...");
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    Console.WriteLine($"[WARN] {error}");
                }
                //Console.WriteLine("[ERROR] Failed patching Hades II, see above error(s)!");
            }
            if (game == null || game?.Path == default)
            {
                Console.WriteLine("[ERROR] Failed to find Hades II Steam game directory!");
                return;
            }

            Console.WriteLine("[INFO] Found Hades II!");
            var hades2Path = game.Path;
            Console.WriteLine($"[INFO] Located at {game.Path}");
            var hades2ExecutablePath = hades2Path.Combine("Ship").Combine("Hades2.exe");

            Console.WriteLine($"[INFO] Reading game exe file into bytes...");
            var hades2Bytes = File.ReadAllBytes(hades2ExecutablePath.ToString());
            Console.WriteLine("[INFO] Backing up game exe file to Hades2.exe.bak...");
            File.WriteAllBytes(hades2Path.Combine("Ship").Combine("Hades2.exe.bak").ToString(), hades2Bytes);

            // Patching
            Console.WriteLine($"[INFO] Locating 'v0.' version prefix in executable...");
            var versionPrefixLocation = IndexOfSequence(hades2Bytes, [0x00, 0x76, 0x30, 0x2E], 0).First();
            Console.WriteLine($"[INFO] Locating version in executable...");
            var versionLocation = IndexOfSequence(hades2Bytes, [0xA0, 0x59, 0x01, 0x40, 0x01, 0x00, 0x00, 0x00, 0xA0, 0x59, 0x01, 0x40, 0x01, 0x00, 0x00, 0x00], 0).First();
            Console.WriteLine($"[INFO] Replacing hex values ...");
            hades2Bytes[versionPrefixLocation + 1] = 0x00;
            hades2Bytes[versionLocation + 16] = 0x00;
            Console.WriteLine($"[INFO] Overwriting exe file with patched version ...");
            File.WriteAllBytes(hades2ExecutablePath.ToString(), hades2Bytes);
            Console.WriteLine($"[INFO] Complete! Warning: please do NOT file bug reports to Supergiant with this patched version!");
            Console.WriteLine($"Press any key to exit...");
            Console.ReadKey();

        }


        // Thanks to 'Just a learner' on StackOverflow!
        public static List<int> IndexOfSequence(byte[] buffer, byte[] pattern, int startIndex)
        {
            List<int> positions = new List<int>();
            int i = Array.IndexOf(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual(pattern))
                    positions.Add(i);
                i = Array.IndexOf(buffer, pattern[0], i + 1);
            }
            return positions;
        }
    }
}
