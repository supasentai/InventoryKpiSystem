using System;
using System.IO;
using System.Text.Json;

public class JsonLoader
{
    public T Load<T>(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json);
    }
}