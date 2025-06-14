using System.Collections.Generic;
using UnityEngine;

/// Holds suggestion data used for autocomplete functionality.
/// Includes context-aware suggestions and default suggestions.
[System.Serializable]
public class SuggestionData
{
    public List<ContextEntry> contextSuggestions;

    public List<string> defaultSuggestions;
}

/// Represents a context entry for suggestions.
/// Each entry maps a key to a list of related suggestion values.
[System.Serializable]
public class ContextEntry
{
    public string key;


    public List<string> values;
}
