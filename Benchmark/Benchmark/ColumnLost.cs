using System.Globalization;
using System.IO;
using System.Text.Json;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using FileContainer;

namespace Benchmark;

public class ColumnLost : IColumn
{
    public string Id         { get; }
    public string ColumnName { get; }

    public ColumnLost(string columnName)
    {
        ColumnName = columnName;
        Id         = nameof(TagColumn) + "." + ColumnName;
    }

    public bool   IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public string GetValue(Summary  summary, BenchmarkCase benchmarkCase) => GetValue(summary, benchmarkCase, SummaryStyle.Default);

    public          bool           IsAvailable(Summary summary) => true;
    public          bool           AlwaysShow                   => true;
    public          ColumnCategory Category                     => ColumnCategory.Custom;
    public          int            PriorityInCategory           => 0;
    public          bool           IsNumeric                    => false;
    public          UnitType       UnitType                     => UnitType.Dimensionless;
    public          string         Legend                       => "Lost space %";
    public override string         ToString()                   => ColumnName;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
    {
        var batchSize     = (int) benchmarkCase.Parameters["BatchSize"];
        var pageSize      = (int) benchmarkCase.Parameters["PageSize"];
        var flags         = (int) (PersistentContainerFlags) benchmarkCase.Parameters["Flags"];
        var compress      = (PersistentContainerCompressType) benchmarkCase.Parameters["Compress"];
        var fileName      = Path.Combine(Path.GetTempPath(), string.Join("_", batchSize, pageSize, flags, (int) compress) + ".json");
        var benchmarkData = JsonSerializer.Deserialize<BenchmarkData>(File.ReadAllText(fileName));

        if (compress != PersistentContainerCompressType.None)
        {
            var xCompress = benchmarkData.RawLength / (double) benchmarkData.StreamLength;
            return "x" + xCompress.ToString("F1", CultureInfo.InvariantCulture);
        }
        else
        {
            var percent = (benchmarkData.StreamLength - benchmarkData.RawLength) / (double) benchmarkData.StreamLength * 100;
            return percent.ToString("F1", CultureInfo.InvariantCulture);
        }
    }
}