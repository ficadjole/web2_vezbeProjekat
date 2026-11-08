using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class BuyBookRequestDTO
    {
       [DataMember] 
        public string UserId { get; set; } = string.Empty;

        [DataMember] 
        public string BookId { get; set; } = string.Empty;

        [DataMember]
        public int Quantity { get; set; } = 1;
    }
}
