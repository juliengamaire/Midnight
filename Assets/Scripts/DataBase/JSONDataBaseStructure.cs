﻿using System;

[System.Serializable]
public class DataBase
{
    public PlaylistId[] PlaylistIdsAlreadyFetched;
    public Artist[] Artists;
    public Album[] Albums;
}

[System.Serializable]
public class PlaylistId
{
    public string Id;
    public string Genre;
}

[System.Serializable]
public class Artist
{
    public string Id;
    public string[] Genres;

    public bool TryAddGenre(string genre)
    {
        genre = genre.ToLowerInvariant();
        // Vérifiez si le genre existe déjà dans le tableau.
        if (!GenreExists(genre))
        {
            Array.Resize(ref Genres, Genres.Length + 1);
            Genres[Genres.Length - 1] = genre;
            return true;
        }
        return false;
    }

    private bool GenreExists(string genre)
    {
        if (Genres == null)
        {
            Genres = new string[0];
        }

        // Vérifiez si le genre existe déjà dans le tableau.
        foreach (string existingGenre in Genres)
        {
            if (existingGenre == genre)
            {
                return true;
            }
        }
        return false;
    }
}

[System.Serializable]
public class Album
{
    public string[] ArtistsName;
    public string[] ArtistsId;
    public string Href;
    public string Id;
    public string[] ImagesUrls;
    public string Name;
    public string ReleaseDate;
    public string Uri;
}