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
    public class DeserializationBenchmark
    {
        PersonObject _personToSerialize;
        byte[] _personProtobuf;
        string _personJsonNet;
        //string personJsonNetBuffers;
        string _personJsonText;

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
            _personToSerialize = new PersonObject()
            {
                FullName = "Riccardo Terrell",
                Birthday = new DateTime(1975, 07, 07),
                Tags = new Dictionary<string, string>
                {
                    { "Bugghina", "Number one" },
                    { "Stellina", "Number two" }
                }
            };


            _personProtobuf = SerializerHelpers.SerializeProtobuf(_personToSerialize);
            _personJsonNet = JsonConvert.SerializeObject(_personToSerialize);
            // personJsonNetBuffers = SerializerHelpers.SerializeJsonNetBuffers(_personToSerialize);
            _personJsonText = System.Text.Json.JsonSerializer.Serialize(_personToSerialize);
        }

       
        [Benchmark]
        public void ProtobufDeserialize()
        {
            SerializerHelpers.DeserializeProtobuf<PersonObject>(_personProtobuf);
        }


        [Benchmark(Baseline = true)]
        public void JsonNetDeserialization()
        {
            JsonConvert.DeserializeObject<PersonObject>(_personJsonNet);
        }

        [Benchmark]
        public void JsonTextDeserialization()
        {
            System.Text.Json.JsonSerializer.Deserialize<PersonObject>(_personJsonText);
        }

        //[Benchmark]
        //public void JsonNetWithBuffersDeserialization()
        //{
        //    SerializerHelpers.DeserializeJsonNetBuffers<PersonObject>(personJsonNetBuffers);
        //}
    }
}
