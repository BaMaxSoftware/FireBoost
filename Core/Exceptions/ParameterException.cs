using Core.Properties;

namespace Core.Exceptions
{
    public class ParameterException : ExecuteException
    {
        public ParameterException(string elementName, string parameterId)
            : base(-1, ParameterMessage(elementName, parameterId)) { }
        private static string ParameterMessage(string elementName, string parameterId) => string.Format(Resources.CanntGetParameter0FromObject1, parameterId, elementName);
    }
}
