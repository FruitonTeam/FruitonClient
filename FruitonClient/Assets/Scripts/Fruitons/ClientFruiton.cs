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
            Initialize();
            isInitialized = true;
        }
    }

    private void Initialize()
    {
        tags = Instantiate(Resources.Load("Models/Auxiliary/Tags", typeof(GameObject))) as GameObject;
        tags.name = TAGS;
        tags.transform.Rotate(0, -90, 0);
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

        animator = GetComponentInChildren<FruitonBattleAnimator>();
        if (animator != null) // TODO remove when all is Spine
        {
            animator.Initialize();
        }
    }

    public void TakeDamage(int damage)
    {
        UpdateHealthTag();
    }

    public void ReceiveHeal(int heal)
    {
        UpdateHealthTag();
    }

    public void UpdateHealthTag()
    {
        int newHp = KernelFruiton.currentAttributes.hp;
        healthTag.text = newHp.ToString();
        healthTag.color = GetHighlightColor(KernelFruiton.originalAttributes.hp, newHp);
    }

    public void FlipAround()
    {
        if (!isInitialized)
        {
            Initialize();
            isInitialized = true;
        }
        // Flip (not rotate 180) to make animations play correctly
        animator.SkeletonAnim.Skeleton.FlipX = !animator.SkeletonAnim.Skeleton.FlipX;
        // Fruiton sprite may be off center, move closer to (further from) camera
        Vector3 spriteLocPos = animator.transform.localPosition;
        animator.transform.localPosition = new Vector3(
            -spriteLocPos.x,
            spriteLocPos.y,
            -spriteLocPos.z);
        tags.transform.Rotate(0, 180, 0);
    }

    public void ModifyAttack(int newAttack)
    {
        damageTag.text = newAttack.ToString();
        damageTag.color = GetHighlightColor(KernelFruiton.originalAttributes.damage, newAttack);
    }

    public void ModifyHealth(int newHealth)
    {
        healthTag.text = newHealth.ToString();
        healthTag.color = GetHighlightColor(KernelFruiton.originalAttributes.hp, newHealth);
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
