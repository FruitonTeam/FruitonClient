using fruiton.kernel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientFruiton : MonoBehaviour {

    public TextMesh HealthTag;
    public TextMesh DamageTag;

    public Fruiton KernelFruiton { get; set; }

    private const string TAGS = "Tags";
    private const string HEALTH = "Health";
    private const string DAMAGE = "Damage";

    private FruitonBattleAnimator animator;
    private GameObject tags;

    public bool IsInitialized;

    private void Start()
    {
        if (!IsInitialized)
        {
            Initialize();
            IsInitialized = true;
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
                        HealthTag = child.GetComponentInChildren<TextMesh>();
                        HealthTag.text = KernelFruiton.currentAttributes.hp.ToString();
                    }
                    break;
                case DAMAGE:
                    {
                        DamageTag = child.GetComponentInChildren<TextMesh>();
                        DamageTag.text = KernelFruiton.currentAttributes.damage.ToString();
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
        HealthTag.text = newHp.ToString();
        HealthTag.color = GetHighlightColor(KernelFruiton.originalAttributes.hp, newHp);
    }

    public void FlipAround()
    {
        if (!IsInitialized)
        {
            Initialize();
            IsInitialized = true;
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
        Transform spineModel = transform.GetChild(0);
        Vector3 oldEulerAngles = spineModel.eulerAngles;
        spineModel.eulerAngles = new Vector3(-oldEulerAngles.x, oldEulerAngles.y, oldEulerAngles.z);
    }

    public void ModifyAttack(int newAttack)
    {
        DamageTag.text = newAttack.ToString();
        DamageTag.color = GetHighlightColor(KernelFruiton.originalAttributes.damage, newAttack);
    }

    public void ModifyHealth(int newHealth)
    {
        HealthTag.text = newHealth.ToString();
        HealthTag.color = GetHighlightColor(KernelFruiton.originalAttributes.hp, newHealth);
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
