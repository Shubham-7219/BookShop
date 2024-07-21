﻿using MyModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyUtility.GenreRepository
{
    public interface IGenreRepository
    {
        Task AddGenre(Genre genre);
        Task UpdateGenre(Genre genre);
        Task<Genre?> GetGenreById(int id);
        Task DeleteGenre(Genre genre);
        Task<IEnumerable<Genre>> GetGenres();
    }
}
