using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class DiagnosticResultDto
    {
        [DataMember]
        public string ServiceName { get; set; } = string.Empty;
        [DataMember]
        public string PartitionId { get; set; } = string.Empty;
        [DataMember]
        public long PartitionKey { get; set; }
        [DataMember]
        public long LowKey { get; set; }
        [DataMember]
        public long GetCount { get; set; }
    }
}
