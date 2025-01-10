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
using Microsoft.Data.SqlClient;

namespace TucLoggbok
{
    /// <summary>
    /// Interaction logic for AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Window
    {
        private string username;
        private int userID;

        public AdminPage(int userID, string username)
        {
            InitializeComponent();
            this.userID = userID;
            this.username = username;

            DisplayWelcomeMessage(this.username);
        }

        public AdminPage()
        {
            InitializeComponent();
            this.username = string.Empty;
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {

            string title = TitleTextBox.Text.Trim();
            string author = AuthorTextBox.Text.Trim();
            string isbn = ISBNTextBox.Text.Trim();
            string status = StatusTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(isbn) || string.IsNullOrEmpty(status))
            {
                MessageBox.Show("Alla fält måste vara ifyllda", "Fel", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Book newBook = new Book(0, title, author, isbn, status);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // SQL-fråga för att lägga till böcker i datbasen
            string query = "INSERT INTO Books (Title, Author, ISBN, Status) VALUES (@Title, @Author, @ISBN, @Status)";

            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@Author", author);
                command.Parameters.AddWithValue("@ISBN", isbn);
                command.Parameters.AddWithValue("@Status", status);


                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Boken har lagts till i databasen.", "Bok tillagd", MessageBoxButton.OK, MessageBoxImage.Information);


                        TitleTextBox.Clear();
                        AuthorTextBox.Clear();
                        ISBNTextBox.Clear();
                        StatusTextBox.Clear();

                        UpdateLog_Click(sender, e);


                    }
                    else
                    {
                        MessageBox.Show("Boken kunde inte läggas till.", "Bok finns", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                }
                catch (Exception ex) // om något fel uppstår
                { 
                    MessageBox.Show($"Fel vid insättning av bok: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
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
             // denna SQL väljer titel, isbn eller författare som matchar sökningen i searchtextbox
            string query = "SELECT * FROM Books WHERE Title LIKE @SearchQuery OR Author LIKE @SearchQuery OR ISBN LIKE @SearchQuery";

            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                // här så lägger jag in SQL-frågan från inmatningen följt av en wildcard eller jokertecken
                // det innebär att om jag skriver något orelaterat och sen min titel på min bok
                // så visas ändå boken oavsett vad som finns förre eller efter.
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

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchBook_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            if (LoanHistoryListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("Välj en eller flera böcker att radera", "Fel", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }

            MessageBoxResult deletion = MessageBox.Show("Vill du radera boken eller böckerna?", "Borttagning av böcker lyckades", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (deletion == MessageBoxResult.Yes)
            {
                // cast konverterar de valda böckerna till bookitem och tolist omvandlar dessa böcker till en lista vilket gör att
                // det blir enklare att arbeta med det eftersom mina böcker är en lista
                var BooksToDelete = LoanHistoryListBox.SelectedItems.Cast<BookItem>().ToList();

                foreach (BookItem selectedBook in BooksToDelete)
                {
                    string deleteQuery = "DELETE FROM Books WHERE BookID = @BookID";

                    using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                    {
                        SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                        deleteCommand.Parameters.AddWithValue("@BookID", selectedBook.BookID);

                        try
                        {
                            connection.Open();
                            int rowsAffected = deleteCommand.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                LoanHistoryListBox.Items.Remove(selectedBook);
                            }
                            else
                            {
                                MessageBox.Show("Boken eller böckerna kunde inte raderas", "Fel", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Fel vid radering: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Information);

                        }
                    }
                }
                MessageBox.Show("De valda böckerna har raderats från databasen", "Borttagning av böcker lyckas", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            UpdateLog_Click(sender, e);
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

        private void ShowLoanHistory_Click(object sender, RoutedEventArgs e)
        {
            if (LoanHistoryListBox.SelectedItem == null)
            {
                MessageBox.Show("Välj en bok för att visa lånehistorik", "Ingen bok vald", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            BookItem selectedBook = (BookItem)LoanHistoryListBox.SelectedItem;

            // här skickar jag en fråga SQL-fråga om att hämta BookID, titeln, författare, isbn, status och username från dbo.books samt borrowdate och returndate
            // ISNULL(b.Username) AS Username innebär att NULL värdena ersätts med texten Ej utlånad
            // LEFT JOIN innebär att de vänstra kolumnerna i books hämtas där bookid från dbo.books eller lika med alla anra bookid
            
            string query = "SELECT b.BookID, b.Title, b.Author, b.ISBN, b.Status, ISNULL(b.Username, 'Ej utlånad') AS Username, bo.BorrowDate, bo.ReturnDate " +
                           "FROM Books b " +
                           "LEFT JOIN Borrowing bo ON b.BookID = bo.BookID " +
                           "WHERE b.BookID = @BookID";


            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BookID", selectedBook.BookID);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        BookItem loanHistoryItem = new BookItem // här skapar jag en ny instans av bookitem med kolumnerna som finns i databasen och lagrar detta i loanhistoryitem
                        {
                            // reader hämtar värdden i databasen 
                            BookID = (int)reader["BookID"],
                            Title = (string)reader["Title"],
                            Author = (string)reader["Author"],
                            ISBN = (string)reader["ISBN"],
                            Status = (string)reader["Status"],
                            Username = (string)reader["Username"]

                            // Hämtar låneinfo från databasen om en bok är utlånad, återlämnad
                            // om den valda boken är utlånad eller tillgänglig så meddelas detta i en messagebox
                        };
                        string loanInfo = $"Titel: {loanHistoryItem.Title}, Författare: {loanHistoryItem.Author}, ISBN: {loanHistoryItem.ISBN}, Status: {loanHistoryItem.Status}";

                        // den här koden skrivs endast ut om bokens status är tillgänglig i databasen
                        if (loanHistoryItem.Status.Trim() == "Tillgänglig")
                        {
                            MessageBox.Show(loanInfo += " - Boken finns att låna.", "Bok tillgänglig", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {

                            if (loanHistoryItem.Username != "Ej utlånad")
                            {
                                loanInfo += $", Lånad av: {loanHistoryItem.Username}";
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("BorrowDate")))
                            {
                                loanInfo += $", Lånedattum: {reader["BorrowDate"]}";
                            }
                             // denna kontrollerar att kolumnen returndate inte är null, om det finns data så visas 
                             // en messagebox med låneinfo + returndate i lånehistoriken.
                            if (!reader.IsDBNull(reader.GetOrdinal("ReturnDate")))
                            {
                                loanInfo += $", Återlämning: {reader["ReturnDate"]}";

                            }
                            else
                            {
                                loanInfo += $", Återlämning: Ej återlämnad.";
                            }

                            MessageBox.Show(loanInfo, "Lånehistorik", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                        }
                    }
                        reader.Close();
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"Fel vid hämtning av lånehistorik: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }

        }

        private void DisplayWelcomeMessage(string username)
        {
            WelcomeMessageTextBlock.Text = $"Välkommen, {username}!";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {

                this.DragMove();
            }
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            if (LoanHistoryListBox.Items.Count == 0)
            {
                MessageBox.Show("Listan är tom.", "Tom lista", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBoxResult messageBoxResult = MessageBox.Show($"Vill du verkligen radera allting i loggen?", "Varning", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                MessageBox.Show("Loggen är raderad.", "Framgång", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                LoanHistoryListBox.Items.Clear();
            }
            else
            {
                MessageBox.Show("Loggen raderades inte.", "Nekad", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                LoanHistoryListBox = null;
            }
        }

        private void EditBook_Click(object sender, RoutedEventArgs e)
        {

            if (LoanHistoryListBox.Items.Count == 0)
            {
                MessageBox.Show("Listan är tom.", "Tom lista", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (LoanHistoryListBox.SelectedItem is not BookItem selectedBook)
            {
                MessageBox.Show("Välj en bok att redigera.", "Ingen bok vald", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EditBook editBook = new EditBook(selectedBook)
            {
                Owner = this
            };
            editBook.ShowDialog();
        }

        // uppdaterar loggen i loanhistorylistbox
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

        private void LoanHistoryListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                var selectedItem = LoanHistoryListBox.SelectedItems;
                foreach (var item in LoanHistoryListBox.SelectedItems) // ger möjligheten att välja fler böcker med vänster ctrl
                {
                    if (!selectedItem.Contains(item))
                    {
                        LoanHistoryListBox.Items.Add(item);
                    }
                }
            }
        }
    }
}

