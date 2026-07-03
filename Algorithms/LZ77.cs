using Projekat_1.Interfaces;
using Projekat_1.Utils;

namespace Projekat_1.Algorithms
{
    internal class LZ77 : ICompressionInterface
    {
        class Token
        {
            public ushort Offset { get; set; }
            public byte Length { get; set; }
            public byte NextSymbol { get; set; }

            public Token(ushort offset, byte length, byte nextSymbol)
            {
                Offset = offset;
                Length = length;
                NextSymbol = nextSymbol;
            }
        }

        private readonly int windowLength;
        private readonly int offsetBits;

        public string Name => "LZ77";

        public LZ77(int windowLength)
        {
            if (windowLength <= 0)
                throw new ArgumentException("Duzina prozora mora biti veca od 0.");

            if (windowLength > ushort.MaxValue)
                throw new ArgumentException("Duzina prozora mora biti manja od 65535.");

            this.windowLength = windowLength;
            this.offsetBits = CalculateBits(windowLength);
        }

        public byte[] Compress(byte[] input)
        {
            if (input == null || input.Length == 0)
                return Array.Empty<byte>();

            List<Token> tokens = new List<Token>();

            int i = 0;

            while (i < input.Length)
            {
                int bestLength = 0;
                int bestOffset = 0;

                int searchStart = Math.Max(0, i - windowLength);

                for (int j = searchStart; j < i; j++)
                {
                    int length = 0;

                    while (
                        i + length < input.Length &&
                        input[j + length] == input[i + length] &&
                        length < byte.MaxValue)
                    {
                        length++;

                        if (j + length >= i + length)
                            break;
                    }

                    if (length > bestLength)
                    {
                        bestLength = length;
                        bestOffset = i - j;
                    }
                }

                byte nextSymbol = 0;

                if (i + bestLength < input.Length)
                    nextSymbol = input[i + bestLength];

                tokens.Add(new Token(
                    (ushort)bestOffset,
                    (byte)bestLength,
                    nextSymbol
                ));

                i += bestLength + 1;
            }

            List<byte> output = new List<byte>();

            output.AddRange(BitConverter.GetBytes(input.Length));
            output.AddRange(BitConverter.GetBytes(windowLength));

            byte currentByte = 0;
            int bitsInCurrentByte = 0;

            foreach (Token token in tokens)
            {
                AlgorithmUtils.WriteBits(token.Offset, offsetBits, output, ref currentByte, ref bitsInCurrentByte);
                AlgorithmUtils.WriteBits(token.Length, 8, output, ref currentByte, ref bitsInCurrentByte);
                AlgorithmUtils.WriteBits(token.NextSymbol, 8, output, ref currentByte, ref bitsInCurrentByte);
            }

            if (bitsInCurrentByte > 0)
            {
                currentByte <<= 8 - bitsInCurrentByte;
                output.Add(currentByte);
            }

            return output.ToArray();
        }

        public byte[] Decompress(byte[] input)
        {
            if (input == null || input.Length < 8)
                return Array.Empty<byte>();

            int originalLength = BitConverter.ToInt32(input, 0);
            int savedWindowLength = BitConverter.ToInt32(input, 4);
            int savedOffsetBits = CalculateBits(savedWindowLength);

            List<byte> output = new List<byte>();

            int bytePosition = 8;
            int bitPosition = 0;

            while (output.Count < originalLength)
            {
                int? offsetValue = AlgorithmUtils.ReadBits(input, savedOffsetBits, ref bytePosition, ref bitPosition);
                int? lengthValue = AlgorithmUtils.ReadBits(input, 8, ref bytePosition, ref bitPosition);
                int? nextSymbolValue = AlgorithmUtils.ReadBits(input, 8, ref bytePosition, ref bitPosition);

                if (offsetValue == null || lengthValue == null || nextSymbolValue == null)
                    break;

                int offset = offsetValue.Value;
                int length = lengthValue.Value;
                byte nextSymbol = (byte)nextSymbolValue.Value;

                if (length > 0 && offset > 0)
                {
                    int start = output.Count - offset;

                    for (int i = 0; i < length && output.Count < originalLength; i++)
                    {
                        output.Add(output[start + i]);
                    }
                }

                if (output.Count < originalLength)
                    output.Add(nextSymbol);
            }

            return output.ToArray();
        }

        private static int CalculateBits(int value)
        {
            int bits = 0;

            while (value > 0)
            {
                bits++;
                value >>= 1;
            }

            return bits;
        }




    }
}