using System.IO;

namespace Wki.EventSourcing.Tests
{
    public static class TempDir
    {
        public static string CreateTempDir()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            return tempDir;
        }

        public static void RemoveTempDir(string dir)
        {
            Directory.Delete(dir, true);
        }
    }
}
