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

    private void Start()
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
        healthTag.color = GetHighlightColor(KernelFruiton.originalAttributes.hp, newHealth);
    }

    public void FlipAround()
    {
        if (animator != null) // TODO remove when all is Spine
        {
            animator.SkeletonAnim.Skeleton.FlipX = !animator.SkeletonAnim.Skeleton.FlipX;
        }
        tags.transform.Rotate(0, 180, 0);
    }

    public void ModifyAttack(int newAttack)
    {
        damageTag.text = newAttack.ToString();
        damageTag.color = GetHighlightColor(KernelFruiton.originalAttributes.damage, newAttack);
    }

    private Color GetHighlightColor(int originalValue, int newValue)
    {
        if (newValue < originalValue)
        {
            return Color.red;
        }
        else if (newValue > originalValue)
        {
            return Color.green;
        }
        else
        {
            return Color.black;
        }
    }
}
