
namespace LibraryStatefulService
{
    [Serializable]
    internal class NoReservationException : Exception
    {
        public NoReservationException()
        {
        }

        public NoReservationException(string? message) : base(message)
        {
        }

        public NoReservationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}