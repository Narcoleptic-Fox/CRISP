namespace CRISP.Core.Exceptions
{
    public sealed class NotFoundException : KeyNotFoundException
    {
        public NotFoundException() { }
        public NotFoundException(object key, string entityName)
            : base($"Entity '{entityName}' with key '{key}' was not found.")
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }
        public NotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public NotFoundException(string message, IFormatProvider formatProvider, params object[] args) : base(string.Format(formatProvider, message, args))
        {
        }
        public NotFoundException(string message, IFormatProvider formatProvider, Exception innerException, params object[] args) : base(string.Format(formatProvider, message, args), innerException)
        {
        }
    }
}
