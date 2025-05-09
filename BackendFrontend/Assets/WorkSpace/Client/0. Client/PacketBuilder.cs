using System.Collections.Generic;

public static class PacketBuilder
{
    public static Dictionary<string, string> CreateBody(params (string key, string value)[] items)
    {
        var dict = new Dictionary<string, string>();
        foreach (var (k, v) in items) dict[k] = v;
        return dict;
    }
}
