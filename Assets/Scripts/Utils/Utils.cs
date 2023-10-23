using SFB;
using UnityEngine;

namespace Midnight.Utils
{ 
    public class Utils : MonoBehaviour
    {
        public static string GetPathViaFileExplorer(string title, string extension)
        {
            string path = null;
            string[] paths = StandaloneFileBrowser.OpenFilePanel(title, "", extension, false);
            if (paths.Length == 1)
            {
                if (!string.IsNullOrEmpty(paths[0]))
                {
                    path = paths[0];
                }
            }

            return path;
        }
    }
}
