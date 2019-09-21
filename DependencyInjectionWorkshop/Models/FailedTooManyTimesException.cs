using System;
using System.Runtime.Serialization;

namespace DependencyInjectionWorkshop.Models
{
    [Serializable]
    public class FailedTooManyTimesException : Exception
    {
        public FailedTooManyTimesException()
        {
        }

        public FailedTooManyTimesException(string message) : base(message)
        {
        }

        public FailedTooManyTimesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FailedTooManyTimesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}