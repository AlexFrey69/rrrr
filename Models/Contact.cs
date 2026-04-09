public class Contact : INotifyPropertyChanged
{
    private int _id;
    private static int _nextId = 1;
    private string _fullName, _phoneNumber, _email, _category;

    public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }
    public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }
    public string PhoneNumber { get => _phoneNumber; set { _phoneNumber = value; OnPropertyChanged(); } }
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public string Category { get => _category; set { _category = value; OnPropertyChanged(); } }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Contact() { Id = _nextId++; }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
