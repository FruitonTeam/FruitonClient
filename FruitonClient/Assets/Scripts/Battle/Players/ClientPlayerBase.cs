using fruiton.kernel;
using fruiton.kernel.actions;
using UnityEngine;

public abstract class ClientPlayerBase
{
    public Player KernelPlayer;
    
    public bool IsActive { get; set; }
    public int ID { get { return KernelPlayer.id; } }

    protected Battle battle;

    public ClientPlayerBase(Player kernelPlayer, Battle battle)
    {
        KernelPlayer = kernelPlayer;
        this.battle = battle;
    }

    public abstract void ProcessOpponentAction(EndTurnAction action);
    public abstract void ProcessOpponentAction(TargetableAction action);
}
