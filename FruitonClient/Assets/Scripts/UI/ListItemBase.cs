using UnityEngine;

namespace UI
{
	public abstract class ListItemBase : MonoBehaviour
	{

		public abstract void Select(bool selected);

		public abstract void OnLoad(object data);

		public delegate void OnSelectedHandler (ListItemBase item);

		public OnSelectedHandler OnSelected;

		public void Selected(bool clear = false)
		{
			if (OnSelected != null)
			{
				OnSelected(this);

				if (clear)
				{
					OnSelected = null;
				}
			}
		}
		
		[SerializeField]
		RectTransform rectTransform;

		public int Index
		{
			get;
			set;
		}

		public Vector2 Size
		{
			get
			{
				return rectTransform.sizeDelta;
			}

			set
			{
				rectTransform.sizeDelta = value;
			}
		}

		public Vector2 Position
		{
			get 
			{
				return rectTransform.anchoredPosition;
			}

			set
			{
				rectTransform.anchoredPosition = value;
			}
		}
	}	
}