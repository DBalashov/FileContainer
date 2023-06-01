using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FileContainer;

namespace Benchmark;

[SimpleJob(RuntimeMoniker.Net70, baseline: true)]
[WarmupCount(3)]
[IterationCount(3)]
[MemoryDiagnoser]
[Config(typeof(Config))]
public class MainTest
{
    const string testFileName = "9594ED0A-34F5-488E-9935-537951EF606F";
    const string testFileUrl  = "https://github.com/Leaflet/Leaflet/archive/refs/tags/v1.7.1.zip";

    static Dictionary<string, byte[]> entries;
    BenchmarkData                     benchmarkData = new(0, 0);

    [Params(1, 5, 10, 25)]
    public int BatchSize { get; set; }
    
    [Params(256, 512, 2048, 8192, 32768)]
    public int PageSize { get; set; }

    [Params(0, PersistentContainerFlags.WriteDirImmediately)]
    public PersistentContainerFlags Flags { get; set; }

    [Params(PersistentContainerCompressType.None, PersistentContainerCompressType.GZip, PersistentContainerCompressType.LZ4)]
    public PersistentContainerCompressType Compress { get; set; }

    #region Setup / Cleanup

    [GlobalSetup]
    public void GlobalSetup()
    {
        var fileName = Path.Combine(Path.GetTempPath(), testFileName);
        if (!File.Exists(fileName))
        {
            Console.WriteLine("Downloading test file: {0}", testFileUrl);
            File.WriteAllBytes(fileName, new HttpClient().GetByteArrayAsync(testFileUrl).GetAwaiter().GetResult());
        }

        Console.WriteLine("In-memory extracting");
        using var zipFile = Ionic.Zip.ZipFile.Read(fileName);
        entries = zipFile.Entries
                         .Where(p => !p.IsDirectory && p.UncompressedSize > 0)
                         .ToDictionary(p => p.FileName,
                                       p =>
                                       {
                                           using var stm = new MemoryStream();
                                           p.Extract(stm);
                                           return stm.ToArray();
                                       });
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        var fileName = Path.Combine(Path.GetTempPath(), string.Join("_", BatchSize, PageSize, (int) Flags, (int) Compress) + ".json");
        File.WriteAllText(fileName, JsonSerializer.Serialize(benchmarkData));
    }

    #endregion

    [Benchmark]
    public void Single()
    {
        var stm = new MemoryStream();
        using (var container = new InMemoryContainer(stm, new PersistentContainerSettings(PageSize, Flags, Compress)))
        {
            if (BatchSize == 1)
                foreach (var file in entries)
                    container.Put(file.Key, file.Value);
            else
            {
                foreach (var item in entries.Chunk(BatchSize))
                    container.Put(new Dictionary<string, byte[]>(item));
            }
        }

        benchmarkData = new BenchmarkData(entries.Sum(c => c.Value.Length), (int) stm.Length);
    }
}