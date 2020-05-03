using BenchmarkDotNet.Running;
using BenchmarkUtils;
using System;


namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var performanceBenchmark = BenchmarkRunner.Run<SerializationBenchmark>();
            // var performanceBenchmark = BenchmarkRunner.Run<DeserializationBenchmark>();
            var summary = Charting.MapSummary(performanceBenchmark);

            Charting.DrawSummaryReport(summary);
            Console.ReadLine();
        }
    }
}