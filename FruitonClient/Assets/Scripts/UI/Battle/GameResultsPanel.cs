using System;
using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;
using Util;
using WebSocketSharp;

namespace UI.Battle
{
    public class GameResultsPanel : MessagePanel
    {
        public Text ReasonText;
        public GameObject RewardsWrapper;
        public GameObject RewardMoney;
        public GameObject RewardFrutions;
        public Text QuestsTitleText;
        public Text QuestsText;

        public void ShowResult(GameOver gameOver, bool isLocalDuel = false)
        {
            OnClose(() => Scenes.Load(Scenes.MAIN_MENU_SCENE));

            var rewards = gameOver.GameRewards;
            if (isLocalDuel)
            {
                if (gameOver.WinnerLogin.IsNullOrEmpty())
                {
                    ShowErrorMessage("Defeat");
                }
                else
                {
                    ShowInfoMessage(gameOver.WinnerLogin + " wins!");
                }
            }
            else if (gameOver.WinnerLogin == GameManager.Instance.UserName)
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

            RewardsWrapper.SetActive(rewards.UnlockedFruitons.Count > 0 || rewards.Money > 0);
            if (rewards.Money > 0)
            {
                RewardMoney.GetComponentInChildren<Text>().text = rewards.Money.ToString();
            }
            else
            {
                RewardMoney.SetActive(false);
            }
            if (rewards.UnlockedFruitons.Count > 0)
            {
                RewardFrutions.GetComponentInChildren<Text>().text = string.Join(", ",
                    rewards.UnlockedFruitons.Select(KernelUtils.GetFruitonName)
                        .ToArray());
            }
            else
            {
                RewardFrutions.SetActive(false);
            }

            var completedAnyQuests = rewards.Quests.Count > 0;
            QuestsTitleText.gameObject.SetActive(completedAnyQuests);
            QuestsText.gameObject.SetActive(completedAnyQuests);
            if (completedAnyQuests)
            {
                QuestsTitleText.text = rewards.Quests.Count == 1 ? "Completed quest:" : "Completed quests:";
                QuestsText.text = string.Join(Environment.NewLine, rewards.Quests.Select(q => q.Name).ToArray());
            }

            if (!GameManager.Instance.IsOnline) return;

            GameManager.Instance.AdjustMoney(rewards.Money);
            GameManager.Instance.UnlockFruitons(rewards.UnlockedFruitons);

            if (rewards.Quests != null)
            {
                foreach (Quest q in rewards.Quests)
                {
                    GameManager.Instance.AdjustMoney(q.Reward.Money);
                }
            }
        }
    }
}