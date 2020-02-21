using BenchmarkDotNet.Running;
using System;


namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var performanceStats = BenchmarkRunner.Run<SerializationBenchmark>();

            Console.ReadLine();
        }
    }
}