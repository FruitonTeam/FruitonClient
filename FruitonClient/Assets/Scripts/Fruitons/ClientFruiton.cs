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

    private void Start()
    {
        var tags = Instantiate(Resources.Load("Models/Auxiliary/Tags", typeof(GameObject))) as GameObject;
        tags.name = TAGS;
        tags.transform.position = transform.position + new Vector3(0, 1.2f, -1.33f);
        tags.transform.Rotate(new Vector3(45, 0, 0));
        tags.transform.parent = transform;
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

        var anim = GetComponent<FruitonBattleAnimator>();
        if (anim != null) // TODO remove when all is Spine
        {
            anim.Initialize();
        }
    }

    public void TakeDamage(int damage)
    {
        string currentHealthStr = healthTag.text;
        int currentHealth = int.Parse(currentHealthStr);

        int newHealth = currentHealth - damage;
        healthTag.text = newHealth.ToString();
    }
}
