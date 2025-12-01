using MongoDB.Driver;

namespace Core.Persistence.Mongo.Repositories;

public interface IQuery<TDocument>
{
    IFindFluent<TDocument, TDocument> Query(bool withDeleted = false);
}
