using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TucLoggbok
{
    public class BookItem
    {
        public int BookID { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? ISBN { get; set; }
        public string? Status { get; set; }
        public string? Username { get; set; }

        public BookItem(int bookID, string title, string author, string iSBN, string status, string username)
        {
            this.BookID = bookID;
            this.Title = title;
            this.Author = author;
            this.ISBN = iSBN;
            this.Status = status;
            this.Username = username;
        }

        public BookItem() { }

        public override string ToString()
        {
            return $"Titel: {Title}, Författare: {Author}, ISBN: {ISBN}, Status: {Status}";
        }
    }
}
