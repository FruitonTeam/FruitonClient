using Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserBar : MonoBehaviour
{
    public Text PlayerNameText;
    public Image PlayerAvatarImage;

    void OnEnable()
    {
        PlayerNameText.text = GameManager.Instance.UserName;
        PlayerHelper.GetAvatar(GameManager.Instance.UserName,
            texture =>
            {
                PlayerAvatarImage.sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
            },
            error =>
            {
                Debug.LogWarning("Could not get user avatar.");
            });
    }
}