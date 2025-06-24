namespace CardGeneratorBackend.Exceptions
{
    public class EntityNotFoundException(Type type, object id) : Exception($"Unable to find {type.Name} with ID = {id}")
    {
        public object Id { get; } = id;
    }
}
