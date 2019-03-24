using Xunit;

namespace HttpRecorderTests
{
    [CollectionDefinition(ServerCollection.Name)]
    public class ServerCollection : ICollectionFixture<ServerFixture>
    {
        public const string Name = "Server";
    }
}
