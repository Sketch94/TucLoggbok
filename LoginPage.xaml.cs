using System.ComponentModel;
using System.Configuration;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TucLoggbok
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Window
    {

        public LoginPage()
        {
            InitializeComponent();
        }

        private void UsernameTextBox_TextChanged(object sender, System.EventArgs e)
        {
            // Denna metod ändrar endast den första bokstaven till en versal och resten till gemener
            // ex; john -> John.
            TextBox textbox = (TextBox)sender;
            if (!string.IsNullOrEmpty(textbox.Text))
            {
                textbox.Text = textbox.Text[..1].ToUpper() + textbox.Text[1..].ToLower();
                textbox.SelectionStart = textbox.Text.Length;
            }
        }

        private void AdminTextBox_TextChanged(object sender, System.EventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (!string.IsNullOrEmpty(textbox.Text))
            {
                textbox.Text = textbox.Text[..1].ToUpper() + textbox.Text[1..].ToLower();
                textbox.SelectionStart = textbox.Text.Length;
            }
        }

        public void UpdateProfileImage(string imageSource, bool isAdmin)
        {
            if (isAdmin) // uppdaterar profilbilden om admin annars uppdatera user
            {
                AdminProfileImage.Source = new BitmapImage(new Uri(imageSource));
            }
            else
            {
                UserProfileImage.Source = new BitmapImage(new Uri(imageSource));
            }
        }

        private void AdminLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = AdminUsernameTextBox.Text.Trim();
            string password = AdminPassword.Password.Trim();


            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vänligen fyll i både användarnamn och lösenord.", "Inloggningsfel", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string query = "SELECT UserID, Password, UserType FROM Users WHERE Username = @Username";

            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();


                    if (reader.HasRows)
                    {
                        reader.Read();

                        string storedPassword = reader["Password"].ToString() ?? string.Empty.Trim();
                        int userID = (int)reader["UserID"];
                        string userType = reader["UserType"].ToString() ?? string.Empty.Trim();

                        if (BCrypt.Net.BCrypt.Verify(password, storedPassword))
                        {
                            if (userType == "Admin")
                            {
                                AdminPage adminPage = new AdminPage(userID, username);
                                adminPage.Show();
                                this.Close();

                            }
                            else
                            {
                                MessageBox.Show($"Obehörig åtkomst. Du har inte behörighet att logga in som admin med detta konto.\n\nKontot du försöker logga in med: {userType}", "Behörighetsfel", MessageBoxButton.OK, MessageBoxImage.Stop);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Fel användarnamn eller lösenord.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                    }
                    else
                    {
                        MessageBox.Show("Fel användarnamn eller lösenord.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fel vid inloggning: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UserLogin_Click(object sender, RoutedEventArgs e)
        {
            

            string username = UsernameTextBox.Text.Trim();
            string password = UserPassword.Password.Trim();


            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vänligen fyll i både användarnamn och lösenord.", "Inloggningsfel", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string query = "SELECT UserID, Password, UserType FROM Users WHERE Username = @Username";

            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {

                        reader.Read();

                        string storedPassword = reader["Password"].ToString() ?? string.Empty.Trim();
                        int userID = (int)reader["UserID"];
                        string userType = reader["UserType"].ToString() ?? string.Empty.Trim();

                        if (BCrypt.Net.BCrypt.Verify(password, storedPassword))
                        {

                            if (userType == "User")
                            {
                                UserPage userPage = new UserPage(userID, username);
                                userPage.Show();
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show($"Obehörig åtkomst. Du har inte behörighet att logga in som vanlig användare med detta konto.\n\nKontot du försöker logga in med: {userType}", "Behörighetsfel", MessageBoxButton.OK, MessageBoxImage.Stop);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Fel användarnamn eller lösenord.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Fel användarnamn eller lösenord.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fel vid inloggning: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Userpassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string username = UsernameTextBox.Text.Trim();
                string enteredPassword = UserPassword.Password;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(enteredPassword))
                {
                    MessageBox.Show("Användarnamn och lösenord kan inte vara tomma", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string query = "SELECT Password FROM Users WHERE Username = @Username";

                using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", username);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            reader.Read();
                            string storedPassword = reader["Password"].ToString() ?? string.Empty.Trim(); ;


                            if (BCrypt.Net.BCrypt.Verify(enteredPassword, storedPassword))
                            {
                                UserLoginButton.IsEnabled = true;
                                MessageBox.Show("Lösenordet är korrekt! Nu kan du logga in.", "Inloggnig", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Fel lösenord, försök igen.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                                UserPassword.Clear();
                                UserLoginButton.IsEnabled = false;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Användarnamnet finns inte i databasen.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fel vid inloggning: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                    }


                }
            }

        }

        private void Adminpassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string username = AdminUsernameTextBox.Text.Trim();
                string enteredPassword = AdminPassword.Password.Trim(); ;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(enteredPassword))
                {
                    MessageBox.Show("Användarnamn och lösenord kan inte vara tomma", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                string query = "SELECT Password FROM Users WHERE Username = @Username";



                using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", username);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            reader.Read();
                            string storedPassword = reader["Password"].ToString() ?? string.Empty.Trim();


                            if (BCrypt.Net.BCrypt.Verify(enteredPassword, storedPassword))
                            {
                                AdminLoginButton.IsEnabled = true;
                                MessageBox.Show("Lösenordet är korrekt! Nu kan du logga in.", "Inloggnig", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Fel lösenord, försök igen.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                                AdminPassword.Clear();
                                AdminLoginButton.IsEnabled = false;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Användarnamnet finns inte i databasen.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fel vid inloggning: {ex.Message}\n{ex.StackTrace}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            }
        }

        private void UserEdit_Click(object sender, RoutedEventArgs e)
        {
            UserProfilePage userProfilePage = new UserProfilePage();
            userProfilePage.Show();
            this.Close();

        }

        private void AdminEdit_Click(object sender, RoutedEventArgs e) 
        {
            AdminProfilePage adminProfilePage = new AdminProfilePage();
            adminProfilePage.Show();
            this.Close();

        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {

                this.DragMove();
            }
        }

        private void NotRegistered_Click(object sender, RoutedEventArgs e)
        {
            RegistrationPage regPage = new RegistrationPage();
            regPage.Show();
            this.Close();
        }

        private void ShowUserPasswordToggle_Checked(object sender, RoutedEventArgs e)
        {

            UserPassword.Visibility = Visibility.Collapsed;
            UserPasswordTextBox.Visibility = Visibility.Visible;
            UserPasswordTextBox.Text = UserPassword.Password;


            UserEyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/TucLoggBok;component/Media/Buttons/eye_open.png"));
        }

        private void ShowUserPasswordToggle_UnChecked(object sender, RoutedEventArgs e)
        {
            UserPassword.Visibility = Visibility.Visible;
            UserPasswordTextBox.Visibility = Visibility.Collapsed;
            UserPassword.Password = UserPasswordTextBox.Text;


            UserEyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/TucLoggBok;component/Media/Buttons/eye_closed.png"));
        }

        private void ShowAdminPasswordToggle_Checked(object sender, RoutedEventArgs e)
        {
            AdminPassword.Visibility = Visibility.Collapsed;
            AdminPasswordTextBox.Visibility = Visibility.Visible;
            AdminPasswordTextBox.Text = AdminPassword.Password;


            AdminEyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/TucLoggBok;component/Media/Buttons/eye_open.png"));

        }

        private void ShowAdminPasswordToggle_UnChecked(object sender, RoutedEventArgs e)
        {
            AdminPassword.Visibility = Visibility.Visible;
            AdminPasswordTextBox.Visibility = Visibility.Collapsed;
            AdminPassword.Password = AdminPasswordTextBox.Text;


            AdminEyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/TucLoggBok;component/Media/Buttons/eye_closed.png"));
        }

    }
}


