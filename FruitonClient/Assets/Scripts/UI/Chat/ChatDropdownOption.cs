using System.Linq;
using UI.Chat;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script attached to dropdown options in chat
/// used for disabling "challenge" option when selected friend isn't in main menu
/// </summary>
public class ChatDropdownOption : MonoBehaviour {

    public const int SHOW_PROFILE = 0;
    public const int CHALLENGE = 1;
    public const int OFFER_FRUITON = 2;
    public const int DELETE_FRIEND =  3;
    public const int CANCEL = 4;
    
	void Start ()
    {
        // we need to add -1 to sibling index, since on index 0 there's disabled item template game object
        int optionIndex = transform.GetSiblingIndex() - 1;
        if (optionIndex == CHALLENGE)
        {
            GetComponent<Toggle>().interactable = ChatController.Instance.IsSelectedPlayerInMenu 
                && !amIChallengedBy(ChatController.Instance.SelectedPlayerLogin);
        } 
        else if (optionIndex == OFFER_FRUITON)
        {
            GetComponent<Toggle>().interactable = ChatController.Instance.IsSelectedPlayerOnline;
        }
    }

    private bool amIChallengedBy(string login)
    {
        return ChallengeController.Instance.EnemyChallenges.Select(data => data.Challenge.ChallengeFrom).Contains(login);
    }
}
