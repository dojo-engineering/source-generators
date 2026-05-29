using Dojo.Generators.Abstractions;

namespace Dojo.OpenApiGenerator.TestWebApi.Services
{
    [AutoInterface(Lifetime = AutoInterfaceLifetime.Singleton)]
    public partial class SingletonService
    {
        public int GetValue()
        {
            return 1;
        }
    }
}