using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class ReservationDTO
    {
        [DataMember] public int Quantity { get; set; }
    }
}
