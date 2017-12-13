using fruiton.kernel;
using fruiton.kernel.actions;

public abstract class ClientPlayerBase
{
    protected Battle battle;

    protected ClientPlayerBase(Player kernelPlayer, Battle battle, string login)
    {
        ID = kernelPlayer.id;
        this.battle = battle;
        Login = login;
    }

    public string Login { get; private set; }

    public int ID { get; private set; }

    public abstract void ProcessOpponentAction(EndTurnAction action);
    public abstract void ProcessOpponentAction(TargetableAction action);
}