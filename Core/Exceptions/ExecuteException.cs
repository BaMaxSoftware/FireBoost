using System;

namespace Core.Exceptions
{
    public class ExecuteException : Exception
    {
        public int ReturnValue{ get; private set; }
        private readonly string _message;
        public override string Message
        {
            get { return _message; }
        }
        public ExecuteException(int returnValue) : base()
        {
            ReturnValue = returnValue;
        }

        public ExecuteException(int returnValue, string message) : base(message)
        {
            ReturnValue = returnValue;
            _message = message;
        }
    }
}
