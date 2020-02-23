using BenchmarkDotNet.Running;
using BenchmarkUtils;
using System;


namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var performanceSerializeStats = BenchmarkRunner.Run<SerializationBenchmark>();          
            var summary = Charting.MapSummary(performanceSerializeStats);

            //var performanceDeserializerStats = BenchmarkRunner.Run<DeserializationBenchmark>();
            //var summary = Charting.MapSummary(performanceDeserializerStats);

            Charting.DrawSummaryReport(summary);
            Console.ReadLine();
        }
    }
}