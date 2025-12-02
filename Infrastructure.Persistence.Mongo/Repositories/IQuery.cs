using MongoDB.Driver;

namespace Infrastructure.Persistence.Mongo.Repositories;

public interface IQuery<TDocument>
{
    IFindFluent<TDocument, TDocument> Query(bool withDeleted = false);
}
