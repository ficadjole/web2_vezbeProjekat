using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public record ConsistencyCheckDTO
    {
        [DataMember] public int Quantity { get; init; }
        [DataMember] public int Reserved {get; init; }
        [DataMember] public int nmbOfReservations { get; init; }

        [DataMember] public int ReservationsQuantitySum { get; init; }

        [DataMember] public bool IsConsistent { get; init; }
    }
}
