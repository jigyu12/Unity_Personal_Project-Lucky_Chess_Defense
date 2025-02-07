using UnityEngine;
using UnityEngine.EventSystems;

public class CellMover : MonoBehaviour
{
    private LineRenderer lineRenderer;
    
    private HeroSpawner heroSpawner;
    private HeroSpawnPointInCell touchedCell;
    private HeroSpawnPointInCell firstSelectedMoveCell;
    private HeroSpawnPointInCell lastSelectedMoveCell;
    
    [SerializeField] private LayerMask cellLayer;
    
    private Camera mainCamera;
    
    [SerializeField] private float cameraDistanceZ;

    private void Awake()
    {
        touchedCell = null;
        firstSelectedMoveCell = null;
        lastSelectedMoveCell = null;
    }

    private void Start()
    {
        GameObject.FindGameObjectWithTag("InGameUIManager").TryGetComponent(out lineRenderer);
        
        TryGetComponent(out heroSpawner);
        
        mainCamera = Camera.main;
    }

    private void Update()
    {
        DetectTouch();
    }
    
    private void DetectTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;
            
            Vector2 touchPosition = mainCamera.ScreenToWorldPoint(touch.position);
            
            Collider2D hitCollider = Physics2D.OverlapPoint(touchPosition, cellLayer);
            
            if (hitCollider is not null)
                touchedCell = heroSpawner.CurrCellsDict[hitCollider];
            
            if (touch.phase == TouchPhase.Began)
            {
                if (touchedCell is null || touchedCell.HeroCount <= 0) 
                    return;
                
                firstSelectedMoveCell = touchedCell;
                firstSelectedMoveCell.ShowHighlightMoveCell();
            }
            else if (touch.phase is TouchPhase.Moved or TouchPhase.Stationary && firstSelectedMoveCell is not null)
            {
                if (touchedCell is null) 
                    return;
                
                if (lastSelectedMoveCell is not null && lastSelectedMoveCell != touchedCell && lastSelectedMoveCell != firstSelectedMoveCell)
                    lastSelectedMoveCell.HideHighlightMoveCell();
                    
                lastSelectedMoveCell = touchedCell;
                lastSelectedMoveCell.ShowHighlightMoveCell();
                DrawLineBetweenCells(firstSelectedMoveCell.GetCenterPos(), lastSelectedMoveCell.GetCenterPos());
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (firstSelectedMoveCell is not null && lastSelectedMoveCell is not null && firstSelectedMoveCell != lastSelectedMoveCell)
                    SwapCellPosToOtherCell();
                
                firstSelectedMoveCell?.HideHighlightMoveCell();
                lastSelectedMoveCell?.HideHighlightMoveCell();
                
                ClearLine();
                
                touchedCell = null;
                firstSelectedMoveCell = null;
                lastSelectedMoveCell = null;
            }
        }
    }
    
    private void DrawLineBetweenCells(Vector3 start, Vector3 end)
    {
        lineRenderer.positionCount = 2;
        
        start.z += cameraDistanceZ;
        end.z += cameraDistanceZ;
        
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    private void ClearLine()
    {
        lineRenderer.positionCount = 0;
    }
    
    private void SwapCellPosToOtherCell()
    {
        Vector3 firstSelectedMoveCellPos = firstSelectedMoveCell.transform.position;
        Vector3 lastSelectedMoveCellPos = lastSelectedMoveCell.transform.position;
        
        firstSelectedMoveCell.MoveCell(lastSelectedMoveCellPos);
        lastSelectedMoveCell.MoveCell(firstSelectedMoveCellPos);
        
        heroSpawner.SortHeroSpawnPointInCellList();
    }
}