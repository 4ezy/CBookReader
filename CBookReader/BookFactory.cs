using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBookReader
{
    class BookFactory
    {
        public Book CreateBook(BookTypes bookType)
        {
            Book book = null;

            switch (bookType)
            {
                case BookTypes.ComicBook:
                    book = new ComicBook();
                    break;
                default:
                    break;
            }

            return book;
        }
    }
}
