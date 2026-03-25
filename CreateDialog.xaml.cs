using System.Windows;

namespace RestorePoint
{
    public partial class CreateDialog : Window
    {
        public string RestorePointName { get; private set; } = "System Restore Point";

        public CreateDialog()
        {
            InitializeComponent();
            NameInput.SelectAll();
            NameInput.Focus();
        }

        private void CreateClick(object sender, RoutedEventArgs e)
        {
            string name = NameInput.Text.Trim();
            if (string.IsNullOrEmpty(name)) name = "System Restore Point";
            RestorePointName = name;
            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
