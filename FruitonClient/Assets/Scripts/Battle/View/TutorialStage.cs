using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One stage in the tutorial.
/// </summary>
public class TutorialStage
{
    public enum StageEndCondition
    {
        ButtonContinueClick,
        LeftClickHighlighted,
        DifferentButtonClick,
        HoverOverHighlighted
    }

    // Scale attributes
    private float currentScale = 1;
    private bool decreasing = false;
    private Dictionary<GameObject, Vector3> originalScales;

    // Color attributes
    private float timer;
    private Dictionary<GameObject, Color> originalColors;

    private static readonly Color BlackColor = Color.black;

    /// <summary> What will be writen to the user. </summary>
    public string Text;
    /// <summary>
    /// Function that return GameObjects that should be highlighted. 
    /// This has to be a function because the list may be (and is) dependent on the time,
    /// so we want to get those objects only at the beginning of the stage, not when creating it.
    /// </summary>
    public Func<List<GameObject>> GetHighlightedObjects;
    public StageEndCondition EndCondition;
    // The following functions are called at the beginning of the stage, during its execution and at the end of it.
    public List<Action> StartActions;
    public List<Action> UpdateActions;
    public List<Action> EndActions;

    public TutorialStage(string text,
        Func<List<GameObject>> highlightedObjects = null,
        List<Action> startActions = null,
        List<Action> updateActions = null,
        List<Action> endActions = null,
        StageEndCondition endCondition = StageEndCondition.ButtonContinueClick,
        bool scaleHighlight = true)
    {
        Text = text;
        GetHighlightedObjects = highlightedObjects ?? (() => new List<GameObject>());
        StartActions = startActions ?? new List<Action>();
        UpdateActions = updateActions ?? new List<Action>();
        EndActions = endActions ?? new List<Action>();
        EndCondition = endCondition;

        if (scaleHighlight)
        {
            originalScales = new Dictionary<GameObject, Vector3>();
            StartActions.Add(InitializeOriginalScales);
            UpdateActions.Add(ScaleUpdate);
            EndActions.Add(ResetScales);
        }
        // Color highlight.
        else
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
                    ? BlackColor
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