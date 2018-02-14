using System;
using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;

static class BattleUIUtil
{
    public static void ShowResults(MessagePanel gameOverPanel, GameOver gameOver)
    {
        gameOverPanel.gameObject.SetActive(true);
        gameOverPanel.OnClose(() => Scenes.Load(Scenes.MAIN_MENU_SCENE));
        gameOverPanel.ShowInfoMessage(
            "Game over: " + gameOver.Reason + Environment.NewLine +
            "Money gain: " + gameOver.Results.Money + Environment.NewLine +
            "Unlocked fruitons: " + gameOver.Results.UnlockedFruitons + Environment.NewLine +
            "Unlocked quests: " + string.Join(",",
                gameOver.Results.Quests.Select(q => q.Name).ToArray())
        );

        GameManager.Instance.AddMoney(gameOver.Results.Money);
        GameManager.Instance.UnlockFruitons(gameOver.Results.UnlockedFruitons);

        if (gameOver.Results.Quests != null)
        {
            foreach (Quest q in gameOver.Results.Quests)
            {
                GameManager.Instance.AddMoney(q.Reward.Money);
            }
        }
    }
}

