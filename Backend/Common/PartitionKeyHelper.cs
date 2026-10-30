using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class PartitionKeyHelper
    {
        private const ulong FnvOffset = 14695981039346656037UL;
        private const ulong FnvPrime = 1099511628211UL;

        public static long ForBook(string bookId) => Fnv1a64("book:" + bookId.Trim().ToUpperInvariant());
        public static long ForAccount(string userId) => Fnv1a64("account:" + userId.Trim());
        public static long ForTransaction(Guid transactionId) => Fnv1a64(transactionId.ToString("N"));

        public static long Fnv1a64(string value)
        {
            ulong hash = FnvOffset;
            foreach(byte b in Encoding.UTF8.GetBytes(value))
            {
                hash ^= b;
                hash *= FnvPrime;
            }

            return unchecked((long)Mix(hash));
        }

        private static ulong Mix(ulong hash)
        {
            hash ^= hash >> 33; 
            hash *= 0xff51afd7ed558ccdUL;

            hash ^= hash >> 33; 
            hash *= 0xc4ceb9fe1a85ec53UL;

            return hash ^ (hash >> 33);

        }
    }
}
