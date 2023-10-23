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

public enum Genre
{
    NONE,
    ACID_ROCK,
    ALT_ROCK,
    ALT_METAL,
    BLACK,
    BLACKENED_DEATH,
    BLUEGRASS,
    BLUES,
    BLUES_ROCK,
    CELTIC,
    DEATH_METAL,
    DEATHGRIND,
    DJENT,
    DOOM,
    FOLK,
    GARAGE,
    GLAM,
    GOREGRIND,
    GOTH,
    GRINDCORE,
    GROOVE,
    GRUNGE,
    HAIR,
    HARD_ROCK,
    HARDCORE,
    HEAVY_METAL,
    INDUSTRIAL,
    MATH,
    MELODEATH,
    METAL,
    METAL_MISC,
    METALCORE,
    METALSTEP,
    MODERN_PROG,
    MODERN_DEATHCORE,
    NEOCLASSICAL,
    NU,
    PIRATE,
    POP_METAL,
    POST,
    PROG,
    POWER,
    PSYCHEDELIC,
    PUNK,
    PUNK_ROCK,
    ROCK,
    ROCK_N_ROLL,
    ROCKABILLY,
    SLUDGE,
    SOUTHERN,
    STONER,
    SYMPHONIC,
    TECH_DEATH,
    TRASH,
    VIKING
}