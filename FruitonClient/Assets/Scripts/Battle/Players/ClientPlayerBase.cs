using fruiton.kernel;
using UnityEngine;

public abstract class ClientPlayerBase : MonoBehaviour
{
    public Player KernelPlayer;
    protected Kernel kernel;
    protected BattleManager battleManager;
    
    public bool IsActive { get; set; }
    public int ID { get { return KernelPlayer.id; } }

    public ClientPlayerBase(Kernel kernel, BattleManager battleManager)
    {
        this.kernel = kernel;
        this.battleManager = battleManager;
    }
}
