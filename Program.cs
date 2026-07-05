using Projekat_1.Algorithms;
using Projekat_1.Interfaces;
using Projekat_1.Utils;

List<ICompressionInterface> algorithms = new List<ICompressionInterface>();

ShannonFano sf = new ShannonFano();
Huffman huffman = new Huffman();
LZ77 lz77 = new LZ77(1024);
LZW lzw = new LZW();

algorithms.Add(sf);
algorithms.Add(huffman);
algorithms.Add(lz77);
algorithms.Add(lzw);

Console.Write("Unesite putanju do ulaznog fajla: ");
string inputPath = Console.ReadLine();

Console.Write("Unesite putanju do foldera gde zelite da sacuvate kompresovane fajlove: ");
string outputPath = Console.ReadLine();

if (!File.Exists(inputPath))
{
    Console.WriteLine("Fajl ne postoji.");
    return;
}

if (!Directory.Exists(outputPath))
{
    Console.WriteLine("Folder ne postoji.");
    return;
}

string compressedPath = Path.Combine(outputPath, "Compressed");
string decompressedPath = Path.Combine(outputPath, "Decompressed");

Directory.CreateDirectory(compressedPath);
Directory.CreateDirectory(decompressedPath);

byte[] originalFile = FileUtils.ReadFile(inputPath);

string originalExtension = Path.GetExtension(inputPath);

string originalName = Path.GetFileNameWithoutExtension(inputPath);

Console.WriteLine("\nEntropija ulaznog fajla je: " + AlgorithmUtils.calculateByteEntropy(originalFile).ToString("F2"));

//////////////////////////////////////////////////////////////

foreach(var algorithm in algorithms)
{
    Console.WriteLine($"\n[{algorithm.Name}] Kompresija u toku...");

    byte[] compressed = algorithm.Compress(originalFile);

    FileUtils.WriteFile(Path.Combine(compressedPath, originalName + $"_{algorithm.Name}_" + "Compressed.bin"), compressed);

    Console.WriteLine($"[{algorithm.Name}] Kompresija zavrsena.");

    byte[] compressedFile = FileUtils.ReadFile(Path.Combine(compressedPath, originalName + $"_{algorithm.Name}_" + "Compressed.bin"));

    Console.WriteLine($"[{algorithm.Name}] Dekompresija u toku...");

    byte[] decompressed = algorithm.Decompress(compressedFile);

    FileUtils.WriteFile(Path.Combine(decompressedPath, originalName + $"_{algorithm.Name}_" + "Decompressed" + originalExtension), decompressed);

    Console.WriteLine($"[{algorithm.Name}] Dekompresija zavrsena.");

    Console.WriteLine($"[{algorithm.Name}] Stepen kompresije: " + FileUtils.CalculateCompressionRatio(originalFile, compressedFile).ToString("F2") + "%");

    if (FileUtils.CompareFiles(originalFile, decompressed) == true)
        Console.WriteLine($"[{algorithm.Name}] Ulazni fajl i dekompresovani fajl JESU identicni.");
    else
        Console.WriteLine($"[{algorithm.Name}] Ulazni fajl i dekompresovani fajl NISU identicni.");
}

