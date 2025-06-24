using CardGeneratorBackend.Entities;

namespace CardGeneratorBackend.FileManagement
{
    public interface IFileIOHandlerFactory
    {
        public IFileIOHandler GetIOHandler(TrackedFile file);
    }
}
