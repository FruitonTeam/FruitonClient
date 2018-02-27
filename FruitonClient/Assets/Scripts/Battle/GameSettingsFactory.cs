using System.ComponentModel;
using fruiton.fruitDb.factories;
using fruiton.kernel;
using fruiton.kernel.gameModes;
using haxe.root;
using ProtoGameMode = Cz.Cuni.Mff.Fruiton.Dto.GameMode;

namespace Battle
{
    public class GameSettingsFactory
    {
        public static GameSettings CreateGameSettings(int mapId)
        {
            Array<object> map = MapFactory.makeMap(mapId, GameManager.Instance.FruitonDatabase);
            var settings = GameSettings.createDefault();
            settings.map = map;
            return settings;
        }

        public static GameSettings CreateGameSettings(int mapId, ProtoGameMode gameMode)
        {
            Array<object> map = MapFactory.makeMap(mapId, GameManager.Instance.FruitonDatabase);
            var settings = GameSettings.createDefault();
            settings.map = map;

            switch (gameMode)
            {
                case ProtoGameMode.Standard:
                    settings.gameMode = new StandardGameMode();
                    break;
                case ProtoGameMode.LastManStanding:
                    settings.gameMode = new LastManStandingGameMode();
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
        
            return settings;
        }
    }
}
