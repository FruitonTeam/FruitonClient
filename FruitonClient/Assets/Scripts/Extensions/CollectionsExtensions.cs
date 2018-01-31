using Google.Protobuf.Collections;

public static class CollectionsExtensions
{
    public static RepeatedField<T> Copy<T>(this RepeatedField<T> pattern)
    {
        var result = new RepeatedField<T>();
        foreach(T item in pattern)
        {
            result.Add(item);
        }
        return result;
    }
}
