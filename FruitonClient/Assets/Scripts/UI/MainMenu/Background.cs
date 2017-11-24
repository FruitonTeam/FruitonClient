using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class Background : MonoBehaviour
    {

        private static readonly float CLOUD1_ANIMATION_TIME = 25;
        private static readonly float CLOUD2_ANIMATION_TIME = 35;
        
        public RawImage Cloud1;
        public RawImage Cloud2;

        private void Start()
        {
            iTween.MoveBy(Cloud1.gameObject, iTween.Hash(
                "x", Screen.width,
                "time", CLOUD1_ANIMATION_TIME,
                "easetype", iTween.EaseType.linear,
                "loopType", iTween.LoopType.pingPong
            ));
            
            iTween.MoveBy(Cloud2.gameObject, iTween.Hash(
                "x", -Screen.width,
                "time", CLOUD2_ANIMATION_TIME,
                "easetype", iTween.EaseType.linear,
                "loopType", iTween.LoopType.pingPong
            ));
        }

    }
}