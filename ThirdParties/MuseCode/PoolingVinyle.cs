// 2023-12-19 AI-Tag 
// This was created with assistance from Muse, a Unity Artificial Intelligence product

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VinylPool : MonoBehaviour
{
    public GameObject vinylPrefab;
    public Button nextButton;
    public int poolSize = 60; // 3 bacs de 20 vinyles

    private List<GameObject> vinylPool;
    private int currentIndex = 0;

    void Start()
    {
        vinylPool = new List<GameObject>();
        for(int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(vinylPrefab);
            obj.SetActive(false);
            vinylPool.Add(obj);
        }

        nextButton.onClick.AddListener(LoadNextSet);
        LoadNextSet();
    }

    void LoadNextSet()
    {
        for(int i = 0; i < poolSize; i++)
        {
            GameObject vinyl = GetVinyl();
            // Supposant que "url" est l'URL de l'image du vinyle actuel
            StartCoroutine(LoadAlbumCover(vinyl, url));
        }
    }

    public GameObject GetVinyl()
    {
        if(currentIndex >= vinylPool.Count)
        {
            currentIndex = 0;
        }

        GameObject vinyl = vinylPool[currentIndex];
        vinyl.SetActive(true);
        currentIndex++;
        return vinyl;
    }

    IEnumerator LoadAlbumCover(GameObject vinyl, string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            vinyl.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
