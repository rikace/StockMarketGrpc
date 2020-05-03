namespace Benchmark
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using ProtoBuf;

    
      public static class SerializerHelpers
    {
        private static ArrayPool<byte> bytePool = ArrayPool<byte>.Shared;
        private static JsonSerializer serializer = JsonSerializer.CreateDefault();

        public static byte[] SerializeProtobuf<T>(T value)
        {
            byte[] buffer = bytePool.Rent(128);
            using (var stream = new MemoryStream(buffer))
            {
                Serializer.Serialize(stream, value);
                var result = new byte[stream.Position];
                Buffer.BlockCopy(buffer, 0, result, 0, result.Length);
                bytePool.Return(buffer);
                return result;
            }
        }

        public static T DeserializeProtobuf<T>(byte[] value)
        {
            using (var valueBytes = new MemoryStream(value))
            {
                return Serializer.Deserialize<T>(valueBytes);
            }
        }

        public static string SerializeJsonNetBuffers(object value)
        {
            using (var stringWriter = new StringWriter(new StringBuilder(256)))
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    jsonTextWriter.ArrayPool = CharArrayPool.Instance;
                    serializer.Serialize(jsonTextWriter, value);
                    jsonTextWriter.Flush();
                    return stringWriter.GetStringBuilder().ToString();
                }
            }
        }

        public static T DeserializeJsonNetBuffers<T>(string value)
        {
            using (var stringReader = new StringReader(value))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    jsonTextReader.ArrayPool = CharArrayPool.Instance;
                    var result = serializer.Deserialize<T>(jsonTextReader);
                    return result;
                }
            }
        }
    }

    class CharArrayPool : IArrayPool<char>
    {
        public static readonly CharArrayPool Instance = new CharArrayPool();

        public char[] Rent(int minimumLength) => ArrayPool<char>.Shared.Rent(minimumLength);

        public void Return(char[] array)
        {
            ArrayPool<char>.Shared.Return(array);
        }
    }
}
