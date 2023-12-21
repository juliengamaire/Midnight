// 2023-12-19 AI-Tag 
// This was created with assistance from Muse, a Unity Artificial Intelligence product

public GameObject GetVinylOrDelimiter(string artistName)
{
    if (currentIndex > 0 && artistName[0] != lastArtistName[0])
    {
        GameObject delimiter = Instantiate(delimiterPrefab);
        delimiter.GetComponent<Text>().text = artistName[0].ToString();
        return delimiter;
    }

    lastArtistName = artistName;
    return GetVinyl();
}

IEnumerator LoadAlbumCover(GameObject obj, string url, bool isDelimiter)
{
    if(isDelimiter)
    {
        yield break;
    }

    // Le reste du code pour charger l'image...
}

void LoadNextSet()
{
    // Pour chaque vinyle dans le nouvel ensemble...
    for(int i = 0; i < poolSize; i++)
    {
        // Supposant que "artistName" et "url" sont le nom de l'artiste et l'URL de l'image du vinyle actuel
        GameObject obj = GetVinylOrDelimiter(artistName);
        bool isDelimiter = obj.GetComponent<Text>() != null;
        StartCoroutine(LoadAlbumCover(obj, url, isDelimiter));
    }
}
