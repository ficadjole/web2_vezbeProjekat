using Common;
using Xunit;

namespace Tests
{

    public class PartitionKeyHelperTests
    {

        private static int PartitionIndex(long partitionKey, int partitionCount)
        {
            ulong offset = unchecked((ulong)(partitionKey - long.MinValue));
            return (int)(offset / (ulong.MaxValue / (ulong)partitionCount + 1));
        }

        [Fact]
        public void Raspodela_kljuceva_je_ravnomerna()
        {
            const int n = 10_000, partitionCount = 4;
            var counts = new int[partitionCount];
            var rnd = new Random(42);

            for (int i = 0; i < n; i++)
            {
                string isbn = $"978-{rnd.Next(10, 99)}-{rnd.Next(100, 999)}-{rnd.Next(1000, 9999)}-{rnd.Next(0, 9)}";
                counts[PartitionIndex(PartitionKeyHelper.ForBook(isbn), partitionCount)]++;
            }

            Assert.All(counts, c => Assert.InRange(c, 2250, 2750));
        }
    }
}

