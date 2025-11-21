using MongoDB.Bson;
using MongoDB.Driver;

namespace Core.Persistence.Mongo.Dynamic;

public static class MongoDynamicFilterExtensions
{
    private static readonly string[] _orders = { "asc", "desc" };
    private static readonly string[] _logics = { "and", "or" };

    private static Dictionary<string, Func<string, string, FilterDefinitionBuilder<TDocument>, FilterDefinition<TDocument>>> BuildOperatorMap<TDocument>()
    {
        return new Dictionary<string, Func<string, string, FilterDefinitionBuilder<TDocument>, FilterDefinition<TDocument>>>
        {
            { "eq", (field, value, b) => b.Eq(field, BsonValue.Create(value)) },
            { "neq", (field, value, b) => b.Ne(field, BsonValue.Create(value)) },
            { "gt", (field, value, b) => b.Gt(field, BsonValue.Create(value)) },
            { "gte", (field, value, b) => b.Gte(field, BsonValue.Create(value)) },
            { "lt", (field, value, b) => b.Lt(field, BsonValue.Create(value)) },
            { "lte", (field, value, b) => b.Lte(field, BsonValue.Create(value)) },

            { "contains", (field, value, b) => b.Regex(field, new BsonRegularExpression(value, "i")) },
            { "startswith", (field, value, b) => b.Regex(field, new BsonRegularExpression("^" + value, "i")) },
            { "endswith", (field, value, b) => b.Regex(field, new BsonRegularExpression(value + "$", "i")) },

            { "isnull", (field, _, b) => b.Eq(field, BsonNull.Value) },
            { "isnotnull", (field, _, b) => b.Ne(field, BsonNull.Value) },
        };
    }

    public static IFindFluent<TDocument, TDocument> ToDynamic<TDocument>(this IMongoCollection<TDocument> collection, DynamicQuery dynamicQuery)
    {
        FilterDefinition<TDocument> filter = BuildFilter<TDocument>(dynamicQuery.Filter);
        IFindFluent<TDocument, TDocument> fluent = collection.Find(filter);

        if (dynamicQuery.Sorts is not null)
            fluent = ApplySorting<TDocument>(fluent, dynamicQuery.Sorts);

        return fluent;
    }

    private static FilterDefinition<TDocument> BuildFilter<TDocument>(Filter? filter)
    {
        if (filter == null)
            return Builders<TDocument>.Filter.Empty;

        if (filter.Logic is not null && !_logics.Contains(filter.Logic.ToLower()))
            throw new ArgumentException("Invalid Logic Type");

        FilterDefinitionBuilder<TDocument> builder = Builders<TDocument>.Filter;
        List<FilterDefinition<TDocument>> definitions = new List<FilterDefinition<TDocument>>();

        ApplyFilterRecursive(filter, builder, definitions);

        return filter.Logic?.ToLower() == "or"
            ? builder.Or(definitions)
            : builder.And(definitions);
    }

    private static void ApplyFilterRecursive<TDocument>(Filter filter, FilterDefinitionBuilder<TDocument> builder, List<FilterDefinition<TDocument>> definitions)
    {
        Dictionary<string, Func<string, string, FilterDefinitionBuilder<TDocument>, FilterDefinition<TDocument>>> operators =
            BuildOperatorMap<TDocument>();

        if (!operators.ContainsKey(filter.Operator))
            throw new ArgumentException("Invalid Operator");

        if (!string.IsNullOrWhiteSpace(filter.Value))
        {
            FilterDefinition<TDocument> definition =
                operators[filter.Operator](filter.Field, filter.Value, builder);

            definitions.Add(definition);
        }

        if (filter.Filters != null)
        {
            foreach (Filter child in filter.Filters)
                ApplyFilterRecursive(child, builder, definitions);
        }
    }

    private static IFindFluent<TDocument, TDocument> ApplySorting<TDocument>(IFindFluent<TDocument, TDocument> fluent, IEnumerable<Sort> sorts)
    {
        SortDefinitionBuilder<TDocument> sortBuilder = Builders<TDocument>.Sort;
        List<SortDefinition<TDocument>> sortDefinitions = new List<SortDefinition<TDocument>>();

        foreach (Sort item in sorts)
        {
            if (string.IsNullOrWhiteSpace(item.Field))
                throw new ArgumentException("Invalid Sort Field");

            if (!_orders.Contains(item.Direction.ToLower()))
                throw new ArgumentException("Invalid Order Type");

            SortDefinition<TDocument> next =
                item.Direction.ToLower() == "asc"
                    ? sortBuilder.Ascending(item.Field)
                    : sortBuilder.Descending(item.Field);

            sortDefinitions.Add(next);
        }

        if (sortDefinitions.Any())
        {
            SortDefinition<TDocument> combinedSort = sortBuilder.Combine(sortDefinitions);
            fluent = fluent.Sort(combinedSort);
        }

        return fluent;
    }
}