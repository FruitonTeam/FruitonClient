using System.Collections.Generic;
using UI.Chat;
using UnityEngine;

namespace UI
{
    public abstract class ListController<T> : MonoBehaviour
    {
        public RecyclableList RecyclableList;

        int selectedIndex;

        ListItemBase selectedItem;

        OnItemSelectedListener listener;

        protected List<T> Data = new List<T>();

        protected void Init()
        {
            RecyclableList.OnItemLoaded = HandleOnItemLoadedHandler;
            RecyclableList.OnItemSelected = HandleOnItemSelectedHandler;
        }

        protected void HandleOnItemSelectedHandler(ListItemBase item)
        {
            if (selectedItem != null)
            {
                selectedItem.Select(false);
            }

            selectedItem = (FriendListItem) item;
            selectedItem.Select(true);

            selectedIndex = selectedItem.Index;

            listener.OnItemSelected(selectedIndex);
        }

        protected void HandleOnItemLoadedHandler(ListItemBase item)
        {
            if (item == selectedItem)
            {
                selectedItem.Select(selectedIndex == selectedItem.Index);
            }

            item.OnLoad(Data[item.Index]);
        }

        public void SetOnItemSelectedListener(OnItemSelectedListener listener)
        {
            this.listener = listener;
        }
    }
}