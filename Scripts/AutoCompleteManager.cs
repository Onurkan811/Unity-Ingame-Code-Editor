using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

/// Manages autocomplete suggestions based on user input in a TMP input field.
public class AutoCompleteManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public SuggestionPopup suggestionPopup;
    public SyntaxContextAnalyzer contextAnalyzer;

    // Singleton instance to allow global access
    public static AutoCompleteManager Instance { get; private set; }

    void Awake()
    {
        // Ensure a single instance (Singleton pattern)
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Listen to input field value changes
        inputField.onValueChanged.AddListener(OnTextChanged);
    }

    /// Called whenever the input field text changes.
    /// Updates and displays autocomplete suggestions.
    public void OnTextChanged(string input)
    {
        int caretPos = inputField.caretPosition;
        string context = input.Substring(0, caretPos);
        string currentWord = GetLastWord(context);

        List<string> suggestions = contextAnalyzer.GetSuggestionsForContext(context, caretPos, currentWord);

        if (suggestions.Count > 0 && currentWord.Length > 0)
        {
            // Don't show suggestions if only one match and it's identical to the current word
            if (suggestions.Count == 1 && suggestions[0].ToLower() == currentWord.ToLower())
            {
                suggestionPopup.Hide();
            }
            else
            {
                suggestionPopup.ShowSuggestions(suggestions, currentWord, caretPos, inputField);
            }
        }
        else
        {
            suggestionPopup.Hide();
        }
    }

    /// Returns the last word typed before the caret position.
    string GetLastWord(string input)
    {
        char[] separators = { ' ', '\n', '\t', '(', ')', ';' };
        string[] parts = input.Split(separators);

        string lastPart = parts.Length > 0 ? parts[parts.Length - 1] : "";

        int dotIndex = lastPart.LastIndexOf('.');
        if (dotIndex != -1)
            return lastPart.Substring(dotIndex + 1); // Return part after the dot
        else
            return lastPart;
    }
}
