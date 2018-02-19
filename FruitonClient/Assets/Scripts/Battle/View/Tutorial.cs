using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.dataStructures;
using fruiton.kernel;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Action = System.Action;

public class Tutorial
{
    private bool isInitialized = false;

    private BattleViewer battleViewer;
    private Queue<char> textToType;
    private Queue<TutorialStage> stages;
    private TutorialStage currentStage;
    private float elapsedTime;

    public Tutorial(BattleViewer battleViewer)
    {
        this.battleViewer = battleViewer;
        textToType = new Queue<char>();
        battleViewer.TutorialContinueButton.onClick.AddListener(EndAndNextStage);
        
        battleViewer.InfoAndroidButton.enabled = false;
        battleViewer.EndTurnButton.enabled = false;
        battleViewer.SurrendButton.enabled = false;
        battleViewer.TimeCounter.gameObject.SetActive(false);
    }

    private void CreateTutorialStages()
    {
        string clickTapString = "clicking on";
        string clickAnywhere = "click anywhere";
#if UNITY_ANDROID
        clickTapString = "tapping";
        clickAnywhere = "tap the screen";
#endif
        string username = GameManager.Instance.UserName;

        stages = new Queue<TutorialStage>();

        string text = "Hi " + username + "! Welcome to Fruitons! Let me quickly introduce you to the basics." +
            " Whenever I become too boring just " + clickAnywhere + " to load the text instantly.";
        stages.Enqueue(new TutorialStage(text));

        text = "These are your Fruitons. Take a good care of them, they will fight and even die for you.";
        Func<ClientFruiton, bool> isMine = clientFruiton => clientFruiton.KernelFruiton.owner.id == ((AIBattle)battleViewer.battle).HumanPlayer.ID;
        stages.Enqueue(new TutorialStage(text, () => FilterFruitons(false, isMine)));

        text = "These are the Fruitons of your enemy. Beware of them, they might appear innocent but their hearts are full of rage and bloodlust.";
        Func<ClientFruiton, bool> isOpponent = clientFruiton => clientFruiton.KernelFruiton.owner.id == ((AIBattle)battleViewer.battle).AiPlayer.ID;
        stages.Enqueue(new TutorialStage(text, () => FilterFruitons(false, isOpponent)));

        text = "What you see now are kings. Protect your king by all means because at the moment one of the king dies, its owner loses and the other player wins.";
        Func<ClientFruiton, bool> isKing = clientFruiton => clientFruiton.KernelFruiton.type == (int)FruitonType.KING;
        stages.Enqueue(new TutorialStage(text, () => FilterFruitons(false, isKing)));

        text = "There are 4 major pieces, 2 to the left of the king and 2 to the right. They use to be strong warriors and have special abilities.";
        Func<ClientFruiton, bool> isMajor = clientFruiton => clientFruiton.KernelFruiton.type == (int)FruitonType.MAJOR;
        stages.Enqueue(new TutorialStage(text, () => FilterFruitons(false, isMajor)));

        text = "Don't ever underestimate the front line, the minor pieces. They may appear to be weak cowards but without" +
               " them your king would be exposed directly to the wrath of your enemy.";
        Func<ClientFruiton, bool> isMinor = clientFruiton => clientFruiton.KernelFruiton.type == (int)FruitonType.MINOR;
        stages.Enqueue(new TutorialStage(text, () => FilterFruitons(false, isMinor)));

        text = "The red circles under the Fruitons show their health points (HP), a Fruiton only lives while it has at least 1 HP.";
        Func<ClientFruiton, GameObject> getHealthTag = clientFruiton => clientFruiton.HealthTag.gameObject.transform.parent.gameObject;
        stages.Enqueue(new TutorialStage(text, () => GetObjectsRelativeToFruitons(getHealthTag)));

        text = "On the contrary, the yellow circles show how much damage a Fruiton causes when attacking an enemy Fruiton.";
        Func<ClientFruiton, GameObject> getDamageTag = clientFruiton => clientFruiton.DamageTag.gameObject.transform.parent.gameObject;
        stages.Enqueue(new TutorialStage(text, () => GetObjectsRelativeToFruitons(getDamageTag)));

        text = "And there it is, the decription. You can find there everything important such as special abilities or effects.";
        TutorialStage descriptionStage = new TutorialStage(
            text,
            () => new List<GameObject> { battleViewer.FruitonInfoPanel },
            endActions: new List<Action> { () => battleViewer.FruitonInfoPanel.SetActive(false) },
            scalingRate: 0.2f);

#if UNITY_ANDROID
        text = "To see the detailed decription of a fruiton, switch to the \"Info mode\" first.";
        stages.Enqueue(new TutorialStage(
            text,
            () => new List<GameObject> { battleViewer.InfoAndroidButton.gameObject },
            endCondition: TutorialStage.StageEndCondition.DifferentButtonClick,
            startActions:new List<Action>{() => battleViewer.InfoAndroidButton.enabled = true},
            endActions:new List<Action>{ () => battleViewer.InfoAndroidButton.enabled = false }));

        text = "Now you are in the \"Info mode\". To see the detailed decription of a fruiton, tap the tile he is standing on.";
        stages.Enqueue(new TutorialStage(
            text,
            () => FilterFruitons(true),
            endCondition: TutorialStage.StageEndCondition.LeftClickHighlighted,
            scaleHighlight: false));

        stages.Enqueue(descriptionStage);

        text = "Switch the \"Info mode\" off now.";
        stages.Enqueue(new TutorialStage(
            text,
            () => new List<GameObject> { battleViewer.InfoAndroidButton.gameObject },
            endCondition: TutorialStage.StageEndCondition.DifferentButtonClick,
            startActions:new List<Action>{() => battleViewer.InfoAndroidButton.enabled = true},
            endActions:new List<Action>{ () => battleViewer.InfoAndroidButton.enabled = false }));
#else
        text = "To see the detailed decription of a fruiton, move the mouse over the tile he is standing on.";
        stages.Enqueue(new TutorialStage(
            text,
            () => FilterFruitons(true),
            endCondition: TutorialStage.StageEndCondition.HoverOverHighlighted,
            scaleHighlight: false));

        stages.Enqueue(descriptionStage);
#endif

        text = "This is Hourglass button used to end your turn.";
        stages.Enqueue(new TutorialStage(
            text,
            () => new List<GameObject> { battleViewer.EndTurnButton.gameObject }));

        text = "The timer shows you how many seconds you have to complete your turn. If you can't make it, " +
               "your turn will be ended automatically when the timer reaches 0. However, don't worry about it in this tutorial.";
        stages.Enqueue(new TutorialStage(
            text,
            () => new List<GameObject> { battleViewer.TimeCounter.transform.parent.gameObject },
            startActions:new List<Action> {() => battleViewer.TimeCounter.gameObject.SetActive(true)},
            updateActions: new List<Action> { battleViewer.UpdateTimer },
            endActions: new List<Action> { () => battleViewer.TimeCounter.gameObject.SetActive(false)}));

        text = "And the (highly unrecommended!) surrender button. Fight until your last breath!";
        stages.Enqueue(new TutorialStage(
            text,
            () => new List<GameObject> { battleViewer.SurrendButton.gameObject }));

        text = "Well, enough with this jibber-jabber. Let's play! You can choose a fruiton by " + clickTapString +
               " the tile he is standing on. Try this one.";
        stages.Enqueue(new TutorialStage(
            text,
            () => new List<GameObject> { battleViewer.GridLayoutManager.GetTile(2, 0) },
            endCondition:TutorialStage.StageEndCondition.LeftClickHighlighted,
            scaleHighlight:false));

        text =
            "Oh look, everything turned into the rainbow. Lovely, isn't it? In each turn you can perform at most 2 " +
            "actions with a single fruiton. You can move and attack/heal or just one of these. Remember that you" +
            " can't move after you attack/heal.";
        stages.Enqueue(new TutorialStage(text));

        text = "Blue tiles are the ones where you can move by " + clickTapString + " them.";
        stages.Enqueue(new TutorialStage(
            text,
            () => battleViewer.GridLayoutManager.GetMovementTiles(),
            scaleHighlight:false));

        text = "These are the obstacles. No one is able to step on an obstacle.";
        stages.Enqueue(new TutorialStage(
            text,
            () => battleViewer.GridLayoutManager.Obstacles,
            scaleHighlight: false));

        text = "Yellow tiles are a little bit tricky. They mark your possible attack locations in this turn, " +
               "if you move close enough to them.";
        stages.Enqueue(new TutorialStage(
            text,
            () => battleViewer.GridLayoutManager.GetTentativeAttacks(),
            scaleHighlight: false));

        text = "Let's try moving here with the selected fruiton.";
        stages.Enqueue(new TutorialStage(
            text,
            () => new List<GameObject> { battleViewer.GridLayoutManager.GetTile(4, 7) },
            endCondition: TutorialStage.StageEndCondition.LeftClickHighlighted,
            scaleHighlight: false));

        TutorialPlayer opponent = (TutorialPlayer) ((AIBattle)battleViewer.battle).AiPlayer;

        text = "Well done! After you move a fruiton, he becomes the only fruiton that can attack/heal that turn." +
               " The remaining actions are automatically highlighted. You can attack fruitons on the red tiles." +
               " Attack the one in the middle.";
        stages.Enqueue(new TutorialStage(
            text,
            () => new List<GameObject> { battleViewer.GridLayoutManager.GetTile(4, 8) },
            endCondition: TutorialStage.StageEndCondition.LeftClickHighlighted,
            scaleHighlight: false,
            endActions: new List<Action>{() => opponent.MakeMove()}));

        text = "You have no more actions available in this turn. Press the Hourglass button to end it and let your opponent play.";
        stages.Enqueue(new TutorialStage(
            text,
            () => new List<GameObject> { battleViewer.EndTurnButton.gameObject },
            endCondition: TutorialStage.StageEndCondition.DifferentButtonClick,
            startActions: new List<Action> { () => battleViewer.EndTurnButton.enabled = true },
            endActions: new List<Action> { () => battleViewer.EndTurnButton.enabled = false }
            ));

        Func<ClientFruiton, bool> isHealer = clientFruiton => clientFruiton.KernelFruiton.currentAttributes.heal > 0;

        text = "Ouch, your Fruiton was hit. Let's fix this, select a healer.";
        stages.Enqueue(new TutorialStage(
            text,
            () => FilterFruitons(true, isMine, isHealer),
            endCondition: TutorialStage.StageEndCondition.LeftClickHighlighted,
            scaleHighlight: false
        ));

        text = "Green tiles mark fruitons that can be healed. Heal the injured one now.";
        stages.Enqueue(new TutorialStage(
            text,
            () => new List<GameObject> { battleViewer.GridLayoutManager.GetTile(4, 7) },
            endCondition: TutorialStage.StageEndCondition.LeftClickHighlighted,
            scaleHighlight: false));

        stages.Enqueue(new TutorialStage("You did well, " + username + ", it's time for real challenges now! "));
        NextStage();
    }

    /// <summary>
    /// Iterate through all the Fruitons on the board and return a list of objects that <paramref name="filter"/> returned when applied on each of Fruitons.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    private List<GameObject> GetObjectsRelativeToFruitons(Func<ClientFruiton, GameObject> filter)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (GameObject gameObject in battleViewer.FruitonsGrid)
        {
            if (gameObject == null) continue;
            ClientFruiton kernelFruiton = gameObject.GetComponent<ClientFruiton>();
            result.Add(filter(kernelFruiton));
        }
        return result;
    }

    /// <summary>
    /// Iterate through all the fruitons on the board and return a list of fruitons/tiles (according to <param name="returnTiles"/>)
    /// that satisfies all the conditions given by <param name="filters"/>
    /// </summary>
    /// <returns></returns>
    private List<GameObject> FilterFruitons(bool returnTiles, params Func<ClientFruiton, bool>[] filters)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (GameObject gameObject in battleViewer.FruitonsGrid)
        {
            if (gameObject == null) continue;
            ClientFruiton clientFruiton = gameObject.GetComponent<ClientFruiton>();
            bool satisfiesAllConditions = filters.All(f => f(clientFruiton));
            if (satisfiesAllConditions)
            {
                if (returnTiles)
                {
                    Point position = clientFruiton.KernelFruiton.position;
                    result.Add(battleViewer.GridLayoutManager.GetTile(position.x, position.y));
                }
                else
                {
                    result.Add(gameObject);
                }
            }
        }
        return result;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Scenes.Load(Scenes.FRACTION_SCENE);
        }
        if (!isInitialized)
        {
            foreach (GameObject gameObject in battleViewer.FruitonsGrid)
            {
                if (gameObject == null) continue;
                ClientFruiton clientFruiton = gameObject.GetComponent<ClientFruiton>();
                if (!clientFruiton.IsInitialized) return;
            }
            CreateTutorialStages();
            isInitialized = true;
            return;
        }
        foreach (Action action in currentStage.UpdateActions)
        {
            action();
        }
        // Check if the typing is finished.
        if (TypeChars())
        {
            switch (currentStage.EndCondition)
            {
                case TutorialStage.StageEndCondition.LeftClickHighlighted:
                    WaitForLeftClick();
                    break;
                case TutorialStage.StageEndCondition.ButtonContinueClick:
                    battleViewer.TutorialContinueButton.gameObject.SetActive(true);
                    break;
                case TutorialStage.StageEndCondition.DifferentButtonClick:
                    break;
                case TutorialStage.StageEndCondition.HoverOverHighlighted:
                    WaitForHover();
                    break;
                default:
                    throw  new ArgumentOutOfRangeException();
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            FinishTypingInstantly();
        }
    }

    /// <summary>
    /// Write all the remaining text at once.
    /// </summary>
    private void FinishTypingInstantly()
    {
        StringBuilder stringBuilder = new StringBuilder();
        while (textToType.Count > 0)
        {
            stringBuilder.Append(textToType.Dequeue());
        }
        var textComponent = battleViewer.TutorialPanel.GetComponentInChildren<Text>();
        textComponent.text += stringBuilder.ToString();
    }

    private void WaitForHover()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] raycastHits = Physics.RaycastAll(ray);
        if (raycastHits.Any(hit => currentStage.GetHighlightedObjects().Contains(hit.transform.gameObject)))
        {
            EndStage();
            battleViewer.HoverLogic();
            NextStage();
        }
    }

    private void WaitForLeftClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] raycastHits = Physics.RaycastAll(ray);
            if (raycastHits.Any(hit => currentStage.GetHighlightedObjects().Contains(hit.transform.gameObject)))
            {
                EndStage();
                battleViewer.HandleLeftButtonUp();
                NextStage();
            }
        }
    }

    public void EndAndNextStage()
    {
        EndStage();
        NextStage();
        battleViewer.TutorialContinueButton.gameObject.SetActive(false);
    }

    private void NextStage()
    {
        if (stages.Count == 0)
        {
            FinishTutorial();
            return;
        }
        currentStage = stages.Dequeue();
        foreach (Action action in currentStage.StartActions)
        {
            action();
        }
        EnqueueString(currentStage.Text);
    }

    private void FinishTutorial()
    {
        if (GameManager.Instance.IsOnline && GameManager.Instance.Fraction == Fraction.None)
        {
            Scenes.Load(Scenes.FRACTION_SCENE);
        }
        else
        {
            Scenes.Load(Scenes.MAIN_MENU_SCENE);
        }
    }

    private void EndStage()
    {
        foreach (Action action in currentStage.EndActions)
        {
            action();
        }
    }

    /// <returns> Typing finished. </returns>
    private bool TypeChars()
    {
        if (textToType.Count == 0)
        {
            return true;
        }
        elapsedTime += Time.deltaTime;
        if (elapsedTime > 0.02f)
        {
            elapsedTime = 0;
            var nextChar = textToType.Dequeue();
            var textComponent = battleViewer.TutorialPanel.GetComponentInChildren<Text>();
            textComponent.text += nextChar;
        }
        return false;
    }

    /// <summary>
    /// Enqueue <paramref name="text"/> char by char to be able to write it char by char.
    /// </summary>
    /// <param name="text"></param>
    private void EnqueueString(string text)
    {
        var textComponent = battleViewer.TutorialPanel.GetComponentInChildren<Text>();
        textComponent.text = "";
        foreach (char c in text)
        {
            textToType.Enqueue(c);
        }
    }

    

}
