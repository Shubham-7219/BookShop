using MyModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MyUtility.HomeRepository
{
    public interface IHomeRepository
    {
        Task<IEnumerable<Genre>> Genres();
        Task<IEnumerable<Book>> GetBooks(string sTerm = "", int genreId = 0);
    }
}
