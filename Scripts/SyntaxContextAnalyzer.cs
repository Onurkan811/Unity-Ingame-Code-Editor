using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// Analyzes the context of the current input and provides relevant code suggestions.
public class SyntaxContextAnalyzer : MonoBehaviour
{
    public TMP_InputField inputField;

    private Dictionary<string, List<string>> contextSuggestions = new Dictionary<string, List<string>>();
    private List<string> defaultSuggestions = new List<string>();

    void Awake()
    {
        LoadSuggestionsFromJson();

        // Enable character validation to support auto pair insertion
        if (inputField != null)
        {
            inputField.onValidateInput += HandleValidateInput;
        }
    }

    /// Loads suggestion data from the JSON file in Resources folder.
    void LoadSuggestionsFromJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("suggestions");
        if (jsonFile != null)
        {
            SuggestionData data = JsonUtility.FromJson<SuggestionData>(jsonFile.text);

            contextSuggestions = new Dictionary<string, List<string>>();
            foreach (var entry in data.contextSuggestions)
            {
                string lowerKey = entry.key.ToLower();
                contextSuggestions[lowerKey] = entry.values;
            }

            defaultSuggestions = data.defaultSuggestions;
        }
        else
        {
            Debug.LogError("suggestions.json not found in Resources!");
        }
    }

    /// Returns a list of suggestions for the current word and context.
    public List<string> GetSuggestionsForContext(string fullText, int caretPos, string currentWord)
    {
        string contextKey = GetContextKey(fullText, caretPos);
        currentWord = currentWord.ToLower();

        if (!string.IsNullOrEmpty(contextKey) && contextSuggestions.ContainsKey(contextKey))
        {
            var list = contextSuggestions[contextKey].FindAll(s => s.ToLower().StartsWith(currentWord));
            return list;
        }

        var defaultList = defaultSuggestions.FindAll(s => s.ToLower().StartsWith(currentWord));
        return defaultList;
    }

    /// Extracts the context key from the text before the caret (usually a class or variable name).
    string GetContextKey(string text, int caretPos)
    {
        if (string.IsNullOrEmpty(text) || caretPos == 0)
            return "";

        string beforeCaret = text.Substring(0, caretPos);
        int dotIndex = beforeCaret.LastIndexOf('.');
        if (dotIndex <= 0)
            return "";

        int i = dotIndex - 1;
        while (i >= 0 && (char.IsLetterOrDigit(beforeCaret[i]) || beforeCaret[i] == '_'))
        {
            i--;
        }

        string key = beforeCaret.Substring(i + 1, dotIndex - i).ToLower();
        return key;
    }

    /// Applies the selected suggestion into the input field.
    /// Some keywords (like 'if', 'for') are expanded as code snippets.
    public void ApplySuggestion(string selected)
    {
        int pos = inputField.caretPosition;
        string insertText = selected;

        switch (selected)
        {
            case "if":
                insertText = "if () {\n\t\n}";
                inputField.text = inputField.text.Insert(pos, insertText);
                inputField.caretPosition = pos + 4;
                break;

            case "for":
                insertText = "for (int i = 0; i < ; i++) {\n\t\n}";
                inputField.text = inputField.text.Insert(pos, insertText);
                inputField.caretPosition = pos + 14;
                break;

            case "while":
                insertText = "while () {\n\t\n}";
                inputField.text = inputField.text.Insert(pos, insertText);
                inputField.caretPosition = pos + 7;
                break;

            default:
                inputField.text = inputField.text.Insert(pos, selected);
                inputField.caretPosition = pos + selected.Length;
                break;
        }

        inputField.ActivateInputField(); // Keep focus after inserting
    }

    /// Validates input to auto-insert matching pairs (like (), {}, "", etc.).
    private char HandleValidateInput(string text, int charIndex, char addedChar)
    {
        char pairChar = '\0';

        switch (addedChar)
        {
            case '(': pairChar = ')'; break;
            case '{': pairChar = '}'; break;
            case '[': pairChar = ']'; break;
            case '"': pairChar = '"'; break;
            case '\'': pairChar = '\''; break;
        }

        if (pairChar != '\0')
        {
            StartCoroutine(InsertPairAfterFrame(pairChar));
        }

        return addedChar;
    }

    /// Inserts the matching pair character in the next frame to preserve correct caret behavior.
    private IEnumerator InsertPairAfterFrame(char pairChar)
    {
        yield return null;

        int caretPos = inputField.caretPosition;
        if (caretPos >= 0 && caretPos <= inputField.text.Length)
        {
            inputField.text = inputField.text.Insert(caretPos, pairChar.ToString());
            inputField.caretPosition = caretPos;
            inputField.selectionAnchorPosition = caretPos;
            inputField.selectionFocusPosition = caretPos;
        }
    }
}
