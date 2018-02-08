using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using fruiton.dataStructures;
using fruiton.kernel;
using UnityEngine;
using UnityEngine.UI;

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
    }

    private void CreateTutorialStages()
    {
        string username = GameManager.Instance.UserName;

        stages = new Queue<TutorialStage>();
        stages.Enqueue(new TutorialStage("Hi " + username + ". Welcome to Fruitons! Let me quickly introduce you to the basics."));

        //Func<ClientFruiton, bool> isMine = clientFruiton => clientFruiton.KernelFruiton.owner.id == ((AIBattle)battleViewer.battle).HumanPlayer.ID;
        //stages.Enqueue(new TutorialStage("These are your Fruitons. Take a good care of them, they will fight and even die for you.", () => FilterFruitons(isMine)));

        //Func<ClientFruiton, bool> isOpponent = clientFruiton => clientFruiton.KernelFruiton.owner.id == ((AIBattle)battleViewer.battle).AiPlayer.ID;
        //stages.Enqueue(new TutorialStage("These are the Fruitons of your enemy. Beware of them, they might appear innocent but their hearts are full of rage and bloodlust. ", () => FilterFruitons(isOpponent)));

        //Func<ClientFruiton, bool> isKing = clientFruiton => clientFruiton.KernelFruiton.type == (int)FruitonType.KING;
        //stages.Enqueue(new TutorialStage("What you see now are kings. Protect your king by all means because at the moment one of the king dies, its owner loses and the other player wins. ", () => FilterFruitons(isKing)));

        //Func<ClientFruiton, bool> isMajor = clientFruiton => clientFruiton.KernelFruiton.type == (int)FruitonType.MAJOR;
        //stages.Enqueue(new TutorialStage("There are 4 major pieces, 2 to the left of the king and 2 to the right. They use to be strong warriors and have special abilities. ", () => FilterFruitons(isMajor)));

        //Func<ClientFruiton, bool> isMinor = clientFruiton => clientFruiton.KernelFruiton.type == (int)FruitonType.MINOR;
        //stages.Enqueue(new TutorialStage("Don't ever underestimate the front line, the minor pieces. They may appear to be weak cowards but without them your king would be exposed directly to the wrath of your enemy. ", () => FilterFruitons(isMinor)));

        //Func<ClientFruiton, GameObject> getHealthTag = clientFruiton => clientFruiton.HealthTag.gameObject.transform.parent.gameObject;
        //stages.Enqueue(new TutorialStage("The red circles under the Fruitons show their health points (HP), a Fruiton only lives while it has at least 1 HP. ", () => GetObjectsRelativeToFruitons(getHealthTag)));

        //Func<ClientFruiton, GameObject> getDamageTag = clientFruiton => clientFruiton.DamageTag.gameObject.transform.parent.gameObject;
        //stages.Enqueue(new TutorialStage("On the contrary, the yellow circles show how much damage a Fruiton causes when attacking an enemy Fruiton. ", () => GetObjectsRelativeToFruitons(getDamageTag)));

        //stages.Enqueue(new TutorialStage(
        //    "Use Hourglass button to end your turn. ",
        //    () => new List<GameObject> {battleViewer.EndTurnButton.gameObject}));

        //stages.Enqueue(new TutorialStage(
        //    "The timer shows you how many seconds do you have to complete your turn. If you can't make it, your turn will be ended automatically when the timer reaches 0. However, don't worry about it in this tutorial. ",
        //    () => new List<GameObject> {battleViewer.TimeCounter.transform.parent.gameObject},
        //    updateActions: new List<Action> {battleViewer.UpdateTimer},
        //    endActions: new List<Action> {() => battleViewer.TimeCounter.text = ""}));

        //stages.Enqueue(new TutorialStage(
        //    "Highly unrecommended surrender button. Fight until your last breath! ",
        //    () => new List<GameObject> { battleViewer.SurrendButton.gameObject }));

        Point playedPosition = new Point(2, 0);
        GameObject playedFruiton = battleViewer.Grid[playedPosition.x, playedPosition.y];

        stages.Enqueue(new TutorialStage(
            "Well, enough of this jibber-jabber. Let's play! You can choose a fruiton by clicking on it, try this one. ",
            () => new List<GameObject> { playedFruiton },
            endCondition:TutorialStage.StageEndCondition.LeftClickHighlighted));

        
        stages.Enqueue(new TutorialStage(
            "Oh look, everything turned into a rainbow. Lovely, isn't it?  "));

        stages.Enqueue(new TutorialStage(
            "Blue ",
            () => battleViewer.GridLayoutManager.GetMovementTiles(),
            scaleHighlight:false,
            colorHighlight:true));


        stages.Enqueue(new TutorialStage("You did well, " + username + ", it's time for the real challenges now! "));
        NextStage();
    }

    private List<GameObject> GetObjectsRelativeToFruitons(Func<ClientFruiton, GameObject> filter)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (GameObject gameObject in battleViewer.Grid)
        {
            if (gameObject == null) continue;
            ClientFruiton kernelFruiton = gameObject.GetComponent<ClientFruiton>();
            result.Add(filter(kernelFruiton));
        }
        return result;
    }

    private List<GameObject> FilterFruitons(params Func<ClientFruiton, bool>[] filters)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (GameObject gameObject in battleViewer.Grid)
        {
            if (gameObject == null) continue;
            ClientFruiton clientFruiton = gameObject.GetComponent<ClientFruiton>();
            bool satisfiesAllConditions = filters.All(f => f(clientFruiton));
            if (satisfiesAllConditions)
            {
                result.Add(gameObject);
            }
        }
        return result;
    }

    public void Update()
    {
        if (!isInitialized)
        {
            foreach (GameObject gameObject in battleViewer.Grid)
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
        if (TypeChars())
        {
            switch (currentStage.EndCondition)
            {
                case TutorialStage.StageEndCondition.LeftClickAnywhere:
                    WaitForLeftClick(true);
                    break;
                case TutorialStage.StageEndCondition.LeftClickHighlighted:
                    WaitForLeftClick(false);
                    break;
                case TutorialStage.StageEndCondition.HoverHighlighted:
                    WaitForHover();
                    break;
                default:
                    throw  new ArgumentOutOfRangeException();
            }
        }
    }

    private void WaitForHover()
    {
        throw new NotImplementedException();
    }

    private void WaitForLeftClick(bool anywhere)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (anywhere)
            {
                NextStage();
            }
            else
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] raycastHits = Physics.RaycastAll(ray);
                if (raycastHits.Any(hit => currentStage.GetHighlightedObjects().Contains(hit.transform.gameObject)))
                {
                    NextStage();
                    if (currentStage.ForwardInputEvent)
                    {
                        battleViewer.LeftButtonUpLogic(raycastHits);
                    }
                }
            }
        }
    }

    private void NextStage()
    {
        if (currentStage != null)
        {
            foreach (Action action in currentStage.EndActions)
            {
                action();
            }
        }
        if (stages.Count == 0)
        {
            Scenes.Load(Scenes.MAIN_MENU_SCENE);
            return;
        }
        currentStage = stages.Dequeue();
        foreach (Action action in currentStage.StartActions)
        {
            action();
        }
        EnqueueString(currentStage.Text);
    }

    /// <returns> Typing finished. </returns>
    private bool TypeChars()
    {
        if (textToType.Count == 0)
        {
            return true;
        }
        elapsedTime += Time.deltaTime;
        if (elapsedTime > 0.0f)
        {
            elapsedTime = 0;
            var nextChar = textToType.Dequeue();
            var textComponent = battleViewer.TutorialPanel.GetComponentInChildren<Text>();
            textComponent.text += nextChar;
            elapsedTime = 0;
            if (textToType.Count == 0)
            {
                switch (currentStage.EndCondition)
                {
                    case TutorialStage.StageEndCondition.LeftClickAnywhere:
                        textComponent.text += " (CLICK ANYWHERE TO CONTINUE)";
                        break;
                }
            }
        }
        return false;
    }



    private void EnqueueString(string text)
    {
        var textComponent = battleViewer.TutorialPanel.GetComponentInChildren<Text>();
        textComponent.text = "";
        foreach (char c in text)
        {
            textToType.Enqueue(c);
        }
    }

    private class TutorialStage
    {
        public enum StageEndCondition { LeftClickAnywhere, LeftClickHighlighted, HoverHighlighted }

        private Dictionary<GameObject, Vector3> originalScales;
        private Dictionary<GameObject, Color> originalColors;
        // Scale attributes
        private float currentScale = 1;
        private bool decreasing = false;
        // Color attributes
        private float timer;
        private Color transparent = new Color(0, 0, 0, 0);

        public string Text;
        public Func<List<GameObject>> GetHighlightedObjects;
        public StageEndCondition EndCondition;
        public bool ErasePreviousText;
        public bool ForwardInputEvent;
        public List<Action> StartActions;
        public List<Action> UpdateActions;
        public List<Action> EndActions;

        public TutorialStage(string text,
            Func<List<GameObject>> highlightedObjects = null,
            List<Action> startActions = null,
            List<Action> updateActions = null,
            List<Action> endActions = null,
            StageEndCondition endCondition = StageEndCondition.LeftClickAnywhere,
            bool erasePreviousText = true,
            bool forwardInputEvent = true,
            bool scaleHighlight = true,
            bool colorHighlight = false)
        {
            Text = text;
            GetHighlightedObjects = highlightedObjects ?? (() => new List<GameObject>());
            StartActions = startActions ?? new List<Action>();
            UpdateActions = updateActions ?? new List<Action>();
            EndActions = endActions ?? new List<Action>();
            EndCondition = endCondition;
            ErasePreviousText = erasePreviousText;
            ForwardInputEvent = forwardInputEvent;

            if (scaleHighlight)
            {
                originalScales = new Dictionary<GameObject, Vector3>();
                StartActions.Add(InitializeOriginalScales);
                UpdateActions.Add(ScaleUpdate);
                EndActions.Add(ResetScales);
            }

            if (colorHighlight)
            {
                originalColors = new Dictionary<GameObject, Color>();
                StartActions.Add(InitializeOriginalColors);
                UpdateActions.Add(ColorUpdate);
                EndActions.Add(ResetColors);
            }
        }

        private void InitializeOriginalScales()
        {
            foreach (GameObject highlightedObject in GetHighlightedObjects())
            {
                originalScales[highlightedObject] = highlightedObject.transform.localScale;
            }
        }
       
        private void InitializeOriginalColors()
        {
            foreach (GameObject highlightedObject in GetHighlightedObjects())
            {
                originalColors[highlightedObject] = highlightedObject.GetComponent<Renderer>().material.color;
            }
        }

        private void ScaleUpdate()
        {
            float speed = Time.deltaTime;
            if (decreasing)
            {
                currentScale -= speed;
                if (currentScale < 0.8f) decreasing = false;
            }
            else
            {
                currentScale += Time.deltaTime;
                if (currentScale > 1.2f) decreasing = true;
            }
            foreach (KeyValuePair<GameObject, Vector3> kvPair in originalScales)
            {
                GameObject gameObject = kvPair.Key;
                Vector3 originalScale = kvPair.Value;
                gameObject.transform.localScale = originalScale * currentScale;
            }
        }

        private void ColorUpdate()
        {
            timer += Time.deltaTime;
            
            if (timer > 0.6f)
            {
                foreach (GameObject highlightedObject in originalColors.Keys)
                {
                    Color originalColor = originalColors[highlightedObject];
                    Color currentColor = highlightedObject.GetComponent<Renderer>().material.color;
                    highlightedObject.GetComponent<Renderer>().material.color = currentColor == originalColor
                        ? transparent
                        : originalColor;
                }
                timer = 0;
            }
        }

        private void ResetScales()
        {
            foreach (KeyValuePair<GameObject, Vector3> originalScale in originalScales)
            {
                originalScale.Key.transform.localScale = originalScale.Value;
            }
        }

        private void ResetColors()
        {
            foreach (KeyValuePair<GameObject, Color> originalColor in originalColors)
            {
                originalColor.Key.GetComponent<Renderer>().material.color = originalColor.Value;
            }
        }
    }

}
