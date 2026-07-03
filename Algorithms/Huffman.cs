using Projekat_1.Interfaces;
using Projekat_1.Utils;

namespace Projekat_1.Algorithms
{
    internal class Huffman : ICompressionInterface
    {

        class Node
        {
            public int Frequency { get; set; }
            public byte Data { get; set; }
            public Node? Left {  get; set; }
            public Node? Right { get; set; }
        }
        public string Name => "Huffman";

        public byte[] Compress(byte[] input)
        {
            if (input == null || input.Length == 0)
                return Array.Empty<byte>();

            int[] bytesCount = AlgorithmUtils.calculateByteCount(input);

            List<Node> huffmanNodes = new List<Node>();
            for(int i=0;i<bytesCount.Length;i++)
            {
                if (bytesCount[i] == 0)
                    continue;
                Node node = new Node();
                node.Data = (byte)i;
                node.Frequency = bytesCount[i];
                node.Left = node.Right = null;

                huffmanNodes.Add(node);
            }

            while (huffmanNodes.Count > 1)
            {
                var sorted = huffmanNodes.OrderBy(n => n.Frequency).Take(2).ToList();

                Node left = sorted[0];
                Node right = sorted[1];

                Node node = new Node();
                node.Frequency = left.Frequency + right.Frequency;
                node.Left = left;
                node.Right = right;

                huffmanNodes.Add(node);

                huffmanNodes.Remove(left);
                huffmanNodes.Remove(right);
            }

            Node root = huffmanNodes[huffmanNodes.Count - 1];

            Dictionary<byte, string> codes = new Dictionary<byte, string>();

            GenerateHuffmanCodes(root, "", codes);

            int totalBits = AlgorithmUtils.CalculateTotalBits(input, codes);
            byte[] compressedData = AlgorithmUtils.EncodeData(input, codes, totalBits);

            List<byte> final = new List<byte>();

            for (int i = 0; i < 256; i++)
                final.AddRange(BitConverter.GetBytes(bytesCount[i]));

            final.AddRange(BitConverter.GetBytes(totalBits));

            final.AddRange(compressedData);

            return final.ToArray();
        }

        private void GenerateHuffmanCodes(Node root, string code, Dictionary<byte, string> mapTable)
        {
            if (root == null)
                return;
            else if(root.Left == null && root.Right == null)
            {
                mapTable[root.Data] = code == "" ? "0" : code;
                return;
            }
            else
            {
                GenerateHuffmanCodes(root.Left, code + "0", mapTable);
                GenerateHuffmanCodes(root.Right, code + "1", mapTable);
            }
        }
        public byte[] Decompress(byte[] input)
        {
            if (input == null || input.Length < 1028)
                return Array.Empty<byte>();

            int[] bytesCount = new int[256];
            int index = 0;
            for (int i = 0; i < 256; i++)
            {
                bytesCount[i] = BitConverter.ToInt32(input, index);
                index += 4;
            }

            int totalBits = BitConverter.ToInt32(input, index);
            index += 4;

            List<Node> huffmanNodes = new List<Node>();
            for (int i = 0; i < bytesCount.Length; i++)
            {
                if (bytesCount[i] == 0) continue;

                Node node = new Node { Data = (byte)i, Frequency = bytesCount[i] };
                huffmanNodes.Add(node);
            }

            while (huffmanNodes.Count > 1)
            {
                var sorted = huffmanNodes.OrderBy(n => n.Frequency).Take(2).ToList();
                Node left = sorted[0];
                Node right = sorted[1];

                Node parent = new Node
                {
                    Frequency = left.Frequency + right.Frequency,
                    Left = left,
                    Right = right
                };

                huffmanNodes.Add(parent);
                huffmanNodes.Remove(left);
                huffmanNodes.Remove(right);
            }

            Node root = huffmanNodes[0];

            List<byte> decompressedData = new List<byte>();
            Node current = root;
            int bitsRead = 0;

            for (int i = index; i < input.Length && bitsRead < totalBits; i++)
            {
                byte b = input[i];

                for (int bit = 7; bit >= 0 && bitsRead < totalBits; bit--)
                {
                    int bitValue = (b >> bit) & 1;

                    if (bitValue == 0)
                        current = current.Left;
                    else
                        current = current.Right;

                    if (current.Left == null && current.Right == null)
                    {
                        decompressedData.Add(current.Data);
                        current = root;
                    }

                    bitsRead++;
                }
            }

            return decompressedData.ToArray();
        }
    }

            
    }
