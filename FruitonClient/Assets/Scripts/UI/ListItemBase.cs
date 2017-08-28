using UnityEngine;

namespace UI
{
	public abstract class ListItemBase : MonoBehaviour
	{

		public abstract void Select(bool selected);

		public abstract void OnLoad(object data);

		public delegate void OnSelectedHandler (ListItemBase item);

		public OnSelectedHandler onSelected;

		public void Selected(bool clear = false)
		{
			if (onSelected != null)
			{
				onSelected(this);

				if (clear)
				{
					onSelected = null;
				}
			}
		}
		
		[SerializeField]
		private RectTransform _rectTransform;


		public int Index
		{
			get;
			set;
		}

		public Vector2 Size
		{
			get
			{
				return _rectTransform.sizeDelta;
			}

			set
			{
				_rectTransform.sizeDelta = value;
			}
		}

		public Vector2 Position
		{
			get 
			{
				return _rectTransform.anchoredPosition;
			}

			set
			{
				_rectTransform.anchoredPosition = value;
			}
		}
	}	
}