using System;
using System.Collections.Generic;
using System.Text;

namespace Projekat_1.Utils
{
    internal class FileUtils
    {

        public static byte[] ReadFile(String path)
        {
            try
            {
                byte[] file = File.ReadAllBytes(path);
                return file;
                
            }
            catch (IOException e)
            {
                Console.WriteLine("Ulazni fajl ne postoji.");
                return null;
            }
        }

        public static void WriteFile(String path, byte[] input)
        {
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(path, input);
        }

        public static bool CompareFiles(byte[] file1, byte[] file2)
        {
            return file1.SequenceEqual(file2);
        }

        public static double CalculateCompressionRatio(byte[] file1, byte[] file2)
        {
            return ((1.0 - (double)file2.Length / file1.Length) * 100);
        }

    }
}
