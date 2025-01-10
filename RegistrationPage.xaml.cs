using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BCrypt.Net;
namespace TucLoggbok
{
    /// <summary>
    /// Interaction logic for RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Window
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPage loginPage = new LoginPage();
            loginPage.Show();

            this.Close();
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {

            string username = UsernameTextBox.Text.Trim();
            string password = RegisterPasswordTextBox.Password.Trim();
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();

            string userType = UserRadioButton.IsChecked == true ? "User" : "Admin";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(email))
            {

                MessageBox.Show("Alla fält måste fyllas i.", "Registrering misslyckades", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsEmailValid(email))
            {
                MessageBox.Show("Din valda e-postadress är inte giltig.\n\nAnvänd hotmail.com, outlook.com eller gmail.com adress.", "Ogiltig e-postadress", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsPhoneValid(phone))
            {
                MessageBox.Show("Telefonnumret är ogiltigt, ange ett nummer med 10s siffror.\n", "Felaktigt nummer.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (WeakPassword(password))
            {
                MessageBox.Show("Lösenordet är för svagt. Vänligen använd minst 8 tecken, en stor och en liten bokstav, en siffra, och ett specialtecken", "Svagt lösenord", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            string query = "INSERT INTO Users (Username, Password, Email, Phone, UserType) VALUES (@Username, @Password, @Email, @Phone, @UserType)";
            string checkUsernameQuery = "SELECT COUNT (*) FROM Users WHERE Username = @Username";

            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand checkCommand = new SqlCommand(checkUsernameQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Username", username);
                        int userCount = (int)checkCommand.ExecuteScalar();

                        if (userCount > 0)
                        {
                            MessageBox.Show("Användarnamnet är redan registrerat", "Fel", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Registrering lyckades!\nKlicka på OK för att återgå till startsidan.", "Framgång", MessageBoxButton.OK, MessageBoxImage.Information);

                            LoginPage loginPage = new LoginPage();
                            loginPage.Show();
                            this.Close();
                        }
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", hashedPassword);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Phone", phone);
                        command.Parameters.AddWithValue("@UserType", userType);

                        command.ExecuteNonQuery();
                    }

                    UsernameTextBox.Clear();
                    RegisterPasswordTextBox.Clear();
                    EmailTextBox.Clear();
                    PhoneTextBox.Clear();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fel vid registrering: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                finally
                {
                    connection.Close();
                }

            }
        }

        private static bool WeakPassword(string password)
        {

            if (password.Length < 8)
            {
                return true;
            }

            var passwordStrengthRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");

            if (!passwordStrengthRegex.IsMatch(password))
            {
                return true;
            }

            string[] commonWeakPasswords = { "password", "123456" };

            if (commonWeakPasswords.Contains(password.ToLower()))
            {
                return true;
            }
            return false;

        }

        private static bool IsEmailValid(string email)
        {

            email = email.Trim();

            var emailRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

            if (!emailRegex.IsMatch(email))
            {
                return false;
            }

            string[] emailParts = email.Split('@');

            if (emailParts.Length != 2)
            {
                return false;
            }

            string emailDomain = emailParts[1].ToLower();

            string[] validDomains = { "hotmail.com", "outlook.com", "gmail.com" };

            return validDomains.Contains(emailDomain);
        }

        private static bool IsPhoneValid(string phone)
        {
            var phoneRegex = new System.Text.RegularExpressions.Regex(@"^\d{10}$|^(\d{3})-(\d{3})-(\d{4})$");
            return phoneRegex.IsMatch(phone);
        }

        private void UsernameTextBox_Changed(object sender, EventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (!string.IsNullOrEmpty(textbox.Text))
            {
                textbox.Text = textbox.Text.Substring(0, 1).ToUpper() + textbox.Text.Substring(1).ToLower();
                textbox.SelectionStart = textbox.Text.Length;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {

                this.DragMove();
            }
        }

        private void ShowRegisteredPasswordToggle_Checked(object sender, RoutedEventArgs e)
        {
            RegisterPasswordTextBox.Visibility = Visibility.Collapsed;
            RegisterTextBox.Visibility = Visibility.Visible;
            RegisterTextBox.Text = RegisterPasswordTextBox.Password;


            RegisterEyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/TucLoggBok;component/Media/Buttons/eye_open.png"));

        }

        private void ShowRegisteredPasswordToggle_UnChecked(object sender, RoutedEventArgs e)
        {
            {
                RegisterPasswordTextBox.Visibility = Visibility.Visible;
                RegisterTextBox.Visibility = Visibility.Collapsed;
                RegisterPasswordTextBox.Password= RegisterTextBox.Text;


                RegisterEyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/TucLoggBok;component/Media/Buttons/eye_closed.png"));

            }
        }

        private void UserRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            AdminRadioButton.IsChecked = false;
        }

        private void AdminRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UserRadioButton.IsChecked = false;
        }
    }
}
