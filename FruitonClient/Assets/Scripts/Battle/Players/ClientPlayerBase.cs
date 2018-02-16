using fruiton.kernel;
using fruiton.kernel.actions;

public abstract class ClientPlayerBase
{
    protected Battle battle;

    protected ClientPlayerBase(Player kernelPlayer, Battle battle, string name)
    {
        ID = kernelPlayer.id;
        this.battle = battle;
        Name = name;
    }

    public string Name { get; private set; }

    public int ID { get; set; }

    public abstract void ProcessOpponentAction(EndTurnAction action);
    public abstract void ProcessOpponentAction(TargetableAction action);
}