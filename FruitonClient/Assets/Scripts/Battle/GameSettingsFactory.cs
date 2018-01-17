using System.Collections.Generic;
using fruiton.fruitDb.factories;
using fruiton.kernel;
using haxe.root;

public class GameSettingsFactory
{
    public static GameSettings CreateGameSettings(int mapId)
    {
        Array<object> map = MapFactory.makeMap(mapId, GameManager.Instance.FruitonDatabase);
        return new GameSettings(map);
    }
}
