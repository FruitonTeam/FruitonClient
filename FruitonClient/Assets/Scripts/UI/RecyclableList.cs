using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class RecyclableList : MonoBehaviour
    {
        #region HANDLER ItemLoaded

        public delegate void OnItemLoadedHandler(ListItemBase item);

        public OnItemLoadedHandler OnItemLoaded;

        public void ItemLoaded(ListItemBase item, bool clear = false)
        {
            if (OnItemLoaded != null)
            {
                OnItemLoaded(item);

                if (clear)
                {
                    OnItemLoaded = null;
                }
            }
        }

        #endregion

        #region HANDLER ItemSelected

        public delegate void OnItemSelectedHandler(ListItemBase item);

        public OnItemSelectedHandler OnItemSelected;

        public void ItemSelected(ListItemBase item, bool clear = false)
        {
            if (OnItemSelected != null)
            {
                OnItemSelected(item);

                if (clear)
                {
                    OnItemSelected = null;
                }
            }
        }

        #endregion


        private enum ScrollOrientation
        {
            HORIZONTAL,
            VERTICAL
        }

        private enum ScrollDirection
        {
            NEXT,
            PREVIOUS
        }


        [SerializeField] 
        ScrollRect scrollRect;

        [SerializeField] 
        RectTransform viewport;

        [SerializeField] 
        RectTransform content;

        [SerializeField] 
        ScrollOrientation scrollOrientation;

        [SerializeField] 
        float spacing;

        [SerializeField] 
        bool fitItemToViewport;

        [SerializeField] 
        bool centerOnItem;

        [SerializeField] 
        float changeItemDragFactor;

        List<ListItemBase> itemsList;

        float itemSize;
        float lastPosition;

        int itemsTotal;
        int itemsVisible;

        int itemsToRecycleBefore;
        int itemsToRecycleAfter;

        int currentItemIndex;
        int lastItemIndex;

        Vector2 dragInitialPosition;

        public void Create(int items, ListItemBase listItemPrefab)
        {
            switch (scrollOrientation)
            {
                case ScrollOrientation.HORIZONTAL:
                    scrollRect.vertical = false;
                    scrollRect.horizontal = true;

                    content.anchorMin = new Vector2(0, 0);
                    content.anchorMax = new Vector2(0, 1);

                    if (fitItemToViewport)
                    {
                        listItemPrefab.Size = new Vector2(viewport.rect.width, listItemPrefab.Size.y);
                    }

                    itemSize = listItemPrefab.Size.x;

                    content.sizeDelta = new Vector2(itemSize * items + spacing * (items - 1), 0);
                    break;

                case ScrollOrientation.VERTICAL:
                    scrollRect.vertical = true;
                    scrollRect.horizontal = false;

                    content.anchorMin = new Vector2(0, 1);
                    content.anchorMax = new Vector2(1, 1);

                    if (fitItemToViewport)
                    {
                        listItemPrefab.Size = new Vector2(listItemPrefab.Size.x, viewport.rect.height);
                    }

                    itemSize = listItemPrefab.Size.y;

                    content.sizeDelta = new Vector2(0, itemSize * items + spacing * (items - 1));
                    break;
            }

            if (centerOnItem)
            {
                scrollRect.inertia = false;
            }


            itemsVisible = Mathf.CeilToInt(GetViewportSize() / itemSize);

            int itemsToInstantiate = itemsVisible;

            if (itemsVisible == 1)
            {
                itemsToInstantiate = 5;
            }
            else if (itemsToInstantiate < items)
            {
                itemsToInstantiate *= 2;
            }

            if (itemsToInstantiate > items)
            {
                itemsToInstantiate = items;
            }

            itemsList = new List<ListItemBase>();

            for (int i = 0; i < itemsToInstantiate; i++)
            {
                ListItemBase item = CreateNewItem(listItemPrefab, i, itemSize);
                item.OnSelected = HandleOnSelectedHandler;
                item.Index = i;

                itemsList.Add(item);

                ItemLoaded(item);
            }

            itemsTotal = items;

            lastItemIndex = itemsList.Count - 1;

            itemsToRecycleAfter = itemsList.Count - itemsVisible;


            scrollRect.onValueChanged.AddListener(position =>
            {
                if (!centerOnItem)
                {
                    Recycle();
                }
            });
        }

        private ListItemBase CreateNewItem(ListItemBase prefab, int index, float dimension)
        {
            GameObject instance = Instantiate(prefab.gameObject, Vector3.zero, Quaternion.identity);
            instance.transform.SetParent(content.transform);
            instance.transform.localScale = Vector3.one;
            instance.SetActive(true);

            float position = index * (dimension + spacing) + dimension / 2;

            RectTransform rectTransform = instance.GetComponent<RectTransform>();

            switch (scrollOrientation)
            {
                case ScrollOrientation.HORIZONTAL:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.anchoredPosition = new Vector2(position, 0);
                    rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0);
                    rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0);
                    break;

                case ScrollOrientation.VERTICAL:
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.anchoredPosition = new Vector2(0, -position);
                    rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
                    rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
                    break;
            }

            return instance.GetComponent<ListItemBase>();
        }


        void HandleOnSelectedHandler(ListItemBase item)
        {
            ItemSelected(item);
        }


        private void Recycle()
        {
            if (lastPosition == -1)
            {
                lastPosition = GetContentPosition();

                return;
            }

            int displacedRows = Mathf.FloorToInt(Mathf.Abs(GetContentPosition() - lastPosition) / itemSize);

            if (displacedRows == 0)
            {
                return;
            }

            ScrollDirection direction = GetScrollDirection();

            for (int i = 0; i < displacedRows; i++)
            {
                switch (direction)
                {
                    case ScrollDirection.NEXT:

                        NextItem();

                        break;

                    case ScrollDirection.PREVIOUS:

                        PreviousItem();

                        break;
                }

                if (direction == ScrollDirection.NEXT && scrollOrientation == ScrollOrientation.VERTICAL 
                    || direction == ScrollDirection.PREVIOUS && scrollOrientation == ScrollOrientation.HORIZONTAL)
                {
                    lastPosition += itemSize + spacing;
                }
                else
                {
                    lastPosition -= itemSize + spacing;
                }
            }
        }

        private void NextItem()
        {
            if (itemsToRecycleBefore >= (itemsList.Count - itemsVisible) / 2 && lastItemIndex < itemsTotal - 1)
            {
                lastItemIndex++;

                RecycleItem(ScrollDirection.NEXT);
            }
            else
            {
                itemsToRecycleBefore++;
                itemsToRecycleAfter--;
            }
        }

        private void PreviousItem()
        {
            if (itemsToRecycleAfter >= (itemsList.Count - itemsVisible) / 2 && lastItemIndex > itemsList.Count - 1)
            {
                RecycleItem(ScrollDirection.PREVIOUS);

                lastItemIndex--;
            }
            else
            {
                itemsToRecycleBefore--;
                itemsToRecycleAfter++;
            }
        }

        private void RecycleItem(ScrollDirection direction)
        {
            ListItemBase firstItem = itemsList[0];
            ListItemBase lastItem = itemsList[itemsList.Count - 1];

            float targetPosition = (itemSize + spacing);

            switch (direction)
            {
                case ScrollDirection.NEXT:

                    switch (scrollOrientation)
                    {
                        case ScrollOrientation.HORIZONTAL:
                            firstItem.Position =
                                new Vector2(lastItem.Position.x + targetPosition, firstItem.Position.y);
                            break;

                        case ScrollOrientation.VERTICAL:
                            firstItem.Position =
                                new Vector2(firstItem.Position.x, lastItem.Position.y - targetPosition);
                            break;
                    }

                    firstItem.Index = lastItemIndex;
                    firstItem.transform.SetAsLastSibling();

                    itemsList.RemoveAt(0);
                    itemsList.Add(firstItem);

                    ItemLoaded(firstItem);
                    break;

                case ScrollDirection.PREVIOUS:

                    switch (scrollOrientation)
                    {
                        case ScrollOrientation.HORIZONTAL:
                            lastItem.Position = new Vector2(firstItem.Position.x - targetPosition, lastItem.Position.y);
                            break;

                        case ScrollOrientation.VERTICAL:
                            lastItem.Position = new Vector2(lastItem.Position.x, firstItem.Position.y + targetPosition);
                            break;
                    }

                    lastItem.Index = lastItemIndex - itemsList.Count;
                    lastItem.transform.SetAsFirstSibling();

                    itemsList.RemoveAt(itemsList.Count - 1);
                    itemsList.Insert(0, lastItem);

                    ItemLoaded(lastItem);
                    break;
            }

            Canvas.ForceUpdateCanvases();
        }


        public void OnDragBegin(BaseEventData eventData)
        {
            if (centerOnItem)
            {
                dragInitialPosition = ((PointerEventData) eventData).position;
            }
        }

        public void OnDragEnd(BaseEventData eventData)
        {
            if (centerOnItem)
            {
                float delta = GetDragDelta(dragInitialPosition, ((PointerEventData) eventData).position);

                if (itemsList != null && Mathf.Abs(delta) > itemSize * changeItemDragFactor)
                {
                    if (Mathf.Sign(delta) == -1 && currentItemIndex < itemsTotal - 1)
                    {
                        NextItem();

                        currentItemIndex++;
                    }
                    else if (Mathf.Sign(delta) == 1 && currentItemIndex > 0)
                    {
                        currentItemIndex--;

                        PreviousItem();
                    }
                }

                CenterOnItem(currentItemIndex);
            }
        }

        public void CenterOnItem(int index)
        {
            StartCoroutine(CenterOnItemCoroutine(index));
        }

        private IEnumerator CenterOnItemCoroutine(int index)
        {
            yield return new WaitForEndOfFrame();

            if (itemsList != null && itemsList.Count > 0)
            {
                float positionX = 0;
                float positionY = 0;

                switch (scrollOrientation)
                {
                    case ScrollOrientation.HORIZONTAL:
                        positionX = -(index * (itemSize + spacing));
                        break;

                    case ScrollOrientation.VERTICAL:
                        positionY = -(index * (itemSize + spacing));
                        break;
                }

                content.anchoredPosition = new Vector2(positionX, positionY);

                // NOT WORKING
                // _scrollRect.normalizedPosition = new Vector2 (positionX, positionY);
            }
            else
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log("CENTER ON ITEM BUT ITEMS LIST IS NULL");
                #endif
            }
        }


        public void Destroy()
        {
            scrollRect.verticalNormalizedPosition = 1;

            if (itemsList != null)
            {
                foreach (ListItemBase item in itemsList)
                {
                    Destroy(item.gameObject);
                }

                itemsList.Clear();
                itemsList = null;
            }

            lastPosition = -1;
        }


        #region UTILS

        private float GetContentPosition()
        {
            switch (scrollOrientation)
            {
                case ScrollOrientation.HORIZONTAL:
                    return content.anchoredPosition.x;

                case ScrollOrientation.VERTICAL:
                    return content.anchoredPosition.y;

                default:
                    return 0;
            }
        }

        private float GetViewportSize()
        {
            switch (scrollOrientation)
            {
                case ScrollOrientation.HORIZONTAL:
                    return viewport.rect.width;

                case ScrollOrientation.VERTICAL:
                    return viewport.rect.height;

                default:
                    return 0;
            }
        }

        private ScrollDirection GetScrollDirection()
        {
            switch (scrollOrientation)
            {
                case ScrollOrientation.HORIZONTAL:
                    return lastPosition < GetContentPosition() ? ScrollDirection.PREVIOUS : ScrollDirection.NEXT;

                case ScrollOrientation.VERTICAL:
                    return lastPosition > GetContentPosition() ? ScrollDirection.PREVIOUS : ScrollDirection.NEXT;

                default:
                    return ScrollDirection.NEXT;
            }
        }

        private float GetDragDelta(Vector2 initial, Vector2 current)
        {
            switch (scrollOrientation)
            {
                case ScrollOrientation.HORIZONTAL:
                    return current.x - initial.x;

                case ScrollOrientation.VERTICAL:
                    return (current.y - initial.y) * -1;

                default:
                    return 0;
            }
        }

        #endregion

        public void AddItem(ListItemBase listItemPrefab)
        {
            if (itemsTotal < 2 * itemsVisible)
            {
                int i = itemsList.Count;
                ListItemBase item = CreateNewItem(listItemPrefab, i, itemSize);
                item.OnSelected = HandleOnSelectedHandler;
                item.Index = i;

                itemsList.Add(item);

                ItemLoaded(item);
            }

            itemsTotal++;
            lastItemIndex = itemsList.Count - 1;
            itemsToRecycleAfter++;

            content.sizeDelta = new Vector2(0, itemSize * itemsTotal + spacing * (itemsTotal - 1));
        }

        public void NotifyDataChanged()
        {
            foreach (ListItemBase item in itemsList)
            {
                ItemLoaded(item);
            }
        }
    }
}