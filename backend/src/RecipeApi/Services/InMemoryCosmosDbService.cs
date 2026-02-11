using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RecipeApi.Services;

public class InMemoryCosmosDbService : ICosmosDbService
{
    private readonly ConcurrentDictionary<string, object> _store = new();

    public Task<T> CreateItemAsync<T>(T item, string partitionKey)
    {
        var id = GetId(item) ?? Guid.NewGuid().ToString();
        _store[id] = item! as object ?? item!;
        return Task.FromResult(item);
    }

    public Task<T?> GetItemAsync<T>(string id, string partitionKey)
    {
        if (_store.TryGetValue(id, out var obj) && obj is T t)
        {
            return Task.FromResult<T?>(t);
        }
        return Task.FromResult<T?>(default);
    }

    public Task<List<T>> QueryItemsAsync<T>(string query, Dictionary<string, object>? parameters = null)
    {
        // Enhanced implementation: parse basic SQL queries for better test emulation
        var results = _store.Values.OfType<T>().ToList();

        // Parse WHERE clause
        var whereMatch = Regex.Match(query, @"WHERE\s+(.+?)(?:\s+ORDER\s+BY|\s*$)", RegexOptions.IgnoreCase);
        if (whereMatch.Success)
        {
            var whereClause = whereMatch.Groups[1].Value;
            results = FilterByWhereClause(results, whereClause, parameters);
        }

        // Parse ORDER BY clause
        var orderByMatch = Regex.Match(query, @"ORDER\s+BY\s+c\.(\w+)(?:\s+(ASC|DESC))?", RegexOptions.IgnoreCase);
        if (orderByMatch.Success)
        {
            var propertyName = orderByMatch.Groups[1].Value;
            var direction = orderByMatch.Groups[2].Value.ToUpperInvariant();
            results = OrderByProperty(results, propertyName, direction == "DESC");
        }

        return Task.FromResult(results);
    }

    public Task<T> UpdateItemAsync<T>(T item, string id, string partitionKey)
    {
        _store[id] = item! as object ?? item!;
        return Task.FromResult(item);
    }

    public Task DeleteItemAsync(string id, string partitionKey)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    private List<T> FilterByWhereClause<T>(List<T> items, string whereClause, Dictionary<string, object>? parameters)
    {
        // Split by AND (basic support)
        var conditions = whereClause.Split(new[] { " AND ", " and " }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var condition in conditions)
        {
            var trimmedCondition = condition.Trim();
            
            // Match patterns like: c.propertyName = 'value' or c.propertyName = @paramName
            var match = Regex.Match(trimmedCondition, @"c\.(\w+)\s*=\s*(.+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var propertyName = match.Groups[1].Value;
                var valueExpr = match.Groups[2].Value.Trim();

                object? targetValue = null;

                // Check if it's a parameter reference
                if (valueExpr.StartsWith("@"))
                {
                    var paramName = valueExpr;
                    if (parameters != null && parameters.TryGetValue(paramName, out var paramValue))
                    {
                        targetValue = paramValue;
                    }
                }
                // Check if it's a string literal
                else if (valueExpr.StartsWith("'") && valueExpr.EndsWith("'"))
                {
                    targetValue = valueExpr.Trim('\'');
                }
                // Check if it's a number
                else if (int.TryParse(valueExpr, out var intValue))
                {
                    targetValue = intValue;
                }

                if (targetValue != null)
                {
                    items = items.Where(item => PropertyEquals(item, propertyName, targetValue)).ToList();
                }
            }
        }

        return items;
    }

    private bool PropertyEquals<T>(T item, string propertyName, object targetValue)
    {
        if (item == null) return false;

        var prop = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop == null) return false;

        var actualValue = prop.GetValue(item);
        if (actualValue == null) return targetValue == null;

        return actualValue.ToString() == targetValue.ToString();
    }

    private List<T> OrderByProperty<T>(List<T> items, string propertyName, bool descending)
    {
        if (items.Count == 0) return items;

        var prop = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop == null) return items;

        if (descending)
        {
            return items.OrderByDescending(item => prop.GetValue(item)).ToList();
        }
        else
        {
            return items.OrderBy(item => prop.GetValue(item)).ToList();
        }
    }

    private string? GetId<T>(T item)
    {
        if (item == null) return null;
        var prop = item!.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (prop != null)
        {
            var val = prop.GetValue(item);
            return val?.ToString();
        }
        return null;
    }
}
