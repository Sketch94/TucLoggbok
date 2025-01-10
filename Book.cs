using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TucLoggbok
{
    public class Book
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Status { get; set; }

        public Book(int bookID, string title, string author, string iSBN, string status)
        {

            if (string.IsNullOrEmpty(iSBN) || (iSBN.Length != 13)) throw new ArgumentException("ISBN måste vara 13 tecken långt.");
            if (string.IsNullOrEmpty(status) || (status != "Tillgänglig" && status != "Utlånad")) throw new ArgumentException("Ange om boken är Tillgänglig eller utlånad.");


            BookID = bookID;
            Title = title;
            Author = author;
            ISBN = iSBN;
            Status = status;
        }

        public override string ToString()
        {
            return $"BookID: {BookID}, Title: {Title}, Author: {Author}, ISBN: {ISBN}, Status {Status}";
        }

        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
        }
    }
}
