public interface IDataService
{
    Task<List<Contact>> LoadAsync();
    Task SaveAsync(IEnumerable<Contact> contacts);
}

public class JsonDataService : IDataService
{
    private readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "contacts.json");
    public async Task<List<Contact>> LoadAsync()
    {
        if (!File.Exists(_path)) return new List<Contact>();
        var json = await File.ReadAllTextAsync(_path);
        return JsonSerializer.Deserialize<List<Contact>>(json) ?? new List<Contact>();
    }
    public async Task SaveAsync(IEnumerable<Contact> contacts)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(contacts, options);
        await File.WriteAllTextAsync(_path, json);
    }
}
