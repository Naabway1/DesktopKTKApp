using System.Windows.Controls;

namespace DesktopKTKApp.View
{
    /// <summary>
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Role.SelectedIndex == 1) 
            {
                Group.IsEnabled = false;
            }
            else
            {
                Group.IsEnabled = true;
            }
        }
    }
}
