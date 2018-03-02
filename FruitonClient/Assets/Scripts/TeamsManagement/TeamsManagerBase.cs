using Cz.Cuni.Mff.Fruiton.Dto;
using UI.Fridge;
using UnityEngine;
using UnityEngine.UI;
using KFruiton = fruiton.kernel.Fruiton;

namespace TeamsManagement
{
    /// <summary>
    /// Handles creation and editation of fruiton teams.
    /// </summary>
    public abstract class TeamManagerBase : FruitonVisualizerBase
    {
        public ScrollRect ScrollRect;
    
        public FridgeTeamGrid MyTeamGrid;

        public Image DragAndDropBarrier;
        public FridgeDndFruiton DragAndDropFruiton;
        public Text WarningText;
        public GameObject Filters;

        /// <summary> Fruiton that is currently being dragged, null if drag and drop isn't active. </summary>
        protected KFruiton draggedFruiton;
        /// <summary> Position in a fruiton team from which is fruiton being dragged, null if there's no such fruiton. </summary>
        protected Position teamDragGridPosition;
        /// <summary> True if player is currently dragging a fruiton using drag and drop feature. </summary>
        protected bool isDragging;
        /// <summary> True if a fruiton from the team grid is currently being dragged.</summary>
        protected bool isDraggingFromTeam;
        /// <summary> True if a fruiton is being added to the team from the fruiton detail window. </summary>
        protected bool isAddingFromDetail;

        /// <param name="fruiton">fruiton to drag</param>
        /// <returns>true if given fruiton can be drag and dropped</returns>
        protected abstract bool ShouldBeginDrag(FridgeFruiton fruiton);
        /// <summary> Invoked when an "add to team" button in fruiton detail window is pressed. </summary>
        protected abstract void AddToTeamButtonListener();
        /// <summary> Invoked whenever drag and drop is initiated on a fruiton from the team grid. </summary>
        /// <param name="fruiton">fruiton being dragged</param>
        /// <param name="position">position in a team from which the fruiton is dragged</param>
        protected abstract void OnBeginDragFromTeamListener(KFruiton fruiton, Position position);
        /// <summary> Invoked whenever player drops  a fruiton after dragging. </summary>
        /// <param name="dropGridPosition">position in a team over which was fruiton dropped, null if fruiton wasn't dropped over the team grid</param>
        protected abstract void ProcessStopDrag(Position dropGridPosition);

        protected virtual Position ProcessDropGridPosition(Position dropGridPosition)
        {
            return dropGridPosition;
        }

        protected void InitializeFruitonDetailListeners()
        {
            FruitonDetail.CloseButton.onClick.AddListener(HideDetail);
            FruitonDetail.Barrier.onClick.AddListener(HideDetail);
            FruitonDetail.AddToTeamButton.onClick.AddListener(AddToTeamButtonListener);
        }

        protected virtual void InitializeTeamGridListeners()
        {
            MyTeamGrid.OnBeginDragFromTeam.AddListener(OnBeginDragFromTeamListener);
            MyTeamGrid.OnMouseEnterSquare.AddListener(HighlightSquare);
            MyTeamGrid.OnMouseExitSquare.AddListener(CancelHighlightSquare);
        }

        /// <summary>
        /// Handles drag and drop logic.
        /// </summary>
        protected virtual void Update()
        {
            // drag and drop (or adding from fruiton detail window) logic
            if (isDragging)
            {
#if UNITY_ANDROID
                Vector3 pointerPosition = Vector3.left * 1000.0f;
                Vector3 dndPosition = Vector3.left * 1000.0f;
                if (Input.touchCount > 0)
                {
                    var touchPos = Input.GetTouch(0).position;
                    pointerPosition = new Vector3(touchPos.x, touchPos.y, 0) + new Vector3(-1, 1, 0) * 50;
                    dndPosition = pointerPosition + new Vector3(-1, 1, 0) * 100;
                }
#elif UNITY_STANDALONE || UNITY_EDITOR
            Vector3 pointerPosition = Input.mousePosition;
            Vector3 dndPosition = Input.mousePosition + Vector3.down * 50;
#endif
                DragAndDropFruiton.transform.position = dndPosition;

                Position dropGridPosition =
                    MyTeamGrid.SuggestFruitonAtMousePosition(pointerPosition, draggedFruiton, teamDragGridPosition);

                dropGridPosition = ProcessDropGridPosition(dropGridPosition);

                if (dropGridPosition == null)
                {
                    DragAndDropFruiton.SetDropStatus(
                        isDraggingFromTeam
                            ? FridgeDndFruiton.DropStatus.Delete
                            : FridgeDndFruiton.DropStatus.Nothing
                    );
                }
                else
                {
                    DragAndDropFruiton.SetDropStatus(
                        isDraggingFromTeam
                            ? FridgeDndFruiton.DropStatus.Swap
                            : FridgeDndFruiton.DropStatus.Ok
                    );
                }

                // stop drag and drop
                if ((!isAddingFromDetail && !Input.GetMouseButton(0))
                    || (isAddingFromDetail && Input.GetMouseButton(0)))
                {
                    HideTooltip();
                    isDragging = false;
                    isAddingFromDetail = false;
                    DragAndDropFruiton.gameObject.SetActive(false);
                    DragAndDropBarrier.gameObject.SetActive(false);
                    MyTeamGrid.CancelHighlights();
                    ScrollRect.horizontal = true;

                    ProcessStopDrag(dropGridPosition);
                    WarningText.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Highlights a square in the team grid.
        /// </summary>
        /// <param name="square">square to highlight</param>
        protected void HighlightSquare(FridgeGridSquare square)
        {
            if (!isDragging)
            {
                square.Highlight(new Color(1, 1, 0.8f));
                if (square.KernelFruiton != null)
                {
                    ShowTooltip(square.KernelFruiton, 1);
                }
            }
        }

        /// <summary>
        /// Cancels highlight of a square in the team grid.
        /// </summary>
        /// <param name="square">square to stop highlighting</param>
        protected void CancelHighlightSquare(FridgeGridSquare square)
        {
            if (!isDragging)
            {
                square.CancelHighlight();
                HideTooltip();
            }
        }

        private void BeginFruitonDrag(FridgeFruiton fruiton)
        {
            if (ShouldBeginDrag(fruiton))
            {
                BeginFruitonDrag(fruiton.KernelFruiton);
            }
        }

        /// <summary>
        /// Initializes drag and drop mode, displays info to user.
        /// </summary>
        /// <param name="fruiton">fruiton that is being dragged</param>
        /// <param name="teamPosition">position in a team grid from the fruiton is being dragged, null if fruiton isn't being dragged from the team</param>
        protected void BeginFruitonDrag(KFruiton fruiton, Position teamPosition = null)
        {
            draggedFruiton = fruiton;
            DragAndDropBarrier.gameObject.SetActive(true);
            DragAndDropFruiton.gameObject.SetActive(true);
            DragAndDropFruiton.SetSkin(fruiton.model);
            ScrollRect.horizontal = false;
            teamDragGridPosition = teamPosition;
            isDragging = true;
            isDraggingFromTeam = teamPosition != null;
#if UNITY_ANDROID && !UNITY_EDITOR
        ShowTooltip(fruiton, 2);
# else
            HideTooltip();
#endif
            var isAnySquareAvailable = MyTeamGrid.HighlightAvailableSquares(fruiton.type, isDraggingFromTeam);
            if (isAnySquareAvailable)
            {
                if (isDraggingFromTeam)
                {
                    WarningText.text =
                        "<color=#5555ff>Move</color> fruiton to other <color=#00ff00>available square</color> or <color=#ff0000>remove</color> it from the team";
                }
                else if (isAddingFromDetail)
                {
                    WarningText.text =
                        "Choose an <color=#00ff00>available square</color> or click anywhere else to cancel";
                }
                else
                {
                    WarningText.text =
                        "Drag the fruiton to any of the <color=#00ff00>available squares</color> to add it to the team";
                }
                WarningText.color = Color.white;
            }
            else
            {
                WarningText.text = "You don't have any empty square for fruiton of this type!";
                WarningText.color = Color.red;
            }
            WarningText.transform.parent.gameObject.SetActive(true);
        }

        /// <summary>
        /// Sets up fridge fruiton event listeners
        /// </summary>
        protected override void InitializeFridgeFruiton(FridgeFruiton fFruiton, KFruiton kFruiton, int fridgeIndex)
        {
            base.InitializeFridgeFruiton(fFruiton, kFruiton, fridgeIndex);
            fFruiton.OnBeginDrag.AddListener(() => BeginFruitonDrag(fFruiton));
#if UNITY_STANDALONE || UNITY_EDITOR
            fFruiton.OnMouseEnter.AddListener(() => ShowTooltip(kFruiton));
            fFruiton.OnMouseExit.AddListener(HideTooltip);
            fFruiton.OnRightClick.AddListener(() => ShowDetail(fFruiton));
#endif
            fFruiton.OnTap.AddListener(() => ShowDetail(fFruiton));
        }

        /// <summary>
        /// Displays fruiton detail window.
        /// </summary>
        /// <param name="fruiton"></param>
        protected override void ShowDetail(FridgeFruiton fruiton)
        {
            FruitonDetail.SetFruiton(fruiton, MyTeamGrid.GetAvailableSquares(fruiton.KernelFruiton).Count != 0);
            base.ShowDetail(fruiton);
        }

    }
}
