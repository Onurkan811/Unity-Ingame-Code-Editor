using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SyntaxHighlighter : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI highlightedText;

    private Dictionary<string, string> syntaxColors = new Dictionary<string, string>();
    private bool isUpdatingText = false;

    void Start()
    {
        if (inputField == null || highlightedText == null)
            return;

        // Hide the original input text by making it transparent
        inputField.textComponent.color = new Color(0, 0, 0, 0);

        // Initialize highlighted text display
        highlightedText.text = "";
        highlightedText.enableWordWrapping = false;
        highlightedText.overflowMode = TextOverflowModes.Overflow;

        LoadKeywordsFromJson();
        inputField.onValueChanged.AddListener(UpdateHighlightedText);
    }

    // Load keyword and function definitions with their highlight colors from a JSON file
    void LoadKeywordsFromJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("keywords");
        if (jsonFile == null)
            return;

        KeywordData data = JsonUtility.FromJson<KeywordData>(jsonFile.text);
        if (data == null || data.keywords == null || data.functions == null)
            return;

        foreach (var keyword in data.keywords)
            syntaxColors[keyword.key] = keyword.color;

        foreach (var function in data.functions)
            syntaxColors[function.key] = function.color;
    }

    // Called when the input field's value changes
    public void UpdateHighlightedText(string text)
    {
        if (isUpdatingText)
            return;

        isUpdatingText = true;
        highlightedText.text = ApplySyntaxHighlightingLogic(text);
        isUpdatingText = false;
    }

    // Apply color tags to known keywords/functions
    private string ApplySyntaxHighlightingLogic(string text)
    {
        // Remove existing color tags
        text = Regex.Replace(text, "<color=[^>]*>(.*?)</color>", "$1");

        // Add new color tags for keywords/functions
        foreach (var keyword in syntaxColors)
        {
            string colorTag = $"<color={keyword.Value}>{keyword.Key}</color>";
            text = Regex.Replace(text, $"\\b{Regex.Escape(keyword.Key)}\\b", colorTag);
        }

        return text;
    }

    // Internal class for loading JSON data
    [System.Serializable]
    private class KeywordData
    {
        [System.Serializable]
        public class Keyword
        {
            public string key;     // e.g. "if", "while"
            public string color;   // e.g. "#ff0000"
        }

        [System.Serializable]
        public class Function
        {
            public string key;     // e.g. "Debug.Log"
            public string color;   // e.g. "#00ff00"
        }

        public List<Keyword> keywords;
        public List<Function> functions;
    }
}
