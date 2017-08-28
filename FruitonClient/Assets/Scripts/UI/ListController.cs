using System.Collections.Generic;
using UI.Chat;
using UnityEngine;

namespace UI
{
    public abstract class ListController<T> : MonoBehaviour
    {
        public RecyclableList RecyclableList;

        private int _selectedIndex;

        private ListItemBase _selectedItem;

        private OnItemSelectedListener _listener;

        protected List<T> Data = new List<T>();

        protected void Init()
        {
            RecyclableList.onItemLoaded = HandleOnItemLoadedHandler;
            RecyclableList.onItemSelected = HandleOnItemSelectedHandler;
        }

        protected void HandleOnItemSelectedHandler(ListItemBase item)
        {
            if (_selectedItem != null)
            {
                _selectedItem.Select(false);
            }

            _selectedItem = (FriendListItem) item;
            _selectedItem.Select(true);

            _selectedIndex = _selectedItem.Index;

            _listener.OnItemSelected(_selectedIndex);
        }

        protected void HandleOnItemLoadedHandler(ListItemBase item)
        {
            if (item == _selectedItem)
            {
                _selectedItem.Select(_selectedIndex == _selectedItem.Index);
            }

            item.OnLoad(Data[item.Index]);
        }

        public void SetOnItemSelectedListener(OnItemSelectedListener listener)
        {
            _listener = listener;
        }
    }
}