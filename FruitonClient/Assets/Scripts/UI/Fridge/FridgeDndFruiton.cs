using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Fridge
{
    /// <summary>
    /// Handles appearance of fruiton object that's shown when dragging a fruiton.
    /// </summary>
    public class FridgeDndFruiton : MonoBehaviour
    {
        public enum DropStatus
        {
            Ok = 0,
            Nothing = 1,
            Swap = 2,
            Delete = 3
        }

        private SkeletonGraphic spineSkeletonGraphic;
        private Image dropStatusIcon;
        private Sprite[] dropStatusSprites;

        /// <summary>
        /// Loads drop status icons.
        /// </summary>
        void Awake()
        {
            spineSkeletonGraphic = GetComponent<SkeletonGraphic>();
            dropStatusIcon = GetComponentInChildren<Image>();
            var iconNames = new string[] {"checkmark", "no", "swap", "trashcan"};
            dropStatusSprites = new Sprite[iconNames.Length];
            for (int i = 0; i < iconNames.Length; i++)
            {
                dropStatusSprites[i] = Resources.Load<Sprite>("Images/UI/Icons/" + iconNames[i] + "_64");
            }
        }

        /// <summary>
        /// Sets icon indicating an action that would happen if the dragged fruiton was dropped.
        /// </summary>
        /// <param name="status">drop status</param>
        public void SetDropStatus(DropStatus status)
        {
            Color statusColor = Color.white;
            switch (status)
            {
                case DropStatus.Delete:
                case DropStatus.Nothing:
                    statusColor = Color.red;
                    break;
                case DropStatus.Ok:
                    statusColor = Color.green;
                    break;
                case DropStatus.Swap:
                    statusColor = new Color(0.3f, 0.3f, 1);
                    break;
            }
            dropStatusIcon.sprite = dropStatusSprites[(int) status];
            dropStatusIcon.color = statusColor;
        }

        /// <summary>
        /// Sets skin of the dragged fruiton.
        /// </summary>
        /// <param name="skin">skin to set</param>
        public void SetSkin(string skin)
        {
            spineSkeletonGraphic.Skeleton.SetSkin(skin);
        }
    }
}