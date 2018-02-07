using System.Collections.Generic;
using System.ComponentModel;
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.fruitDb.factories;
using fruiton.kernel;
using fruiton.kernel.gameModes;
using haxe.root;

public class GameSettingsFactory
{
    public static GameSettings CreateGameSettings(int mapId)
    {
        Array<object> map = MapFactory.makeMap(mapId, GameManager.Instance.FruitonDatabase);
        var settings = GameSettings.createDefault();
        settings.map = map;
        return settings;
    }

    public static GameSettings CreateGameSettings(int mapId, FindGame.Types.GameMode gameMode)
    {
        Array<object> map = MapFactory.makeMap(mapId, GameManager.Instance.FruitonDatabase);
        var settings = GameSettings.createDefault();
        settings.map = map;

        switch (gameMode)
        {
            case FindGame.Types.GameMode.Standard:
                settings.gameMode = new StandardGameMode();
                break;
            case FindGame.Types.GameMode.LastManStanding:
                settings.gameMode = new LastManStandingGameMode();
                break;
            default:
                throw new InvalidEnumArgumentException();
        }
        
        return settings;
    }
}
