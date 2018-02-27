using UnityEngine;
using UnityEngine.UI;

namespace TeamsManagement
{
    public class ScrollController : MonoBehaviour {

        public HorizontalLayoutGroup content;

        public void OnScroll()
        {
            Debug.Log(content.transform.childCount * content.GetComponent<RectTransform>().sizeDelta.y);
            //this.GetComponent<ScrollRect>().StopMovement();
            //this.GetComponent<ScrollRect>().enabled = false;
        }
    }
}
