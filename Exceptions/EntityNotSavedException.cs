namespace CardGeneratorBackend.Exceptions
{
    public class EntityNotSavedException(object notSavedData, string message) : Exception(message)
    {
        public object NotSavedData { get; } = notSavedData;
    }
}
