using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Core.Persistence.Mongo.Repositories;

public interface IQuery<TDocument> 
{
    IQueryable<TDocument> Query(bool withDeleted = false);
}
