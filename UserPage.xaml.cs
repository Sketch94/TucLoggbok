using System;
using System.Collections.Generic;
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
using System.Diagnostics;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace TucLoggbok
{
    /// <summary>
    /// Interaction logic for UserPage.xaml
    /// </summary>
    public partial class UserPage : Window
    {
        readonly string username;
        readonly int userID;

        public UserPage(int userID, string username)
        {
            InitializeComponent();

            this.userID = userID;
            this.username = username;

            DisplayWelcomeMessage(this.username);
        }

        public UserPage()
        {
            InitializeComponent();
            this.username = string.Empty;
        }

        private void ShowLoanHistory_Click(object sender, RoutedEventArgs e)
        {
            string query = "SELECT u.Username, b.Title, bo.BorrowDate, bo.ReturnDate " +
                "FROM Borrowing bo " +
                "JOIN Users u ON bo.UserID = u.UserID " +
                "LEFT JOIN Books b ON bo.BookID = b.BookID " +
                "WHERE bo.UserID = @UserID";


            using (SqlConnection connection = new(DatabaseConfig.ConnectionString))
            {
                SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@UserID", userID);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    LoanHistoryListBox.Items.Clear();

                    while (reader.Read())
                    {
                        string loanInfo = $"Bok: {reader["Title"]} Lånedatum: {reader["BorrowDate"]}, Återlämning: {reader["ReturnDate"] ?? "Ej återlämnad"}";
                        LoanHistoryListBox.Items.Add(loanInfo);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"Fel vid hämtning av lånehistorik: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }

        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPage loginPage = new LoginPage();
            loginPage.Show();

            this.Close();
        }

        private void BorrowBook_Click(object sender, RoutedEventArgs e)
        {

            if (LoanHistoryListBox.SelectedItem is BookItem selectedBook)
            {
                if (selectedBook?.Status?.Trim() == "Tillgänglig")
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("Vill du låna den här boken?", "Låna bok", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        try
                        {
                            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                            {
                                connection.Open();
                                string updateQuery = "UPDATE Books SET Status = 'Utlånad', Username = @Username WHERE BookID = @BookID";

                                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                                {
                                    updateCommand.Parameters.AddWithValue("@BookID", selectedBook.BookID);
                                    updateCommand.Parameters.AddWithValue("@Username", username);

                                    int rowsAffected = updateCommand.ExecuteNonQuery();
                                    if (rowsAffected > 0)
                                    {
                                        string insertBorrowingQuery = "INSERT INTO Borrowing (UserID, BookID, BorrowDate, ReturnDate) " +
                                            "VALUES (@UserID, @BookID, @BorrowDate, NULL)";
                                        SqlCommand insertCommand = new SqlCommand(insertBorrowingQuery, connection);
                                        insertCommand.Parameters.AddWithValue("@UserID", userID);
                                        insertCommand.Parameters.AddWithValue("@BookID", selectedBook.BookID);
                                        insertCommand.Parameters.AddWithValue("@BorrowDate", DateTime.Now);

                                        insertCommand.ExecuteNonQuery();

                                        MessageBox.Show("Boken är nu lånad!", "Framgång", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                                        selectedBook.Status = "Utlånad";
                                        selectedBook.Username = username;
                                        UpdateLog_Click(sender, e);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Ingen uppdatering gjordes.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ett fel inträffade vid låneprocessen\n\n{ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Boken är utlånad.", "Lånad bok", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Välj en bok från listan för att låna.", "Ingen bok vald", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ReturnBook_Click(object sender, RoutedEventArgs e)
        {
            if (LoanHistoryListBox.SelectedItem == null)
            {
                MessageBox.Show("Vänligen välj en bok att återlämna", "Fel", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var selectedItem = LoanHistoryListBox.SelectedItem;
            if (selectedItem is BookItem selectedBook)
            {
                int bookID = selectedBook.BookID;
                string updateBookQuery = "UPDATE Books SET Status = 'Tillgänglig', Username = NULL WHERE BookID = @BookID AND Status = 'Utlånad'";
                string updateBorrowingQuery = "UPDATE Borrowing SET ReturnDate = GETDATE() WHERE BookID = @BookID AND UserID = @UserID AND ReturnDate IS NULL";

                using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    SqlCommand updateBookCommand = new SqlCommand(updateBookQuery, connection);
                    SqlCommand updateBorrowingCommand = new SqlCommand(updateBorrowingQuery, connection);
                    updateBookCommand.Parameters.AddWithValue("@BookID", bookID);
                    updateBorrowingCommand.Parameters.AddWithValue("@BookID", bookID);
                    updateBorrowingCommand.Parameters.AddWithValue("@UserID", userID);

                    try
                    {
                        connection.Open();
                        int rowsAffectedBook = updateBookCommand.ExecuteNonQuery();

                        if (rowsAffectedBook > 0)
                        {
                            int rowsAffectedBorrowing = updateBorrowingCommand.ExecuteNonQuery();
                            if (rowsAffectedBorrowing > 0)
                            {
                                MessageBox.Show("Boken har återlämnats.", "Återlämnad", MessageBoxButton.OK, MessageBoxImage.Information);

                                selectedBook.Status = "Tillgänglig";
                                selectedBook.Username = null;
                                LoanHistoryListBox.Items[LoanHistoryListBox.SelectedIndex] = selectedBook;

                                UpdateLog_Click(sender, e);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Boken är inte utlånad eller inte så finns den inte.", "Fel", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fel vid återlämning: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {

                this.DragMove();
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchBook_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void SearchBook_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = SearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchQuery))
            {
                MessageBox.Show("Skriv något för att söka.", "Sökfel", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string query = "SELECT * FROM Books WHERE Title LIKE @SearchQuery OR Author LIKE @SearchQuery OR ISBN LIKE @SearchQuery";

            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SearchQuery", "%" + searchQuery + "%");

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    LoanHistoryListBox.Items.Clear();
                    while (reader.Read())
                    {
                        int bookID = (int)reader["BookID"];
                        string title = (string)reader["Title"];
                        string author = (string)reader["Author"];
                        string isbn = (string)reader["ISBN"];
                        string status = (string)reader["Status"];
                        string username = (string)reader["Username"];

                        BookItem bookItem = new BookItem(bookID, title, author, isbn, status, username);

                        LoanHistoryListBox.Items.Add(bookItem);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"Fel vid sökning: {ex.Message}", "Sökfel", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DisplayWelcomeMessage(string username)
        {
            WelcomeMessageTextBlock.Text = $"Välkommen, {username}!";
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            if (LoanHistoryListBox.Items.Count == 0)
            {
                MessageBox.Show("Listan är tom.", "Fel", MessageBoxButton.OK, MessageBoxImage.Question);
                return;
            }

            MessageBoxResult messageBoxResult = MessageBox.Show($"Vill du verkligen radera allting i loggen?", "Varning", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                LoanHistoryListBox.Items.Clear();
            }
            else
            {
                LoanHistoryListBox = null;
            }

        }

        private void UpdateLog_Click(object sender, RoutedEventArgs e)
        {
            LoanHistoryListBox?.Items.Clear();

            string query = "SELECT BookID, Title, Author, ISBN, Status FROM Books";

            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int bookID = Convert.ToInt32(reader["BookID"]);
                        string title = reader["Title"].ToString() ?? "Okänd Titel";
                        string author = reader["Author"].ToString() ?? "Okänd Författare";
                        string isbn = reader["ISBN"].ToString() ?? "Okänt ISBN";
                        string status = reader["Status"].ToString() ?? "Okänd Status";

                        BookItem bookItem = new(bookID, title, author, isbn, status, username);

                        LoanHistoryListBox?.Items.Add(bookItem);
                    }
                    SearchTextBox.Clear();
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fel vid uppdatering av loggen: {ex.Message}\n{ex.StackTrace}", "Fel", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

        }
    }
}


