using MongoDB.Bson.Serialization.Conventions;

namespace Fcg.Orders.Infrastructure.Persistence.Mongo
{
    public static class MongoConventions
    {
        private static bool _registered;

        public static void EnsureRegistered()
        {
            if (_registered) return;

            var pack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("camelCase", pack, t => true);
            _registered = true;
        }
    }
}
