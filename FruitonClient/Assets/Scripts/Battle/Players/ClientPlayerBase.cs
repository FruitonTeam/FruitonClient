using fruiton.kernel;
using fruiton.kernel.actions;

public abstract class ClientPlayerBase
{
    protected Battle battle;

    protected ClientPlayerBase(Player kernelPlayer, Battle battle)
    {
        ID = kernelPlayer.id;
        this.battle = battle;
    }

    public bool IsActive { get; set; }

    public int ID { get; private set; }

    public abstract void ProcessOpponentAction(EndTurnAction action);
    public abstract void ProcessOpponentAction(TargetableAction action);
}