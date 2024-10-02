using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingCreator : Singleton<BuildingCreator>
{
    [SerializeField] private List<BuildingCategory> categoriesToCreateTilemap; // 동적으로 생성할 타일맵들의 종류
    [SerializeField] private List<UICategory> categoriesToCreateUI; // 동적으로 생성할 BuildingObject 카테고리
    [SerializeField] private Transform tilemapParent; // 동적으로 생성될 타일맵들의 부모 object
    [SerializeField] private Transform uiParent; // 동적으로 생성될 BuildingObject 셀렉터의 부모 object
    [SerializeField] private GameObject categoryPrefab; // 동적으로 생성될 셀렉터 UI 중에서 각각의 UI 카테고리 하나를 전체로 묶음
    [SerializeField] private GameObject categoryItemPrefab; // 동적으로 생성될 셀렉터 UI 중에서 각각의 BuildingObject 아이콘 하나
    [SerializeField] private Tilemap previewTileMap, defaultMap;
    [SerializeField] int buildingObjectUISize = 48;
    
    private PlayerInput _playerInput;
    private Vector2 _mousePos;
    private Vector3Int _currentGridPosition; // 현재 가리키고 있는 그리드의 idx
    private Vector3Int _lastGridPosition; // 직전 idx (마우스가 이동함에 따라 미리보기 타일을 삭제하기 위함)
    private Camera _camera;
    private BuildingObjectBase _selectedObj;
    private List<Tilemap> generatedTilemaps = new List<Tilemap>();
    
    private bool holdActive; // 현재 마우스를 누르고 있는 상태인지
    private Vector3Int holdStartPosiiton; // 마우스를 처음 누른 지점의 좌표
    private BoundsInt bounds; // 드래그 동작에서 직사각형 영역의 x, y좌표를 저장
    
    public Vector3 playerStartPosition = Vector3.back;
    private Vector3Int playerStartCellPosition = Vector3Int.back;
    
    private Dictionary<UICategory, GameObject> uiCategories = new Dictionary<UICategory, GameObject>(); // 각각의 UI 카테고리(벽, 바닥...)와 UI 카테고리 오브젝트 간의 매핑을 저장 
    private Dictionary<GameObject, Transform> buildingObjectItemSlot = new Dictionary<GameObject, Transform>(); // 각각의 BuildingObject와 UI 카테고리 오브젝트 부모 (ui category 아래의 'Items' 오브젝트) 간의 매핑을 저장
    
    private BuildingObjectBase SelectedObj
    {
        set
        {
            _selectedObj = value;
            UpdatePreview();
        }
    }

    private Tilemap tilemap 
    {
        get // 각 selectedObj의 카테고리에 해당하는 타일맵의 종류를 리턴
        {
            if (_selectedObj != null && _selectedObj.BuildingCategory != null && _selectedObj.BuildingCategory.Tilemap != null)
            {
                return _selectedObj.BuildingCategory.Tilemap;
            }
            return defaultMap;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _playerInput = new PlayerInput();
        _camera = Camera.main;
    }

    private void Start()
    {
        InitializeMaps();
        BuildUI();
    }
    
    private void Update()
    {
        if (_selectedObj != null)
        {
            Vector3 pos = _camera.ScreenToWorldPoint(_mousePos);
            Vector3Int gridPos = previewTileMap.WorldToCell(pos); // 현재 마우스 위치의 WorldPosition -> GridIndex
        
            if (gridPos != _currentGridPosition) // GridPosition 정보 갱신
            {
                _lastGridPosition = _currentGridPosition;
                _currentGridPosition = gridPos; 
            
                UpdatePreview();
                if (holdActive && _selectedObj.PlaceType > PlaceType.Single)
                {
                    HandleDrawing();
                }
            }
        }
    }

    private void OnEnable()
    {
        _playerInput.Enable();
        _playerInput.Gameplay.MousePosition.performed += OnMouseMove;
        _playerInput.Gameplay.MouseLeftClick.performed += OnLeftClick;
        _playerInput.Gameplay.MouseLeftClick.started += OnLeftClick;
        _playerInput.Gameplay.MouseLeftClick.canceled += OnLeftClick;
        _playerInput.Gameplay.MouseRightClick.performed += OnRightClick;
    }

    private void OnDisable()
    {
        _playerInput.Disable();
        _playerInput.Gameplay.MousePosition.performed -= OnMouseMove;
        _playerInput.Gameplay.MouseLeftClick.performed -= OnLeftClick;
        _playerInput.Gameplay.MouseLeftClick.started -= OnLeftClick;
        _playerInput.Gameplay.MouseLeftClick.canceled -= OnLeftClick;
        _playerInput.Gameplay.MouseRightClick.performed -= OnRightClick;
    }

    // 타일을 배치하기 전, 타일을 선택한 채로 마우스를 이동하면 마우스 포인터를 따라 미리보기 타일을 생성하는 기능
    private void UpdatePreview()
    {
        // 이전 위치의 타일을 삭제
        previewTileMap.SetTile(_lastGridPosition, null);
        // 현재 위치에 tileBase를 set
        previewTileMap.SetTile(_currentGridPosition, _selectedObj ? _selectedObj.TileBase : null);
    }

    private void OnMouseMove(InputAction.CallbackContext ctx)
    {
        _mousePos = ctx.ReadValue<Vector2>();
    }

    private void OnLeftClick(InputAction.CallbackContext ctx)
    {
        if (_selectedObj != null && !EventSystem.current.IsPointerOverGameObject())
        {
            if (ctx.phase == InputActionPhase.Started && ctx.interaction is TapInteraction) // 누르기 시작.
            // 'TapInteraction' 조건이 없으면 SlowTapInteraction.Started에서 값이 갱신되어 마우스를 빠르게 이동시키는 경우 끊기게 되거나 single Placetype의 경우 두 개가 설치된다.
            {
                holdActive = true;
                holdStartPosiiton = _currentGridPosition;
                HandleDrawing();
            }
            else if ((ctx.interaction is TapInteraction && ctx.phase is InputActionPhase.Performed) ||
                (ctx.interaction is SlowTapInteraction && ctx.phase is InputActionPhase.Performed) ||
                (ctx.interaction is SlowTapInteraction && ctx.phase is InputActionPhase.Canceled)) // 마우스를 뗌
            {
                holdActive = false;
                HandleDrawRelease();
            }
        }
    }
    
    private void OnRightClick(InputAction.CallbackContext ctx)
    {
        if (_selectedObj is not null)
        {
            SelectedObj = null;
        }
        else
        {
            Vector3 pos = _camera.ScreenToWorldPoint(_mousePos);
            Vector3Int gridPos = previewTileMap.WorldToCell(pos); // 현재 마우스 위치의 WorldPosition -> GridIndex
        
            // 가장 위의 index에 해당하는 맵의 타일을 지운다
            generatedTilemaps.Any(map =>
            {
                if (map.HasTile(gridPos))
                {
                    map.SetTile(gridPos, null);
                    return true;
                }
        
                return false;
            });
        }
    }

    private void OnCtrlPressed(InputAction.CallbackContext ctx)
    {
        // Debug.Log("Undo");
    }
    
    public void SelectObject(BuildingObjectBase obj)
    {
        // Set preview
        SelectedObj = obj;
        // on click -> draw
        // on right click -> cancel draw
    }

    private void InitializeMaps() // categoriesToCreateTilemap에 포함된 BuildingCategory마다 각각의 타일맵을 생성
    {
        foreach (BuildingCategory category in categoriesToCreateTilemap)
        {
            GameObject obj = new GameObject("Tilemap_" + category.name);
            Tilemap map = obj.AddComponent<Tilemap>();
            TilemapRenderer tilemapRenderer = obj.AddComponent<TilemapRenderer>();
            
            obj.transform.SetParent(tilemapParent);
             
            tilemapRenderer.sortingOrder = category.SortingOrder;
            category.Tilemap = map;
            
            generatedTilemaps.Add(map);
        }
        
        generatedTilemaps = FindObjectsOfType<Tilemap>().ToList();
        // sortingOrder 내림차순으로 정렬
        generatedTilemaps.Sort((a, b) =>
        {
            TilemapRenderer aRenderer = a.GetComponent<TilemapRenderer>();
            TilemapRenderer bRenderer = b.GetComponent<TilemapRenderer>();
        
            return bRenderer.sortingOrder.CompareTo(aRenderer.sortingOrder); 
        });
    }

    private void BuildUI() // categoriesToCreateUI에 포함된 UICategory마다 각각의 UI 섹션을 생성
    {
        foreach (UICategory category in categoriesToCreateUI)
        {
            var inst = Instantiate(categoryPrefab, Vector3.zero, Quaternion.identity);
            inst.transform.SetParent(uiParent, false);
            inst.name = "Category_" + category.name;
            inst.GetComponentInChildren<TextMeshProUGUI>().text = category.name;
            inst.transform.SetSiblingIndex(category.SiblingIndex);
            // inst.GetComponentInChildren<Image>().color = category.BackgroundColor;

            uiCategories[category] = inst; // 이 'category' 의 셀렉터UI 부모는 'inst' 이다.
            buildingObjectItemSlot[inst] = inst.transform.Find("Items"); // 이 셀렉터UI 안에서 아이템들이 추가되어 갈 곳은 'inst.transform.Find("Items")'
        }

        BuildingObjectBase[] buildables = GetAllBuildableObjects(); // Resources 폴더 하의 모든 BuildingObject들을 가져옴
        foreach (BuildingObjectBase buildingObject in buildables)
        {
            if (buildingObject == null)
            {
                continue;
            }
            // categoryGameObject: 이 buildingObject의 UI Category에 해당하는 셀렉터UI 상의 부모 오브젝트
            GameObject categoryGameObject = uiCategories[buildingObject.UICategory];
            // itemsParent: 셀렉터UI 하에 있는, buildingObject들을 하위 아이템으로 갖는 wrapper Object ('Items' 에 해당)
            var itemsParent = buildingObjectItemSlot[categoryGameObject];

            var inst = Instantiate(categoryItemPrefab, Vector3.zero, Quaternion.identity);
            inst.transform.SetParent(itemsParent);
            inst.name = buildingObject.name;
            inst.GetComponent<RectTransform>().sizeDelta = new Vector2(buildingObjectUISize, buildingObjectUISize);
            
            Image img = inst.GetComponent<Image>();
            Tile tile = (Tile)buildingObject.TileBase;
            img.sprite = tile.sprite;
            img.GetComponent<RectTransform>().sizeDelta = new Vector2(buildingObjectUISize, buildingObjectUISize);
            img.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            
            // 실제 배치될 BuildingObject를 셀렉터UI 상의 아이템과 연결
            var script = inst.GetComponent<BuildingButtonHandler>();
            script.Item = buildingObject;
        }
    }

    private BuildingObjectBase[] GetAllBuildableObjects()
    { 
        return Resources.LoadAll<BuildingObjectBase>("Scriptables/Buildables");
    }

    private void HandleDrawing()
    {
        if (_selectedObj != null)
        {
            switch (_selectedObj.PlaceType)
            {
                case PlaceType.Single:
                default:
                    if (_selectedObj.tag != null && _selectedObj.tag.Equals("PlayerStart"))
                    {
                        Debug.Log("PlayerStart");
                        tilemap.SetTile(playerStartCellPosition, null);
                        playerStartCellPosition = _currentGridPosition;
                        playerStartPosition = tilemap.GetCellCenterWorld(_currentGridPosition);
                    }
                    DrawItem(tilemap);
                    break;
                case PlaceType.Line:
                    LineRenderer();
                    break;
                case PlaceType.Rectangle:
                    RectangleRenderer();
                    break;
            }
        }
    }

    private void HandleDrawRelease()
    {
        if (_selectedObj != null)
        {
            if (_selectedObj.PlaceType == PlaceType.Line ||
                _selectedObj.PlaceType == PlaceType.Rectangle)
            {
                DrawBounds(tilemap); 
                previewTileMap.ClearAllTiles();
            }
        }
    }

    private void RectangleRenderer()
    {
        previewTileMap.ClearAllTiles();

        bounds.xMin = _currentGridPosition.x < holdStartPosiiton.x ? _currentGridPosition.x : holdStartPosiiton.x;
        bounds.xMax = _currentGridPosition.x > holdStartPosiiton.x ? _currentGridPosition.x : holdStartPosiiton.x;
        bounds.yMin = _currentGridPosition.y < holdStartPosiiton.y ? _currentGridPosition.y : holdStartPosiiton.y;
        bounds.yMax = _currentGridPosition.y > holdStartPosiiton.y ? _currentGridPosition.y : holdStartPosiiton.y;
        
        DrawBounds(previewTileMap);
    }

    private void LineRenderer()
    {
        previewTileMap.ClearAllTiles();

        float diffX = Mathf.Abs(_currentGridPosition.x - holdStartPosiiton.x);
        float diffY = Mathf.Abs(_currentGridPosition.y - holdStartPosiiton.y);

        if (diffX >= diffY) // 가로
        {
            bounds.xMin = _currentGridPosition.x < holdStartPosiiton.x ? _currentGridPosition.x : holdStartPosiiton.x;
            bounds.xMax = _currentGridPosition.x > holdStartPosiiton.x ? _currentGridPosition.x : holdStartPosiiton.x;
            bounds.yMin = holdStartPosiiton.y;
            bounds.yMax = holdStartPosiiton.y;
        }
        else // 세로
        {
            bounds.xMin = holdStartPosiiton.x;
            bounds.xMax = holdStartPosiiton.x;
            bounds.yMin = _currentGridPosition.y < holdStartPosiiton.y ? _currentGridPosition.y : holdStartPosiiton.y;
            bounds.yMax = _currentGridPosition.y > holdStartPosiiton.y ? _currentGridPosition.y : holdStartPosiiton.y;
        }
        
        DrawBounds(previewTileMap);
    }

    private void DrawBounds(Tilemap map) // Bounds로 설정된 직사각형 영역에 setTile
    {
        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                map.SetTile(new Vector3Int(x, y, 0), _selectedObj.TileBase);
            }
        }
    }

    private void DrawItem(Tilemap targetMap)
    {
        targetMap.SetTile(_currentGridPosition, _selectedObj.TileBase);
    }
}
