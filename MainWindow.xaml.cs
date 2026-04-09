public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm;
    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainWindowViewModel();
        DataContext = _vm;
        Closed += async (s, e) => await new JsonDataService().SaveAsync(_vm.Contacts);
    }
    private void Exit_Click(object sender, RoutedEventArgs e) => Close();
}
