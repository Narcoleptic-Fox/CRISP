namespace CRISP.Core.Exceptions
{
    public sealed class DomainException : Exception
    {
        public DomainException() { }
        public DomainException(string message) : base(message)
        {
        }
        public DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public DomainException(string message, IFormatProvider formatProvider, params object[] args) : base(string.Format(formatProvider, message, args))
        {
        }
        public DomainException(string message, IFormatProvider formatProvider, Exception innerException, params object[] args) : base(string.Format(formatProvider, message, args), innerException)
        {
        }
    }
}
