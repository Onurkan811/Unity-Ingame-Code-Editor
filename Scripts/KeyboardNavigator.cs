using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardNavigator : MonoBehaviour
{
    public SuggestionPopup suggestionPopup;

    void OnGUI()
    {
        // Only handle keyboard input if the suggestion popup is active
        if (suggestionPopup.gameObject.activeSelf)
        {
            Event e = Event.current;

            if (e.type == EventType.KeyDown)
            {
                // Navigate down through suggestions
                if (e.keyCode == KeyCode.DownArrow)
                {
                    suggestionPopup.SelectNext();
                    e.Use(); // prevent default handling
                }
                // Navigate up through suggestions
                else if (e.keyCode == KeyCode.UpArrow)
                {
                    suggestionPopup.SelectPrevious();
                    e.Use();
                }
                // Confirm selection with Enter or Tab
                else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Tab)
                {
                    suggestionPopup.ConfirmSelection();
                    e.Use();
                }
                // Close the popup with Escape
                else if (e.keyCode == KeyCode.Escape)
                {
                    suggestionPopup.Hide();
                    e.Use();
                }
            }
        }
    }
}
