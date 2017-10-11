using fruiton.kernel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientFruiton : MonoBehaviour {

    private TextMesh healthTag;
    private TextMesh damageTag;

    public Fruiton KernelFruiton { get; set;}

    private const string TAGS = "Tags";
    private const string HEALTH = "Health";
    private const string DAMAGE = "Damage";

    private void Start()
    {
        var tags = UnityEngine.Object.Instantiate(Resources.Load("Models/Auxiliary/Tags", typeof(GameObject))) as GameObject;
        tags.name = TAGS;
        tags.transform.position = transform.position + new Vector3(0, 1, -1.33f);
        tags.transform.Rotate(new Vector3(45, 0, 0));
        tags.transform.parent = transform;
        foreach (Transform child in tags.transform)
        {
            switch (child.name)
            {
                case HEALTH:
                    {
                        healthTag = child.GetComponentInChildren<TextMesh>();
                        healthTag.text = KernelFruiton.hp.ToString();
                    }
                    break;
                case DAMAGE:
                    {
                        damageTag = child.GetComponentInChildren<TextMesh>();
                        damageTag.text = KernelFruiton.damage.ToString();
                    }
                    break;
            }
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
