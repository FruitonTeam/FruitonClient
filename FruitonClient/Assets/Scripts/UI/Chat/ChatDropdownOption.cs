using UI.Chat;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script attached to dropdown options in chat
/// used for disabling "challenge" option when selected friend isn't in main menu
/// </summary>
public class ChatDropdownOption : MonoBehaviour {

	void Start ()
    {
        // if the option is "challenge" we enable it only if currently selected friend is in main menu
        // we need to add +1 to sibling index, since on index 0 there's disabled item template game object
        if (transform.GetSiblingIndex() == ChatController.DROPDOWN_CHALLENGE+1)
        {
            GetComponent<Toggle>().interactable = ChatController.Instance.IsSelectedPlayerInMenu;
        }
	}
}
