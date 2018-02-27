using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using Google.Protobuf.Collections;

namespace Battle
{
    public static class BattleHelper
    {
        public static RepeatedField<Position> FlipCoordinates(IEnumerable<Position> positions, int width, int height)
        {
            var result = new RepeatedField<Position>();
            foreach (var position in positions)
            {
                result.Add(new Position { X = width - 1 - position.X, Y = height - 1 - position.Y });
            }
            return result;
        }
    }
}
