using System;
using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class SessionMeta
    {
        [DataMember]
        public string TurbineId { get; set; }

        [DataMember]
        public string Operator { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string SourceFileName { get; set; }   

        [DataMember]
        public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

        [DataMember]
        public DateTime StartTime { get; set; } = DateTime.UtcNow; 
    }
}
