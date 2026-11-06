namespace SmartCv.Factories;

public static class OrderRules
{
    public static bool IsCompleteOrder(IReadOnlyCollection<int> requested, IEnumerable<int> existing)
    {
        var requestedSet = requested.ToHashSet();
        return requestedSet.Count == requested.Count && requestedSet.SetEquals(existing);
    }
}
