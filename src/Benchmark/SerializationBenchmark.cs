namespace Benchmark
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Configs;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;
    using BenchmarkDotNet.Diagnosers;
    
    [MemoryDiagnoser]
    [RPlotExporter]
    [RankColumn]
    [Config(typeof(Config))]
    public class SerializationBenchmark
    {
        PersonObject personToSerialize;
        
        private class Config : ManualConfig
        {
            public Config()
            {
                Add(MemoryDiagnoser.Default);                
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            personToSerialize = new PersonObject()
            {
                FullName = "Riccardo Terrell",
                Birthday = new DateTime(1975, 07, 07),
                Tags = new Dictionary<string, string>
                {
                    { "Bugghina", "Number one" },
                    { "Stellina", "Number two" }
                }
            };
        }

        [Benchmark]
        public void ProtobufSerialize()
        {
            SerializerHelpers.SerializeProtobuf(personToSerialize);
        }


        [Benchmark(Baseline = true)]
        public void JsonNetSerialization()
        {
            JsonConvert.SerializeObject(personToSerialize);
        }

        [Benchmark]
        public void JsonTextSerialization()
        {
            System.Text.Json.JsonSerializer.Serialize(personToSerialize);
        }

        //[Benchmark]
        //public void JsonNetSerializationWithBuffers()
        //{
        //    SerializerHelpers.SerializeJsonNetBuffers(personToSerialize);
        //}

    }
}
