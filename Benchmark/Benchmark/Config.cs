using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Benchmark;

class Config : ManualConfig
{
    public Config()
    {
        AddColumn(new ColumnSpeed("Avg MB/sec"));
        AddColumn(new ColumnLost("Lost%/Compress"));
        Orderer = new CustomOrderer();
    }

    private class CustomOrderer : IOrderer
    {
        public IEnumerable<BenchmarkCase> GetExecutionOrder(ImmutableArray<BenchmarkCase> benchmarksCase, IEnumerable<BenchmarkLogicalGroupRule> order = null) =>
            benchmarksCase.OrderBy(p => p.Descriptor.WorkloadMethod.Name)
                          .ThenBy(p => (int) p.Parameters["PageSize"]);

        public IEnumerable<BenchmarkCase> GetSummaryOrder(ImmutableArray<BenchmarkCase> benchmarksCase, Summary summary) =>
            benchmarksCase.OrderBy(p => p.Descriptor.WorkloadMethod.Name)
                          .ThenBy(p => (int) p.Parameters["PageSize"]);

        public string GetHighlightGroupKey(BenchmarkCase benchmarkCase) => null;

        public string GetLogicalGroupKey(ImmutableArray<BenchmarkCase> allBenchmarksCases, BenchmarkCase benchmarkCase) =>
            benchmarkCase.Job.DisplayInfo + "_" + benchmarkCase.Parameters.DisplayInfo;

        public IEnumerable<IGrouping<string, BenchmarkCase>> GetLogicalGroupOrder(IEnumerable<IGrouping<string, BenchmarkCase>> logicalGroups,
                                                                                  IEnumerable<BenchmarkLogicalGroupRule>        order = null) => logicalGroups.OrderBy(it => it.Key);

        public bool SeparateLogicalGroups => false;
    }
}