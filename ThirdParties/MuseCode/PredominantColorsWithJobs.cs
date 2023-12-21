// 2023-12-19 AI-Tag 
// This was created with assistance from Muse, a Unity Artificial Intelligence product

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

// Define a struct for the job
struct ColorCountJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Color> ImageColors;
    public NativeHashMap<Color, int>.ParallelWriter ColorCounts;
    public int Precision;

    public void Execute(int index)
    {
        Color pixelColor = ImageColors[index];

        // Group similar colors together by reducing the precision of the RGB values
        float r = Mathf.Round(pixelColor.r * Precision) / Precision;
        float g = Mathf.Round(pixelColor.g * Precision) / Precision;
        float b = Mathf.Round(pixelColor.b * Precision) / Precision;
        Color groupedColor = new Color(r, g, b);

        // Use a thread-safe method to increment the color count in the NativeHashMap
        ColorCounts.Add(groupedColor, 1);
    }
}

void Start()
{
    Texture2D tex = ... // Your loaded image
    int n = ... // The number of predominant colors to extract
    int precision = ... // The precision for color grouping, lower means fewer color bins

    // Downscale the image for performance
    TextureScale.Bilinear(tex, tex.width / 2, tex.height / 2);

    // Create NativeArrays to pass to the job
    NativeArray<Color> imageColors = new NativeArray<Color>(tex.GetPixels(), Allocator.TempJob);
    NativeHashMap<Color, int> colorCounts = new NativeHashMap<Color, int>(imageColors.Length, Allocator.TempJob);

    // Schedule the job, dividing the work among multiple threads
    ColorCountJob job = new ColorCountJob
    {
        ImageColors = imageColors,
        ColorCounts = colorCounts.AsParallelWriter(),
        Precision = precision
    };

    JobHandle jobHandle = job.Schedule(imageColors.Length, 64);
    jobHandle.Complete();

    // Extract the 'n' most frequent colors from the colorCounts array
    List<KeyValuePair<Color, int>> sortedColors = colorCounts.ToList();
    sortedColors.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

    // Extract the 'n' most predominant colors
    List<Color> predominantColors = new List<Color>();
    for (int i = 0; i < n; i++)
    {
        predominantColors.Add(sortedColors[i].Key);
    }

    // Dispose of the NativeArrays and NativeHashMap when finished
    imageColors.Dispose();
    colorCounts.Dispose();
}
