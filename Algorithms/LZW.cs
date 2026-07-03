using Projekat_1.Interfaces;
using Projekat_1.Utils;

namespace Projekat_1.Algorithms
{
    internal class LZW : ICompressionInterface
    {
        public string Name => "LZW";

        public byte[] Compress(byte[] input)
        {
            if (input == null || input.Length == 0)
                return Array.Empty<byte>();

            Dictionary<string, ushort> dictionary = new();

            for (int i = 0; i < 256; i++)
                dictionary[Convert.ToHexString(new byte[] { (byte)i })] = (ushort)i;

            int nextCode = 256;
            int bitWidth = 9;

            List<byte> output = new();

            byte currentByte = 0;
            int bitsInCurrentByte = 0;

            string w = Convert.ToHexString(new byte[] { input[0] });

            for (int i = 1; i < input.Length; i++)
            {
                string c = Convert.ToHexString(new byte[] { input[i] });
                string wc = w + c;

                if (dictionary.ContainsKey(wc))
                {
                    w = wc;
                }
                else
                {
                    AlgorithmUtils.WriteBits(dictionary[w], bitWidth, output, ref currentByte, ref bitsInCurrentByte);

                    if (nextCode < ushort.MaxValue)
                    {
                        dictionary[wc] = (ushort)nextCode;
                        nextCode++;

                        if (nextCode == (1 << bitWidth) && bitWidth < 16)
                            bitWidth++;
                    }

                    w = c;
                }
            }

            AlgorithmUtils.WriteBits(dictionary[w], bitWidth, output, ref currentByte, ref bitsInCurrentByte);

            if (bitsInCurrentByte > 0)
            {
                currentByte <<= (8 - bitsInCurrentByte);
                output.Add(currentByte);
            }

            return output.ToArray();
        }

        public byte[] Decompress(byte[] input)
        {
            if (input == null || input.Length == 0)
                return Array.Empty<byte>();

            Dictionary<ushort, string> dictionary = new();

            for (int i = 0; i < 256; i++)
            {
                string value = Convert.ToHexString(
                    new byte[] { (byte)i }
                );

                dictionary[(ushort)i] = value;
            }

            int nextCode = 256;
            int bitWidth = 9;

            int bytePosition = 0;
            int bitPosition = 0;

            int? firstCode = AlgorithmUtils.ReadBits(input, bitWidth, ref bytePosition, ref bitPosition);

            if (firstCode == null)
                return Array.Empty<byte>();

            string previous = dictionary[(ushort)firstCode.Value];

            List<byte> output = new();

            output.AddRange(
                Convert.FromHexString(previous)
            );

            while (true)
            {
                int? readCode = AlgorithmUtils.ReadBits(input, bitWidth, ref bytePosition, ref bitPosition);

                if (readCode == null)
                    break;

                ushort code = (ushort)readCode.Value;

                string current;

                if (dictionary.ContainsKey(code))
                {
                    current = dictionary[code];
                }
                else if (code == nextCode)
                {
                    string firstByteOfPrevious =
                        previous.Substring(0, 2);

                    current =
                        previous + firstByteOfPrevious;
                }
                else
                {
                    break;
                }

                output.AddRange(
                    Convert.FromHexString(current)
                );

                if (nextCode < ushort.MaxValue)
                {
                    string firstByteOfCurrent =
                        current.Substring(0, 2);

                    dictionary[(ushort)nextCode] =
                        previous + firstByteOfCurrent;

                    nextCode++;

                    if (nextCode == (1 << bitWidth) - 1 &&
                        bitWidth < 16)
                    {
                        bitWidth++;
                    }
                }

                previous = current;
            }

            return output.ToArray();
        }
    }
}