using Infrastructure.Persistence.Mongo.Dynamic;
using Infrastructure.Persistence.Mongo.Paging;
using Infrastructure.Persistence.Mongo.Repositories;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Mongo.Repositories;

public interface IMongoRepository<TDocument> : IQuery<TDocument>
    where TDocument : BaseDocument
{
    TDocument Get(
        Expression<Func<TDocument, bool>> predicate,
        bool withDeleted = false
    );

    Paginate<TDocument> GetList(
        Expression<Func<TDocument, bool>>? predicate = null,
        Func<SortDefinitionBuilder<TDocument>, SortDefinition<TDocument>>? orderBy = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false
    );

    Paginate<TDocument> GetListByDynamic(
        DynamicQuery dynamicQuery,
        Expression<Func<TDocument, bool>>? predicate = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false
    );

    bool Any(Expression<Func<TDocument, bool>>? predicate = null, bool withDeleted = false);
}
