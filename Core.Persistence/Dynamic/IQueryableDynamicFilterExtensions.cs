using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Linq.Dynamic.Core;
using System.Text;

namespace Core.Persistence.Dynamic;

public static class IQueryableDynamicFilterExtensions
{
    private static readonly string[] _orders = { "asc", "desc" };
    private static readonly string[] _logics = { "and", "or" };
    private static readonly ConcurrentDictionary<string, string> _cache = new();

    private static readonly IDictionary<string, string> _operators = new Dictionary<string, string>
    {
        { "eq", "=" },
        { "neq", "!=" },
        { "lt", "<" },
        { "lte", "<=" },
        { "gt", ">" },
        { "gte", ">=" },
        { "isnull", "== null" },
        { "isnotnull", "!= null" },
        { "startswith", "StartsWith" },
        { "endswith", "EndsWith" },
        { "contains", "Contains" },
        { "doesnotcontain", "Contains" }
    };

    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamicQuery)
    {
        if (dynamicQuery.Filter is not null)
            query = Filter(query, dynamicQuery.Filter);
        if (dynamicQuery.Sorts is not null && dynamicQuery.Sorts.Any())
            query = Sort(query, dynamicQuery.Sorts);
        return query;
    }

    private static IQueryable<T> Sort<T>(IQueryable<T> query, IEnumerable<Sort> sorts)
    {
        foreach (Sort sort in sorts)
        {
            if (string.IsNullOrWhiteSpace(sort.Field))
                throw new ArgumentException("Invalid Field");
            if (string.IsNullOrWhiteSpace(sort.Direction) || !_orders.Contains(sort.Direction))
                throw new ArgumentException("Invalid Order Type");
        }

        if (sorts.Any())
        {
            string ordering = string.Join(separator: ",", values: sorts.Select(s => $"{s.Field} {s.Direction}"));
            return query.OrderBy(ordering);
        }

        return query;
    }

    private static IQueryable<T> Filter<T>(IQueryable<T> query, Filter filter)
    {
        IList<Filter> filters = GetAllFilters(filter);
        string?[] values = filters.Select(f => f.Value).ToArray();
        string where = Transform<T>(filter, filters);
        if (!string.IsNullOrWhiteSpace(where) && values != null)
            query = query.Where(where, values);
        return query;
    }

    private static string Transform<T>(Filter filter, IList<Filter> filters)
    {
        string cacheKey = JsonConvert.SerializeObject(filter);
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        if (!typeof(T).GetProperties().Any(p => p.Name.Equals(filter.Field, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Invalid field name: {filter.Field}");

        if (string.IsNullOrWhiteSpace(filter.Operator) || !_operators.ContainsKey(filter.Operator))
            throw new ArgumentException("Invalid Operator");

        int index = filters.IndexOf(filter);
        string comparison = _operators[filter.Operator];
        StringBuilder where = new();

        if (!string.IsNullOrWhiteSpace(filter.Value))
        {
            if (filter.Operator == "doesnotcontain")
                where.Append($"(!np({filter.Field}).{comparison}(@{index.ToString()}))");
            else if (comparison is "StartsWith" or "EndsWith" or "Contains")
                where.Append($"(np({filter.Field}).{comparison}(@{index.ToString()}))");
            else
                where.Append($"np({filter.Field}) {comparison} @{index.ToString()}");
        }
        else if (filter.Operator is "isnull" or "isnotnull")
        {
            where.Append($"np({filter.Field}) {comparison}");
        }

        if (filter.Logic is not null && filter.Filters is not null && filter.Filters.Any())
        {
            if (!_logics.Contains(filter.Logic))
                throw new ArgumentException("Invalid Logic");

            string logicExpr = string.Join(
                $" {filter.Logic} ",
                filter.Filters.Select(f => Transform<T>(f, filters))
            );

            where.Append($" {filter.Logic} ({logicExpr})");
        }

        _cache[cacheKey] = where.ToString();
        return where.ToString();
    }

    private static IList<Filter> GetAllFilters(Filter filter)
    {
        List<Filter> filters = new();
        GetFilters(filter, filters);
        return filters;
    }

    private static void GetFilters(Filter filter, List<Filter> filters)
    {
        filters.Add(filter);
        if (filter.Filters is not null && filter.Filters.Any())
            foreach (Filter item in filter.Filters)
                GetFilters(item, filters);
    }
}
