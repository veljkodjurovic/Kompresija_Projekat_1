using Projekat_1.Interfaces;
using Projekat_1.Utils;
using System.Text;

namespace Projekat_1.Algorithms
{
    internal class ShannonFano : ICompressionInterface
    {
        private class Node
        {
            public byte Data { get; }
            public int Frequency { get; }
            public string Code { get; private set; }

            public Node(byte data, int frequency)
            {
                Data = data;
                Frequency = frequency;
                Code = "";
            }

            public void AppendCode(char bit)
            {
                Code += bit;
            }
        }

        public string Name => "Shannon-Fano";

        public byte[] Compress(byte[] input)
        {
            if (input == null || input.Length == 0)
                return Array.Empty<byte>();

            int[] byteCounts = AlgorithmUtils.calculateByteCount(input);

            List<Node> nodes = BuildNodes(byteCounts);

            if (nodes.Count == 1)
            {
                nodes[0].AppendCode('0');
            }
            else
            {
                ShannonFanoCompression(nodes, 0, nodes.Count - 1);
            }

            Dictionary<byte, string> codes = BuildCodeMap(nodes);

            int totalBits = AlgorithmUtils.CalculateTotalBits(input, codes);
            byte[] compressedData = AlgorithmUtils.EncodeData(input, codes, totalBits);

            List<byte> final = new List<byte>();

            for (int i = 0; i < 256; i++)
                final.AddRange(BitConverter.GetBytes(byteCounts[i]));

            final.AddRange(BitConverter.GetBytes(totalBits));

            final.AddRange(compressedData);

            return final.ToArray();
        }

        public byte[] Decompress(byte[] input)
        {
            if (input == null || input.Length == 0)
                return Array.Empty<byte>();

            int offset = 0;

            int[] byteCounts = new int[256];

            for (int i = 0; i < 256; i++)
            {
                byteCounts[i] = BitConverter.ToInt32(input, offset);
                offset += 4;
            }

            int totalBits = BitConverter.ToInt32(input, offset);
            offset += 4;

            byte[] encodedData = new byte[input.Length - offset];
            Array.Copy(input, offset, encodedData, 0, encodedData.Length);

            List<Node> nodes = BuildNodes(byteCounts);

            if (nodes.Count == 1)
            {
                byte[] result = new byte[nodes[0].Frequency];
                Array.Fill(result, nodes[0].Data);
                return result;
            }

            ShannonFanoCompression(nodes, 0, nodes.Count - 1);

            Dictionary<string, byte> decodeMap = new Dictionary<string, byte>();

            foreach (var n in nodes)
                decodeMap[n.Code] = n.Data;

            List<byte> output = new List<byte>();
            StringBuilder current = new StringBuilder();

            for (int bitPos = 0; bitPos < totalBits; bitPos++)
            {
                int byteIndex = bitPos / 8;
                int bitIndex = 7 - (bitPos % 8);

                int bit = (encodedData[byteIndex] >> bitIndex) & 1;

                current.Append(bit == 1 ? '1' : '0');

                string code = current.ToString();

                if (decodeMap.ContainsKey(code))
                {
                    output.Add(decodeMap[code]);
                    current.Clear();
                }
            }

            return output.ToArray();
        }

        private void ShannonFanoCompression(List<Node> nodes, int start, int end)
        {
            if (start >= end)
                return;

            int bestIndex = start;
            int bestDiff = int.MaxValue;

            for (int i = start; i < end; i++)
            {
                int left = 0;
                int right = 0;

                for (int j = start; j <= i; j++)
                    left += nodes[j].Frequency;

                for (int j = i + 1; j <= end; j++)
                    right += nodes[j].Frequency;

                int diff = Math.Abs(left - right);

                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    bestIndex = i;
                }
            }

            for (int i = start; i <= bestIndex; i++)
                nodes[i].AppendCode('0');

            for (int i = bestIndex + 1; i <= end; i++)
                nodes[i].AppendCode('1');

            ShannonFanoCompression(nodes, start, bestIndex);
            ShannonFanoCompression(nodes, bestIndex + 1, end);
        }

        private List<Node> BuildNodes(int[] byteCounts)
        {
            List<Node> nodes = new List<Node>();

            for (int i = 0; i < 256; i++)
            {
                if (byteCounts[i] > 0)
                    nodes.Add(new Node((byte)i, byteCounts[i]));
            }

            nodes.Sort((a, b) => b.Frequency.CompareTo(a.Frequency));

            return nodes;
        }

        private Dictionary<byte, string> BuildCodeMap(List<Node> nodes)
        {
            Dictionary<byte, string> map = new Dictionary<byte, string>();

            foreach (var n in nodes)
                map[n.Data] = n.Code;

            return map;
        }

    }
}