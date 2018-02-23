using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.fruitDb;
using fruiton.fruitDb.factories;
using UnityEngine;
using UnityEngine.UI;

public class GameResultsPanel : MessagePanel
{
    public Text RewardText;
    public Text ReasonText;
    public GameObject RewardMoney;
    public GameObject RewardFrutions;
    public Text QuestsTitleText;
    public Text QuestsText;

    public void ShowResult(GameOver gameOver)
    {
        OnClose(() => Scenes.Load(Scenes.MAIN_MENU_SCENE));

        var results = gameOver.Results;
        // TODO use name of the winner from protobufs once it is added there
        if (results.Money > 0 || gameOver.Reason == GameOver.Types.Reason.Disconnect || gameOver.Reason == GameOver.Types.Reason.Surrender)
        {
            ShowInfoMessage("Victory");
        }
        else
        {
            ShowErrorMessage("Defeat");
        }

        ReasonText.gameObject.SetActive(true);
        switch (gameOver.Reason)
        {
            case GameOver.Types.Reason.Disconnect:
                ReasonText.text = "Your opponent has disconnected";
                break;
            case GameOver.Types.Reason.Surrender:
                ReasonText.text = "Your opponent has surrendered";
                break;
            default:
                ReasonText.gameObject.SetActive(false);
                break;
        }

        RewardMoney.SetActive(false);
        RewardFrutions.SetActive(false);
        if (results.UnlockedFruitons.Count == 0 && results.Money == 0)
        {
            RewardText.text = "No rewards";
        }
        else
        {
            RewardText.text = "Rewards:";
            if (results.Money > 0)
            {
                RewardMoney.SetActive(true);
                RewardMoney.GetComponentInChildren<Text>().text = results.Money.ToString();
            }
            if (results.UnlockedFruitons.Count > 0)
            {
                RewardFrutions.SetActive(true);
                RewardFrutions.GetComponentInChildren<Text>().text = string.Join(", ",
                    gameOver.Results.UnlockedFruitons.Select(id => FruitonFactory
                            .makeFruiton(id, GameManager.Instance.FruitonDatabase).name)
                        .ToArray());
            }
        }

        var gotNewQuests = results.Quests.Count > 0;
        QuestsTitleText.gameObject.SetActive(gotNewQuests);
        QuestsText.gameObject.SetActive(gotNewQuests);
        if (gotNewQuests)
        {
            QuestsTitleText.text = results.Quests.Count == 1 ? "Completed quest:" : "Completed quests:";
            QuestsText.text = string.Join(Environment.NewLine, results.Quests.Select(q => q.Name).ToArray());
        }

        if (!GameManager.Instance.IsOnline) return;

        GameManager.Instance.AddMoney(results.Money);
        GameManager.Instance.UnlockFruitons(results.UnlockedFruitons);

        if (results.Quests != null)
        {
            foreach (Quest q in results.Quests)
            {
                GameManager.Instance.AddMoney(q.Reward.Money);
            }
        }
    }
}