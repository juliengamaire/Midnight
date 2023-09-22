using System;

[System.Serializable]
public class DataBase
{
    public string[] PlaylistIdsAlreadyFetched;
    public Album[] Albums;
}

[System.Serializable]
public class Album
{
    public string[] Artists;
    public string[] Genres;
    public string Href;
    public string Id;
    public string[] ImagesUrls;
    public string Name;
    public string ReleaseDate;
    public string Uri;

    public void AddGenre(string genre)
    {
        genre = genre.ToLowerInvariant();
        // Vérifiez si le genre existe déjà dans le tableau.
        if (!GenreExists(genre))
        {
            Array.Resize(ref Genres, Genres.Length + 1);
            Genres[Genres.Length] = genre;
        }
    }

    private bool GenreExists(string genre)
    {
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