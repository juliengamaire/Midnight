using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownSearch : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private string currentSearch = "";
    private float searchResetDelay = 0.5f;
    private float searchResetTimer;

    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    void Update()
    {
        if (Input.anyKeyDown && dropdown.IsExpanded)
        {
            char inputChar = (char)0;
            for (int i = (int)'a'; i <= (int)'z'; i++)
            {
                if (Input.GetKeyDown((KeyCode)i))
                {
                    inputChar = (char)i;
                    break;
                }
            }

            if (inputChar != (char)0)
            {
                currentSearch += inputChar;
                searchResetTimer = searchResetDelay;
                SelectOptionStartingWith(currentSearch);
            }
        }

        if (searchResetTimer > 0)
        {
            searchResetTimer -= Time.deltaTime;
            if (searchResetTimer <= 0)
            {
                currentSearch = "";
            }
        }
    }

    void SelectOptionStartingWith(string searchString)
    {
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (dropdown.options[i].text.StartsWith(searchString, System.StringComparison.OrdinalIgnoreCase))
            {
                dropdown.value = i;
                StartCoroutine(SetScrollViewPosition(i));
                break;
            }
        }
    }

    IEnumerator SetScrollViewPosition(int selectedIndex)
    {
        yield return new WaitForEndOfFrame();
        GameObject dropdownList = GameObject.Find("Dropdown List");
        if (dropdownList != null)
        {
            ScrollRect scrollRect = dropdownList.GetComponentInChildren<ScrollRect>();
            if (scrollRect != null)
            {
                float normalizedPosition = 1 - ((float)selectedIndex / (dropdown.options.Count - 1));
                scrollRect.verticalNormalizedPosition = normalizedPosition;
            }
        }
    }
}
