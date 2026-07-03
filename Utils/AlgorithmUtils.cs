using System;
using System.Collections.Generic;
using System.Text;

namespace Projekat_1.Utils
{
    internal class AlgorithmUtils
    {
        public static double calculateByteEntropy(byte[] input)
        {
            double entropy = 0.0;
            int[] bytesCount = calculateByteCount(input);

            for(int i = 0; i < bytesCount.Length; i++)
            {
                double pi = (double)bytesCount[i] / input.Length;

                if (pi.CompareTo(0) == 0)
                    continue;
                entropy += (-pi * Math.Log2(pi));
            }

            return entropy;
        }

        public static int[] calculateByteCount(byte[] input)
        {
            int[] bytesCount = new int[256];

            for (int i = 0; i < input.Length; i++)
            {
                bytesCount[input[i]]++;
            }

            return bytesCount;

        }

        public static int CalculateTotalBits(byte[] input, Dictionary<byte, string> codes)
        {
            int total = 0;

            foreach (byte b in input)
                total += codes[b].Length;

            return total;
        }

        public static byte[] EncodeData(byte[] input, Dictionary<byte, string> codes, int totalBits)
        {
            byte[] output = new byte[(totalBits + 7) / 8];

            int bitPos = 0;

            foreach (byte b in input)
            {
                string code = codes[b];

                foreach (char bit in code)
                {
                    if (bit == '1')
                    {
                        int byteIndex = bitPos / 8;
                        int bitIndex = 7 - (bitPos % 8);

                        output[byteIndex] |= (byte)(1 << bitIndex);
                    }

                    bitPos++;
                }
            }

            return output;
        }

        public static int? ReadBits(byte[] input, int bitCount, ref int bytePosition, ref int bitPosition)
        {
            int remainingBits = (input.Length - bytePosition) * 8 - bitPosition;

            if (remainingBits < bitCount)
                return null;

            int value = 0;

            for (int i = 0; i < bitCount; i++)
            {
                int bit = (input[bytePosition] >> (7 - bitPosition)) & 1;

                value = (value << 1) | bit;

                bitPosition++;

                if (bitPosition == 8)
                {
                    bitPosition = 0;
                    bytePosition++;
                }
            }

            return value;
        }

        public static void WriteBits(int value, int bitCount, List<byte> output, ref byte currentByte, ref int bitsInCurrentByte)
        {
            for (int i = bitCount - 1; i >= 0; i--)
            {
                int bit = (value >> i) & 1;

                currentByte <<= 1;
                currentByte |= (byte)bit;

                bitsInCurrentByte++;

                if (bitsInCurrentByte == 8)
                {
                    output.Add(currentByte);
                    currentByte = 0;
                    bitsInCurrentByte = 0;
                }
            }
        }

    }
}
