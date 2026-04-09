public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IDataService _dataService = new JsonDataService();
    public ObservableCollection<Contact> Contacts { get; set; } = new();
    public ICollectionView FilteredContacts { get; private set; }

    private Contact _selectedContact;
    public Contact SelectedContact { get => _selectedContact; set { _selectedContact = value; OnPropertyChanged(); } }

    private Contact _currentContact = new();
    public Contact CurrentContact { get => _currentContact; set { _currentContact = value; OnPropertyChanged(); } }

    private string _searchText = "";
    public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

    private string _selectedCategoryFilter = "Все";
    public string SelectedCategoryFilter
    {
        get => _selectedCategoryFilter;
        set { _selectedCategoryFilter = value; OnPropertyChanged(); ApplyFilter(); }
    }

    public List<string> Categories { get; } = new() { "Все", "Семья", "Работа", "Друзья", "Другое" };
    private string _statusMessage;
    public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }

    public ICommand AddCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ExportCommand { get; }

    public MainWindowViewModel()
    {
        FilteredContacts = CollectionViewSource.GetDefaultView(Contacts);
        FilteredContacts.Filter = FilterPredicate;
        AddCommand = new RelayCommand(_ => AddContact());
        SaveCommand = new RelayCommand(_ => SaveContact(), _ => IsContactValid());
        DeleteCommand = new RelayCommand(_ => DeleteContact(), _ => SelectedContact != null);
        CancelCommand = new RelayCommand(_ => CancelEdit());
        ExportCommand = new RelayCommand(_ => ExportToTxt());
        LoadData();
    }

    private async void LoadData()
    {
        var list = await _dataService.LoadAsync();
        Contacts.Clear();
        foreach (var c in list) Contacts.Add(c);
        StatusMessage = $"Загружено {Contacts.Count} контактов";
    }

    private bool FilterPredicate(object obj)
    {
        if (obj is not Contact c) return false;
        bool matchesSearch = string.IsNullOrWhiteSpace(SearchText) ||
            c.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
            c.PhoneNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        bool matchesCategory = SelectedCategoryFilter == "Все" || c.Category == SelectedCategoryFilter;
        return matchesSearch && matchesCategory;
    }

    private void ApplyFilter() => FilteredContacts.Refresh();

    private void AddContact()
    {
        CurrentContact = new Contact { Category = "Другое" };
        CommandManager.InvalidateRequerySuggested();
    }

    private async void SaveContact()
    {
        if (Contacts.Any(c => c.Id == CurrentContact.Id))
        {
            var existing = Contacts.First(c => c.Id == CurrentContact.Id);
            existing.FullName = CurrentContact.FullName;
            existing.PhoneNumber = CurrentContact.PhoneNumber;
            existing.Email = CurrentContact.Email;
            existing.Category = CurrentContact.Category;
        }
        else Contacts.Add(CurrentContact);
        await _dataService.SaveAsync(Contacts);
        CancelEdit();
        StatusMessage = "Сохранено";
    }

    private async void DeleteContact()
    {
        if (SelectedContact == null) return;
        if (MessageBox.Show("Удалить контакт?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            Contacts.Remove(SelectedContact);
            await _dataService.SaveAsync(Contacts);
            StatusMessage = "Контакт удалён";
        }
    }

    private void CancelEdit() => CurrentContact = new Contact { Category = "Другое" };

    private bool IsContactValid()
    {
        if (string.IsNullOrWhiteSpace(CurrentContact.FullName) || CurrentContact.FullName.Length < 2) return false;
        var digits = new string(CurrentContact.PhoneNumber.Where(char.IsDigit).ToArray());
        if (digits.Length < 10 || digits.Length > 11) return false;
        if (!string.IsNullOrEmpty(CurrentContact.Email) && !CurrentContact.Email.Contains('@')) return false;
        return true;
    }

    private void ExportToTxt()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "export.txt");
        File.WriteAllLines(path, Contacts.Select(c => $"{c.FullName} - {c.PhoneNumber}"));
        StatusMessage = $"Экспортировано в {path}";
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
