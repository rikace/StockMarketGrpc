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
    public class SerializationBenchmark
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
                FullName = "Stefán Jökull Sigurðarson",
                Birthday = new DateTime(1979, 11, 23),
                Tags = new Dictionary<string, string>
                {
                    { "Interest1", "Judo" },
                    { "Interest2", "Music" }
                }
            };

            
            personProtobuf = SerializerHelpers.SerializeProtobuf(personToSerialize);
            personJsonNet = JsonConvert.SerializeObject(personToSerialize);          
            personJsonNetBuffers = SerializerHelpers.SerializeJsonNetBuffers(personToSerialize);
            personJsonText = System.Text.Json.JsonSerializer.Serialize(personToSerialize);
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
           
        }

        [Benchmark]
        public void JsonNetSerializationWithBuffers()
        {
            SerializerHelpers.SerializeJsonNetBuffers(personToSerialize);
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
            System.Text.Json.JsonSerializer.Deserialize<PersonObject>(personToSerialize);
        }


        [Benchmark]
        public void JsonNetWithBuffersDeserialization()
        {
            SerializerHelpers.DeserializeJsonNetBuffers<PersonObject>(personJsonNetBuffers);
        }
    }
}
