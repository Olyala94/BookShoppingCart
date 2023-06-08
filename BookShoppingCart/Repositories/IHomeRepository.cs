﻿namespace BookShoppingCart
{
    public interface IHomeRepository
    {
        Task<IEnumerable<Book>> GetBooks(string sTerm = "", int genreId = 0);
    }
}