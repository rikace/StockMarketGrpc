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

namespace Benchmark
{
    [MemoryDiagnoser]
    [RPlotExporter]
    [RankColumn]
    [Config(typeof(Config))]
    public class DeserializationBenchmark
    {
        PersonObject personToSerialize;
        byte[] personProtobuf;
        string personJsonNet;
        string personJsonNetBuffers;
        string personJsonText;

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


            personProtobuf = SerializerHelpers.SerializeProtobuf(personToSerialize);
            personJsonNet = JsonConvert.SerializeObject(personToSerialize);
            personJsonNetBuffers = SerializerHelpers.SerializeJsonNetBuffers(personToSerialize);
            personJsonText = System.Text.Json.JsonSerializer.Serialize(personToSerialize);
        }

       
        [Benchmark]
        public void ProtobufDeserialize()
        {
            SerializerHelpers.DeserializeProtobuf<PersonObject>(personProtobuf);
        }


        [Benchmark]
        public void JsonNetDeserialization()
        {
            JsonConvert.DeserializeObject<PersonObject>(personJsonNet);
        }

        [Benchmark]
        public void JsonTextDeserialization()
        {
            System.Text.Json.JsonSerializer.Deserialize<PersonObject>(personJsonText);
        }

        //[Benchmark]
        //public void JsonNetWithBuffersDeserialization()
        //{
        //    SerializerHelpers.DeserializeJsonNetBuffers<PersonObject>(personJsonNetBuffers);
        //}
    }
}
