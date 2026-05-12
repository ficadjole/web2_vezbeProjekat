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
       [DataMember] public Book Book { get; set; } = new Book();
       [DataMember] public int UserId { get; set; }

        [DataMember] public string Email { get; set; } = string.Empty;
    }
}
