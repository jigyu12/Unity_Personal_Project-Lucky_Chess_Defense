using UnityEngine;

public class CellMover : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private readonly Vector3 centerPosOffset = new(0.5f, 0.5f, -1f);
    
    private HeroSpawner heroSpawner;
    private HeroSpawnPointInCell touchedCell;
    private HeroSpawnPointInCell firstSelectedMoveCell;
    private HeroSpawnPointInCell lastSelectedMoveCell;
    
    public LayerMask cellMask;
    
    private Camera mainCamera;

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
            
            Vector2 touchPosition = mainCamera.ScreenToWorldPoint(touch.position);
            
            Collider2D hitCollider = Physics2D.OverlapPoint(touchPosition, cellMask);
            
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
                DrawLineBetweenCells(firstSelectedMoveCell.transform.position, lastSelectedMoveCell.transform.position);
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
        
        lineRenderer.SetPosition(0, start + centerPosOffset);
        lineRenderer.SetPosition(1, end + centerPosOffset);
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