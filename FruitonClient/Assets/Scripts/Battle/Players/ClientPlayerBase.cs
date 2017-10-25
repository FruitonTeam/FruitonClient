using fruiton.kernel;
using fruiton.kernel.actions;
using UnityEngine;

public abstract class ClientPlayerBase : MonoBehaviour
{
    public Player KernelPlayer;
    protected Battle battle;
    
    public bool IsActive { get; set; }
    public int ID { get { return KernelPlayer.id; } }

    public ClientPlayerBase(Battle battle)
    {
        this.battle = battle;
    }

    public abstract void ProcessOpponentAction(Action action);
}
