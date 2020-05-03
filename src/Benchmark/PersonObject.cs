namespace Benchmark
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [ProtoContract]
    public partial class PersonObject
    {
        [ProtoMember(1)]
        public string FullName { get; set; }

        [ProtoMember(2)]
        public DateTime Birthday { get; set; }

        [ProtoMember(3)]
        public Dictionary<string, string> Tags { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var comparer = obj as PersonObject;
            if (comparer == null)
            {
                return false;
            }

            return comparer.FullName == this.FullName &&
                   comparer.Birthday == this.Birthday &&
                   comparer.Tags.SequenceEqual(this.Tags);
        }
    }
}