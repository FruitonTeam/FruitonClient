using fruiton.kernel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientFruiton : MonoBehaviour {

    private TextMesh healthTag;
    private TextMesh damageTag;

    public Fruiton KernelFruiton { get; set; }

    private const string TAGS = "Tags";
    private const string HEALTH = "Health";
    private const string DAMAGE = "Damage";

    private FruitonBattleAnimator animator;
    private GameObject tags;

    private bool isInitialized;

    private void Start()
    {
        if (!isInitialized)
        {
            isInitialized = true;
            Initialize();
        }
    }

    private void Initialize()
    {
        tags = Instantiate(Resources.Load("Models/Auxiliary/Tags", typeof(GameObject))) as GameObject;
        tags.name = TAGS;
        tags.transform.parent = transform;
        tags.transform.localPosition = Vector3.zero;
        foreach (Transform child in tags.transform)
        {
            switch (child.name)
            {
                case HEALTH:
                    {
                        healthTag = child.GetComponentInChildren<TextMesh>();
                        healthTag.text = KernelFruiton.currentAttributes.hp.ToString();
                    }
                    break;
                case DAMAGE:
                    {
                        damageTag = child.GetComponentInChildren<TextMesh>();
                        damageTag.text = KernelFruiton.currentAttributes.damage.ToString();
                    }
                    break;
            }
        }

        animator = GetComponent<FruitonBattleAnimator>();
        if (animator != null) // TODO remove when all is Spine
        {
            animator.Initialize();
        }
    }

    public void TakeDamage(int damage)
    {
        string currentHealthStr = healthTag.text;
        int currentHealth = int.Parse(currentHealthStr);

        int newHealth = currentHealth - damage;
        healthTag.text = newHealth.ToString();
    }

    public void FlipAround()
    {
        if (!isInitialized)
        {
            isInitialized = true;
            Initialize();
        }
        if (animator != null) // TODO remove when all is Spine
        {
            animator.SkeletonAnim.Skeleton.FlipX = !animator.SkeletonAnim.Skeleton.FlipX;
        }
        if (tags == null)
        {
            Debug.Log("null tags");
        }
        tags.transform.Rotate(0, 180, 0);
    }
}
