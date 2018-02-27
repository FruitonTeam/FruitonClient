using Cz.Cuni.Mff.Fruiton.Dto;
using UnityEngine;

namespace UI.Fridge
{
    public class FridgeFruitonTeam : MonoBehaviour
    {
        public static readonly Color COLOR_DEFAULT = new Color(1, 1, 1);
        public static readonly Color COLOR_INVALID = new Color(1, 0.6f, 0.6f);
        public static readonly Color COLOR_SELECTED = new Color(0.55f, 0.85f, 1);

        public int FridgeIndex;
        public FruitonTeam KernelTeam;
        public bool Valid;
    }
}