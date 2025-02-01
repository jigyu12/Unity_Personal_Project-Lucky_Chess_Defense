using System.Collections.Generic;
using UnityEngine;

public class HeroSpawnPointInCell : MonoBehaviour
{
    [SerializeField] private GameObject singlePos;
    
    [SerializeField] private GameObject doublePos0;
    [SerializeField] private GameObject doublePos1;
    
    [SerializeField] private GameObject triplePos0;
    [SerializeField] private GameObject triplePos1;
    [SerializeField] private GameObject triplePos2;

    private readonly List<GameObject> singlePosList = new();
    private readonly List<GameObject> doublePosList = new();
    private readonly List<GameObject> triplePosList = new();

    public int Cost { get; private set; }
    public int HeroCount { get; private set; }
    public int OccupyHeroId { get; private set; }
    public float AttackRange { get; private set; }
    
    [SerializeField] private List<Hero> heroList;

    private const int HeroCountMax = 3;
    
    [SerializeField] private SpriteRenderer circleRenderer;
    [SerializeField] private SpriteRenderer circleOutlineRenderer;
    [SerializeField] private float outlineThickness = 0.025f;
    private float circleSpeed;
    private bool isCellSwapping;
    
    private Camera mainCamera;
    
    private GameObject parent;
    
    [SerializeField] private SpriteRenderer squareRenderer;
    public Collider2D Coll2D { get; private set; }
    
    [SerializeField] private LayerMask cellLayer;
    
    private void Awake()
    {
        heroList.Capacity = HeroCountMax;

        TryGetComponent(out Collider2D coll);
        Coll2D = coll;
        
        isCellSwapping = false;
        
        singlePosList.Clear();
        doublePosList.Clear();
        triplePosList.Clear();
    }

    private void OnEnable()
    {
        foreach (Hero hero in heroList)
            hero.DestroyHero();
        
        heroList.Clear();
        
        Cost = 3;
        HeroCount = 0;
        OccupyHeroId = -1;
        AttackRange = 0f;

        HideAttackRangeCircle();
        HideHighlightMoveCell();
    }
    
    private void Start()
    {
        parent = GameObject.FindGameObjectWithTag("Cells");
        transform.SetParent(parent.transform);

        mainCamera = Camera.main;
        
        singlePosList.Add(singlePos);
        
        doublePosList.Add(doublePos0);
        doublePosList.Add(doublePos1);
        
        triplePosList.Add(triplePos0);
        triplePosList.Add(triplePos1);
        triplePosList.Add(triplePos2);

        circleSpeed = Hero.HeroSwapSpeed;
    }
    
    private void Update()
    {
        DetectTouch();
        
        CircleMoveToCurrCellPos();
    }

    private void DetectTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            Vector2 touchPosition = mainCamera.ScreenToWorldPoint(touch.position);

            Collider2D hitCollider = Physics2D.OverlapPoint(touchPosition, cellLayer);

            if (touch.phase == TouchPhase.Began)
            {
                if (hitCollider?.gameObject == gameObject && AttackRange != 0f)
                    ShowAttackRangeCircle();
                else
                    HideAttackRangeCircle();
            }
        }
    }

    public void ShowHighlightMoveCell()
    {
        squareRenderer.gameObject.SetActive(true);
    }

    public void HideHighlightMoveCell()
    {
        squareRenderer.gameObject.SetActive(false);
    }
    
    private void ShowAttackRangeCircle()
    {
        float spriteSize = circleRenderer.sprite.bounds.size.x;
        circleRenderer.gameObject.transform.localScale = Vector3.one * ((AttackRange - outlineThickness) * 2 / spriteSize);
        
        float outlineSpriteSize = AttackRange * 2 / spriteSize;
        circleOutlineRenderer.gameObject.transform.localScale = Vector3.one * outlineSpriteSize;
        
        circleRenderer.gameObject.SetActive(true);
        circleOutlineRenderer.gameObject.SetActive(true);
    }

    private void HideAttackRangeCircle()
    {
        circleRenderer.gameObject.SetActive(false);
        circleOutlineRenderer.gameObject.SetActive(false);
    }

    public void MoveCell(Vector3 destPos)
    {
        circleRenderer.transform.SetParent(null);
        circleOutlineRenderer.transform.SetParent(null);
        
        isCellSwapping = true;
        
        transform.position = destPos;
        
        PlaceHero(false);
    }
    
    private void CircleMoveToCurrCellPos()
    {
        if(!isCellSwapping)
            return;
        
        Vector3 destPosition = singlePos.transform.position;
        
        circleRenderer.transform.position = Vector3.MoveTowards(circleRenderer.transform.position, destPosition, 
            circleSpeed * Time.deltaTime);
        circleOutlineRenderer.transform.position = Vector3.MoveTowards(circleOutlineRenderer.transform.position, destPosition, 
            circleSpeed * Time.deltaTime);
        
        if (Vector3.Distance(circleRenderer.transform.position, destPosition) < 0.01f)
        {
            isCellSwapping = false;

            circleRenderer.transform.position = destPosition;
            circleOutlineRenderer.transform.position = destPosition;
            
            circleRenderer.transform.SetParent(transform);
            circleOutlineRenderer.transform.SetParent(transform);
        }
    }

    public bool CanSpawnHero(Hero hero)
    {
        if (OccupyHeroId == -1)
            return CalculatePlaceHeroByCostAndCount(hero);
        
        return OccupyHeroId == hero.HeroId && CalculatePlaceHeroByCostAndCount(hero);
    }

    private bool CalculatePlaceHeroByCostAndCount(Hero hero)
    {
        if (Cost < hero.Cost || HeroCount >= HeroCountMax) 
            return false;
                
        OccupyHeroId = hero.HeroId;
        Cost -= hero.Cost;
        AttackRange = hero.AttackRange;

        heroList.Add(hero);
        ++HeroCount;

        PlaceHero();
                
        return true;
    }

    private void PlaceHero(bool teleport = true)
    {
        switch (HeroCount)
        {
            case 0:
                break;
            case 1:
            {
                for (int i = 0; i < singlePosList.Count; ++i)
                {
                    if (teleport)
                    {
                        if(heroList[i].IsMoving)
                            heroList[i].destPosition = singlePosList[i].transform.position;
                        else
                            heroList[i].transform.position = singlePosList[i].transform.position;
                    }
                    else
                    {
                        heroList[i].destPosition = singlePosList[i].transform.position;
                        
                        heroList[i].IsMoving = true;
                    }
                }
            }
                break;
            case 2:
            {
                for (int i = 0; i < doublePosList.Count; ++i)
                {
                    if (teleport)
                    {
                        if(heroList[i].IsMoving)
                            heroList[i].destPosition = doublePosList[i].transform.position;
                        else
                            heroList[i].transform.position = doublePosList[i].transform.position;
                    }
                    else
                    {
                        heroList[i].destPosition = doublePosList[i].transform.position;
                        
                        heroList[i].IsMoving = true;
                    }
                       
                }
            }
                break; 
            case 3:
            {
                for (int i = 0; i < triplePosList.Count; ++i)
                {
                    if (teleport)
                    {
                        if(heroList[i].IsMoving)
                            heroList[i].destPosition = triplePosList[i].transform.position;
                        else
                            heroList[i].transform.position = triplePosList[i].transform.position;
                    }
                    else
                    {
                        heroList[i].destPosition = triplePosList[i].transform.position;
                        
                        heroList[i].IsMoving = true;
                    }
                }
            }
                break;
            default:
                Debug.Assert(false, "Invalid Hero Count");
                break;
        }
    }

    public Vector3 GetCenterPos()
    {
        return singlePos.transform.position;
    }
}