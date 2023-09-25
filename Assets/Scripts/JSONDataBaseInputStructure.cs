[System.Serializable]
public class DataBaseInput
{
    public PlaylistInput[] playlists;
    public AlbumInput[] albums;
    public ArtistInput[] artists;
}

[System.Serializable]
public class PlaylistInput
{
    public string playlistId;
    public string genre;
}

[System.Serializable]
public class AlbumInput
{
    public string albumId;
    public string genre;
}

[System.Serializable]
public class ArtistInput
{
    public string artistId;
    public string genre;
}