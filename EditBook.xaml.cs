using Microsoft.Data.SqlClient;
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
using System.Windows.Shapes;

namespace TucLoggbok
{
    /// <summary>
    /// Interaction logic for EditBook.xaml
    /// </summary>
    public partial class EditBook : Window
    {
        private BookItem CurrentBook { get; }

        public EditBook(BookItem bookItem)
        {
            InitializeComponent();
            CurrentBook = bookItem;


            NewTitleTextBox.Text = CurrentBook.Title;
            NewAuthorTextBox.Text = CurrentBook.Author;
            NewISBNTextBox.Text = CurrentBook.ISBN;
            NewStatusTextBox.Text = CurrentBook.Status;
        }

        private void AddUpdatedBook_Click(object sender, RoutedEventArgs e)
        {
            CurrentBook.Title = NewTitleTextBox.Text;
            CurrentBook.Author = NewAuthorTextBox.Text;
            CurrentBook.ISBN = NewISBNTextBox.Text;
            CurrentBook.Status = NewStatusTextBox.Text;

            UpdateBookInDatabse(CurrentBook);

            if (Owner is AdminPage adminPage)
            {
                    adminPage.LoanHistoryListBox.Items.Refresh();
            }
            this.Close();
        }

        private void UpdateBookInDatabse(BookItem bookItem)
        {

            string newTitle = NewTitleTextBox.Text.Trim();
            string newAuthor = NewAuthorTextBox.Text.Trim();
            string newIsbn = NewISBNTextBox.Text.Trim();
            string newStatus = NewStatusTextBox.Text.Trim();

            if (string.IsNullOrEmpty(newTitle) || string.IsNullOrEmpty(newAuthor) || string.IsNullOrEmpty(newIsbn) || string.IsNullOrEmpty(newStatus))
            {
                MessageBox.Show("Alla fält måste vara ifyllda", "Fel", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Book newBook = new Book(0, newTitle, newAuthor, newIsbn, newStatus);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }

            string query = "UPDATE Books SET Title = @Title, Author = @Author, ISBN = @ISBN, Status = @status WHERE BookID = @BookID";
            string updateUsernameQuery = "UPDATE Books SET Username = NULL WHERE BookID = @BookID AND Status = 'Tillgänglig'";
           
            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                SqlCommand updateUsernameCommand = new SqlCommand(updateUsernameQuery, connection);

                command.Parameters.AddWithValue("@Title", bookItem.Title);
                command.Parameters.AddWithValue("@Author", bookItem.Author);
                command.Parameters.AddWithValue("@ISBN", bookItem.ISBN);
                command.Parameters.AddWithValue("@Status", bookItem.Status);
                command.Parameters.AddWithValue("@BookID", bookItem.BookID);

                updateUsernameCommand.Parameters.AddWithValue("@BookID", bookItem.BookID);
                updateUsernameCommand.Parameters.AddWithValue("@Status", bookItem.Status);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        if (bookItem?.Status?.Trim() == "Tillgänglig")
                        {
                            updateUsernameCommand.ExecuteNonQuery();
                        }
                        MessageBox.Show("Boken uppdaterades i databasen", "Uppdatering lyckades", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ingen bok uppdaterades, kontrollera BookID ", "Uppdatering misslyckades", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fel vid uppdatering: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
        }

    }
}
