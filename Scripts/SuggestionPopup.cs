using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// Displays and manages the autocomplete suggestion popup in the UI.
public class SuggestionPopup : MonoBehaviour
{
    public RectTransform panel;
    public GameObject suggestionItemPrefab;
    public VerticalLayoutGroup layoutGroup;

    private List<GameObject> currentItems = new List<GameObject>();
    private int selectedIndex = 0;
    private List<string> currentSuggestions;
    private string currentWord;
    private TMP_InputField targetInput;

    public Canvas canvas; // Must be assigned (usually the root canvas)
    /// Displays the suggestion panel with the given list of suggestions.
    public void ShowSuggestions(List<string> suggestions, string word, int caretPos, TMP_InputField input)
    {
        ClearItems();
        currentSuggestions = suggestions;
        currentWord = word;
        targetInput = input;

        // Create UI items for each suggestion
        foreach (string suggestion in suggestions)
        {
            GameObject item = Instantiate(suggestionItemPrefab, layoutGroup.transform);
            item.GetComponentInChildren<TMP_Text>().text = suggestion;
            currentItems.Add(item);
        }

        selectedIndex = 0;
        HighlightSelected();
        panel.gameObject.SetActive(true);
        UpdatePosition();
    }

    /// Hides the suggestion panel and clears items.
    public void Hide()
    {
        ClearItems();
        panel.gameObject.SetActive(false);
    }

    /// Removes all suggestion items from the panel.
    void ClearItems()
    {
        foreach (var item in currentItems)
            Destroy(item);
        currentItems.Clear();
    }

    /// Selects the next suggestion in the list.
    public void SelectNext()
    {
        selectedIndex = (selectedIndex + 1) % currentSuggestions.Count;
        HighlightSelected();
    }

    /// Selects the previous suggestion in the list.
    public void SelectPrevious()
    {
        selectedIndex = (selectedIndex - 1 + currentSuggestions.Count) % currentSuggestions.Count;
        HighlightSelected();
    }

    /// Applies the selected suggestion into the input field.
    public void ConfirmSelection()
    {
        string selected = currentSuggestions[selectedIndex];
        ReplaceWord(selected);
        Hide();
    }

    /// Highlights the currently selected suggestion in the list.
    void HighlightSelected()
    {
        for (int i = 0; i < currentItems.Count; i++)
        {
            currentItems[i].GetComponent<Image>().color = (i == selectedIndex) ? Color.white : Color.green;
        }
    }

    /// Replaces the current word in the input field with the selected suggestion.
    void ReplaceWord(string newWord)
    {
        int caretPos = targetInput.caretPosition;
        string beforeCaret = targetInput.text.Substring(0, caretPos);
        int wordStart = beforeCaret.LastIndexOf(currentWord);

        // Remove any extra characters after the word (e.g. tabs)
        int extraLength = caretPos - (wordStart + currentWord.Length);
        if (extraLength > 0)
        {
            targetInput.text = targetInput.text.Remove(wordStart + currentWord.Length, extraLength);
        }

        // Replace the word
        string newText = targetInput.text.Remove(wordStart, currentWord.Length).Insert(wordStart, newWord);
        targetInput.text = newText;
        targetInput.caretPosition = wordStart + newWord.Length;

        // Re-trigger autocomplete based on new text
        AutoCompleteManager.Instance.OnTextChanged(targetInput.text);
    }

    /// Updates the popup panel's position relative to the text caret.
    public void UpdatePosition()
    {
        if (targetInput == null || targetInput.textComponent == null || canvas == null)
            return;

        TMP_Text text = targetInput.textComponent;
        int caretIndex = Mathf.Clamp(targetInput.caretPosition, 0, targetInput.text.Length);

        text.ForceMeshUpdate();
        if (text.textInfo.characterCount == 0)
            return;

        int charIndex = Mathf.Clamp(caretIndex - 1, 0, text.textInfo.characterCount - 1);
        TMP_CharacterInfo charInfo = text.textInfo.characterInfo[charIndex];

        Vector3 worldPos = text.transform.TransformPoint(charInfo.bottomLeft);
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.worldCamera,
            out localPoint
        );

        // Set pivot to top-left (to account for potential overflow)
        panel.pivot = new Vector2(0, 1);

        float yOffset = -text.fontSize * 1.2f;
        Vector2 anchored = localPoint + new Vector2(0, yOffset);

        // Rebuild layout to get updated size
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        Vector2 panelSize = panel.sizeDelta;

        RectTransform canvasRect = canvas.transform as RectTransform;
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // Shift left if popup goes beyond right edge
        float rightEdge = anchored.x + panelSize.x;
        if (rightEdge > canvasWidth / 2f)
            anchored.x -= (rightEdge - canvasWidth / 2f + 10f);

        // Shift up if popup goes beyond bottom edge
        float bottomEdge = anchored.y - panelSize.y;
        if (bottomEdge < -canvasHeight / 2f)
            anchored.y += (-canvasHeight / 2f - bottomEdge + 10f);

        // Optionally shift down if it overflows top edge
        float topEdge = anchored.y;
        if (topEdge > canvasHeight / 2f)
            anchored.y = canvasHeight / 2f - 10f;

        panel.anchoredPosition = anchored;
    }
}
