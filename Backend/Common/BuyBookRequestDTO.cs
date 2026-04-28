using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class BuyBookRequestDTO
    {
        public Book Book { get; set; } = new Book();
        public int UserId { get; set; }
    }
}
