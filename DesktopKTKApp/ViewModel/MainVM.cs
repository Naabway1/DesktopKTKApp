using DesktopKTKApp.Model;
using DesktopKTKApp.View;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Data.Sql;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Threading.Tasks;

namespace DesktopKTKApp.ViewModel
{
    public class MainVM : ObservableObject
    {
        private readonly NavigationVM _navVM;
        private SqlConnection KTKdb;
        private int _roleID;
        private string _selectedRole;
        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (_selectedRole != value)
                {
                    _selectedRole = value;
                    OnPropertyChanged(nameof(SelectedRole));

                    IsGroupsComboBoxEnabled = (_selectedRole.ToString() == "Студент");
                    if (_selectedRole.ToString() == "Студент") { _roleID = 1; } else if (_selectedRole.ToString() == "Преподаватель") { _roleID = 2; }
                }
            }
        }
        private string _selectedGroup;
        public string SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (_selectedGroup != value)
                {
                    _selectedGroup = value;
                    OnPropertyChanged(nameof(SelectedGroup));
                }
            }
        }
        private bool _isGroupsComboBoxEnabled;
        public bool IsGroupsComboBoxEnabled
        {
            get => _isGroupsComboBoxEnabled;
            set
            {
                if (_isGroupsComboBoxEnabled != value)
                {
                    _isGroupsComboBoxEnabled = value;
                    OnPropertyChanged(nameof(IsGroupsComboBoxEnabled));
                }
            }
        }
        public ObservableCollection<string> Roles { get; private set; }
        public ObservableCollection<string> Groups { get; private set; }

        private string _login;
        public string Login
        {
            get => _login;
            set
            {
                if (value != _login)
                {
                    _login = value;
                }
                OnPropertyChanged(nameof(Login));
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (value != _password)
                {
                    _password = value;
                }
                OnPropertyChanged(nameof(Password));
            }
        }

        private string _passwordToCheck;
        public string PasswordToCheck
        {
            get => _passwordToCheck;
            set
            {
                if (value != _passwordToCheck)
                {
                    _passwordToCheck = value;
                }
                OnPropertyChanged(nameof(PasswordToCheck));
            }
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set
            {
                if (value != _fullName)
                {
                    _fullName = value;
                }
                OnPropertyChanged(nameof(FullName));
            }
        }

        public ICommand Navigate { get; private set; }
        public ICommand AuthCommand { get; private set; }
        public ICommand RegistrationCommand { get; private set; }

        public MainVM(NavigationVM vm)
        {
            _navVM = vm; // объявление переменных
            Roles = new ObservableCollection<string>();
            Groups = new ObservableCollection<string>();

            Navigate = new RelayCommand(NavigateCommandExecuted); // команды для кнопок
            AuthCommand = new RelayCommand(_ => CheckAuthData());
            RegistrationCommand = new RelayCommand(_ => Registration());

            KTKdb = new SqlConnection("Server=10.14.206.27;Database=user5;User Id=user5;Password=Lu%5%4e4");
            KTKdb.Open();

            var cmdForRoles = new SqlCommand()
            {
                Connection = KTKdb,
                CommandText = "SELECT RoleName FROM Roles WHERE RoleName != 'Администратор'"
            }; //Вытягивание данных о ролях (препод/студентик)
            var readerForRoles = cmdForRoles.ExecuteReader();
            while (readerForRoles.Read())
            {
                object param = readerForRoles.GetValue(0);
                Roles.Add(param.ToString());
            }
            readerForRoles.Close();

            var cmdForGroups = new SqlCommand()
            {
                Connection = KTKdb,
                CommandText = "Select GroupName from Groups"
            }; //Вытягивание данных о группах
            var readerForGroups = cmdForGroups.ExecuteReader();
            while (readerForGroups.Read())
            {
                object param = readerForGroups.GetValue(0);
                Groups.Add(param.ToString());
            }
            readerForGroups.Close();
        }

        private void CheckAuthData()
        {
            SqlCommand sql = new SqlCommand()
            {
                CommandText = $"SELECT * FROM Users WHERE Login = \'{_login}\' and Password = \'{_password}\'",
                Connection = KTKdb
            };
            SqlDataReader sqlDataReader = sql.ExecuteReader();
            try
            {
                if (sqlDataReader.Read() == true)
                {
                    _navVM.NavigateTo("MainPage", this);
                    sqlDataReader.Close();
                }
                else
                {
                    Debug.WriteLine("А пароль то неправильный, XD");
                    sqlDataReader.Close();
                }
            }
            catch { return; }
        }
        private async void Registration()
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = KTKdb,
                CommandText = $"INSERT INTO Users (FullName, Login, Password, RoleID) VALUES (\'{_fullName}\', \'{_login}\', \'{_password}\', \'{_roleID}\')"
            };
            SqlCommand cmdForCheckLogins = new SqlCommand()
            {
                Connection = KTKdb,
                CommandText = $"SELECT Login FROM Users WHERE Login = \'{_login}\'"
            };
            SqlDataReader readerForCheckLogins = cmdForCheckLogins.ExecuteReader();
            if (readerForCheckLogins.Read() == false)
            {
                readerForCheckLogins.Close();
                if (_password == _passwordToCheck)
                {
                    if (_selectedRole.ToString() == "Студент") //добавление в таблицу Студентов
                    {
                        await Task.Run(() => cmd.ExecuteNonQueryAsync());
                        RegStudent();
                    }
                    else if (_selectedRole.ToString() == "Преподаватель") //добавление в таблицу Преподаватели
                    {
                        await Task.Run(() => cmd.ExecuteNonQueryAsync());
                        RegTeacher();
                    }
                    _navVM.NavigateTo("AuthPage", this);
                }
                else
                {
                    MessageBox.Show("Пароли не совпадают!");
                }
            }
            else
            {
                MessageBox.Show("Пользователь с таким логином уже существует! Придумайте другой!");
            }
        }

        private void RegStudent()
        {
            SqlCommand checkUserTable = new SqlCommand()
            {
                Connection = KTKdb,
                CommandText = $"SELECT UserID FROM Users WHERE Login = \'{_login}\'"
            };
            SqlDataReader readerUserTable = checkUserTable.ExecuteReader();
            int userID = int.Parse(readerUserTable.GetValue(0).ToString());
            readerUserTable.Close(); //получение юзер айди для внесения в таблицу студентов

            SqlCommand checkGroupTable = new SqlCommand()
            {
                Connection = KTKdb,
                CommandText = $"Select GroupID from Groups where GroupName = \'{_selectedGroup}\'"
            };
            SqlDataReader readerGroupTable = checkGroupTable.ExecuteReader();
            int groupID = int.Parse(readerGroupTable.GetValue(0).ToString());

            SqlCommand writeIntoStudentTable = new SqlCommand()
            {
                Connection = KTKdb,
                CommandText = $"INSERT INTO Students (UserID, GroupID) VALUES ({userID}, {groupID})"
            };
            readerGroupTable.Close(); //получение группы айди для внесения в таблицу студентов
            writeIntoStudentTable.ExecuteNonQuery();
        }
        private void RegTeacher()
        {
            SqlCommand checkUserTable = new SqlCommand()
            {
                Connection = KTKdb,
                CommandText = $"SELECT UserID FROM Users WHERE Login = \'{_login}\'"
            };
            SqlDataReader readerUserTable = checkUserTable.ExecuteReader();
            if (readerUserTable.Read() == true)
            {
                int userID = int.Parse(readerUserTable.GetValue(0).ToString());
                readerUserTable.Close(); //получение юзер айди для внесения в таблицу преподавателей
                SqlCommand writeIntoTeachersTable = new SqlCommand()
                {
                    Connection = KTKdb,
                    CommandText = $"INSERT INTO Teachers (UserID) VALUES ({userID})"
                };
                writeIntoTeachersTable.ExecuteNonQuery(); //запись в таблицу преподавателей
            }
        }

        private void NavigateCommandExecuted(object param)
        {
            _navVM.NavigateTo(param?.ToString(), this);
        }
    }

    public class NavigationVM
    {
        private readonly Frame _frame;

        public NavigationVM(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo(object param, object dataContext)
        {
            string pageName = param.ToString();

            Page page;
            switch (pageName)
            {
                case "AuthPage":
                    page = new AuthPage();
                    break;
                case "MainPage":
                    page = new MainPage();
                    break;
                case "RegistrationPage":
                    page = new RegistrationPage();
                    break;
                default:
                    throw new ArgumentException("Неизвестная страница ", nameof(pageName));
            }
            page.DataContext = dataContext;
            _frame.Navigate(page);

            Debug.WriteLine("Команда выполнена");
        }
    }
}
