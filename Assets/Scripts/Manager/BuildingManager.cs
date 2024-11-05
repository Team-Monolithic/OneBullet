using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingManager : Singleton<BuildingManager>
{
    [SerializeField] private List<TilemapSO> tilemapsToCreate; // 동적으로 생성할 타일맵들의 종류
    [SerializeField] private List<TileUICategorySO> tileUICategoriesToCreate; // 동적으로 생성할 BuildingObject 카테고리
    [SerializeField] private Transform tilemapParent; // 동적으로 생성될 타일맵들의 부모 object
    [SerializeField] private Transform tileCategoryParent; // 동적으로 생성될 타일 카테고리들의 부모 object
    [SerializeField] private GameObject tileCategoryPrefab; // 동적으로 생성될 셀렉터 UI 중에서 각각의 UI 카테고리 하나를 전체로 묶음
    [SerializeField] private GameObject tileCategoryItemPrefab; // 동적으로 생성될 셀렉터 UI 중에서 각각의 BuildingObject 아이콘 하나
    [SerializeField] private Tilemap previewTilemap, defaultTilemap;
    [SerializeField] int tileCategoryItemUISize = 48;
    [SerializeField] private Button scriptingTabButton;

    private BuildingManagerInput _playerInput;
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

    private Dictionary<TileUICategorySO, GameObject>
        uiCategories = new Dictionary<TileUICategorySO, GameObject>(); // 각각의 UI 카테고리(벽, 바닥...)와 UI 카테고리 오브젝트 간의 매핑을 저장 

    private Dictionary<GameObject, Transform>
        buildingObjectItemSlot =
            new Dictionary<GameObject, Transform>(); // 각각의 BuildingObject와 UI 카테고리 오브젝트 부모 (ui category 아래의 'Items' 오브젝트) 간의 매핑을 저장
    private Dictionary<Tilemap, MapTileInfo> tileInfo = new Dictionary<Tilemap, MapTileInfo>(); // 각 타일맵의 모든 타일 정보를 딕셔너리 안의 딕셔너리로 저장
    
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
            if (_selectedObj != null && _selectedObj.Tilemap != null &&
                _selectedObj.Tilemap.Tilemap != null)
            {
                return _selectedObj.Tilemap.Tilemap;
            }

            return defaultTilemap;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _playerInput = new BuildingManagerInput();
        _camera = Camera.main;
    }

    private void Start()
    {
        InitializeMaps();
        BuildUI();
    }

    private void Update()
    {
        Vector3 pos = _camera.ScreenToWorldPoint(_mousePos);
        Vector3Int gridPos = previewTilemap.WorldToCell(pos); // 현재 마우스 위치의 WorldPosition -> GridIndex

        if (gridPos != _currentGridPosition) // currentGridPosition 정보 갱신
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

    private void OnEnable()
    {
        _playerInput.Enable();

        _playerInput.Gameplay.MousePosition.performed += OnMouseMove;
        _playerInput.Gameplay.MouseLeftClick.performed += OnLeftClick;
        _playerInput.Gameplay.MouseLeftClick.started += OnLeftClick;
        _playerInput.Gameplay.MouseLeftClick.canceled += OnLeftClick;
        _playerInput.Gameplay.MouseRightClick.performed += OnRightClick;
        _playerInput.Gameplay.Undo.performed += OnUndoPressed;
        _playerInput.Gameplay.Redo.performed += OnRedoPressed;
    }

    private void OnDisable()
    {
        _playerInput.Disable();
        _playerInput.Gameplay.MousePosition.performed -= OnMouseMove;
        _playerInput.Gameplay.MouseLeftClick.performed -= OnLeftClick;
        _playerInput.Gameplay.MouseLeftClick.started -= OnLeftClick;
        _playerInput.Gameplay.MouseLeftClick.canceled -= OnLeftClick;
        _playerInput.Gameplay.MouseRightClick.performed -= OnRightClick;
        _playerInput.Gameplay.Undo.performed -= OnUndoPressed;
        _playerInput.Gameplay.Redo.performed -= OnRedoPressed;
    }

    // 타일을 배치하기 전, 타일을 선택한 채로 마우스를 이동하면 마우스 포인터를 따라 미리보기 타일을 생성하는 기능
    private void UpdatePreview()
    {
        // 이전 위치의 타일을 삭제
        previewTilemap.SetTile(_lastGridPosition, null);
        // 현재 위치에 tileBase를 set
        previewTilemap.SetTile(_currentGridPosition, _selectedObj ? _selectedObj.TileBase : null);
    }

    private void OnMouseMove(InputAction.CallbackContext ctx)
    {
        _mousePos = ctx.ReadValue<Vector2>();
    }

    private void OnLeftClick(InputAction.CallbackContext ctx)
    {
        if (_selectedObj is not null && !EventSystem.current.IsPointerOverGameObject())
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
        // 선택한 타일이 없는 채로 타일맵을 클릭하면, 해당 위치의 타일을 선택한다.
        else if (_selectedObj is null && !EventSystem.current.IsPointerOverGameObject() && ctx.interaction is TapInteraction && ctx.phase is InputActionPhase.Started)
        {
            BuildingObjectBase selectedTile = GetTileAtPosition(_currentGridPosition);
            ScriptingManager.GetInstance().ToggleSelectedTileMode(selectedTile is not null);
            if (selectedTile is not null)
            {
                ScriptingManager.GetInstance().SetTileInfo(selectedTile); // ScriptingTab에 해당 타일의 정보를 표시함
                ScriptingManager.GetInstance().SetTileEvents(selectedTile);
                scriptingTabButton.onClick.Invoke(); // 자동으로 Scripting Tab으로 전환
                ScriptingManager.GetInstance().SelectedTile = selectedTile;
            }
            else
            {
                ScriptingManager.GetInstance().SelectedTile = null;
            }
        }
    }

    private void OnRightClick(InputAction.CallbackContext ctx)
    {
        if (_selectedObj is not null) // 선택 해제
        {
            SelectedObj = null;
        }
        else
        {
            Vector3 pos = _camera.ScreenToWorldPoint(_mousePos);
            Vector3Int gridPos = previewTilemap.WorldToCell(pos); // 현재 마우스 위치의 WorldPosition -> GridIndex

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

    private void OnUndoPressed(InputAction.CallbackContext ctx)
    {
        BuildingHistoryManager.GetInstance().Undo();
    }
    
    private void OnRedoPressed(InputAction.CallbackContext ctx)
    {
        BuildingHistoryManager.GetInstance().Redo();
    }


    public void SelectObject(BuildingObjectBase obj)
    {
        SelectedObj = obj;
    }

    private void InitializeMaps() // categoriesToCreateTilemap에 포함된 BuildingCategory마다 각각의 타일맵을 생성
    {
        foreach (TilemapSO category in tilemapsToCreate)
        {
            GameObject obj = new GameObject("Tilemap_" + category.name);
            Tilemap map = obj.AddComponent<Tilemap>();
            TilemapRenderer tilemapRenderer = obj.AddComponent<TilemapRenderer>();
            
            TilemapCollider2D tilemapCollider2D = obj.AddComponent<TilemapCollider2D>();
            Rigidbody2D rigidbody2D = obj.AddComponent<Rigidbody2D>();
            CompositeCollider2D compositeCollider2D = obj.AddComponent<CompositeCollider2D>();
            
            tilemapCollider2D.usedByComposite = true;
            rigidbody2D.bodyType = RigidbodyType2D.Static;
            rigidbody2D.simulated = category.SimulatePhysics;
            
            obj.transform.SetParent(tilemapParent);

            tilemapRenderer.sortingOrder = category.SortingOrder;
            category.Tilemap = map;

            generatedTilemaps.Add(map);
            tileInfo[map] = new MapTileInfo();
        }

        // generatedTilemaps = FindObjectsOfType<Tilemap>().ToList();
        // sortingOrder 내림차순으로 정렬 (가장 위에 표시될 타일맵의 타일부터 탐색할 수 있도록)
        generatedTilemaps.Sort((a, b) =>
        {
            TilemapRenderer aRenderer = a.GetComponent<TilemapRenderer>();
            TilemapRenderer bRenderer = b.GetComponent<TilemapRenderer>();

            return bRenderer.sortingOrder.CompareTo(aRenderer.sortingOrder);
        });
    }

    private void BuildUI() // categoriesToCreateUI에 포함된 UICategory마다 각각의 UI 섹션을 생성
    {
        foreach (TileUICategorySO category in tileUICategoriesToCreate)
        {
            var inst = Instantiate(tileCategoryPrefab, Vector3.zero, Quaternion.identity);
            inst.transform.SetParent(tileCategoryParent, false);
            inst.name = "Category_" + category.name;
            inst.GetComponentInChildren<TextMeshProUGUI>().text = category.name;
            inst.transform.SetSiblingIndex(category.SiblingIndex);
            // inst.GetComponentInChildren<Image>().color = category.BackgroundColor;

            uiCategories[category] = inst; // 이 'category' 의 셀렉터UI 부모는 'inst' 이다.
            buildingObjectItemSlot[inst] =
                inst.transform.Find("Items"); // 이 셀렉터UI 안에서 아이템들이 추가되어 갈 곳은 'inst.transform.Find("Items")'
        }

        BuildingObjectBase[] buildables = GetAllBuildableObjects(); // Resources 폴더 하의 모든 BuildingObject들을 가져옴
        foreach (BuildingObjectBase buildingObject in buildables)
        {
            if (buildingObject == null)
            {
                continue;
            }

            // categoryGameObject: 이 buildingObject의 UI Category에 해당하는 셀렉터UI 상의 부모 오브젝트
            GameObject categoryGameObject = uiCategories[buildingObject.TileCategory];
            // itemsParent: 셀렉터UI 하에 있는, buildingObject들을 하위 아이템으로 갖는 wrapper Object ('Items' 에 해당)
            var itemsParent = buildingObjectItemSlot[categoryGameObject];

            var inst = Instantiate(tileCategoryItemPrefab, Vector3.zero, Quaternion.identity);
            inst.transform.SetParent(itemsParent);
            inst.name = buildingObject.name;
            inst.GetComponent<RectTransform>().sizeDelta = new Vector2(tileCategoryItemUISize, tileCategoryItemUISize);

            Image img = inst.GetComponent<Image>();
            Tile tile = (Tile)buildingObject.TileBase;
            img.sprite = tile.sprite;
            img.GetComponent<RectTransform>().sizeDelta = new Vector2(tileCategoryItemUISize, tileCategoryItemUISize);
            img.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            // 실제 배치될 BuildingObject를 셀렉터UI 상의 아이템과 연결
            var script = inst.GetComponent<TileUIHandler>();
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
                        tilemap.SetTile(playerStartCellPosition, null);
                        playerStartCellPosition = _currentGridPosition;
                        playerStartPosition = tilemap.GetCellCenterWorld(_currentGridPosition);
                    }

                    if (_selectedObj.GetType() == typeof(BuildingObjectInteractable)) // 버튼 등 상호작용 가능한 물체일 경우 (작업예정)
                    {
                        BuildingObjectInteractable selectedInteractable = (BuildingObjectInteractable)_selectedObj;
                        Debug.Log(selectedInteractable.EventIndex);
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
                previewTilemap.ClearAllTiles();
            }
        }
    }

    private void RectangleRenderer()
    {
        previewTilemap.ClearAllTiles();

        bounds.xMin = _currentGridPosition.x < holdStartPosiiton.x ? _currentGridPosition.x : holdStartPosiiton.x;
        bounds.xMax = _currentGridPosition.x > holdStartPosiiton.x ? _currentGridPosition.x : holdStartPosiiton.x;
        bounds.yMin = _currentGridPosition.y < holdStartPosiiton.y ? _currentGridPosition.y : holdStartPosiiton.y;
        bounds.yMax = _currentGridPosition.y > holdStartPosiiton.y ? _currentGridPosition.y : holdStartPosiiton.y;

        DrawBounds(previewTilemap);
    }

    private void LineRenderer()
    {
        previewTilemap.ClearAllTiles();

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

        DrawBounds(previewTilemap);
    }

    private void DrawBounds(Tilemap targetMap) // Bounds로 설정된 직사각형 영역에 setTile
    {
        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                targetMap.SetTile(position, _selectedObj.TileBase);
                
                if (targetMap != previewTilemap)
                {
                    tileInfo[targetMap].SetTileAtPosition(position, _selectedObj);
                }
            }
        }

        if (targetMap != previewTilemap)
        {
            BuildingHistoryItem item = new BuildingHistoryItem(BuildingHistoryType.Placing, targetMap, _selectedObj.TileBase,
                Vector3Int.zero, Vector3Int.zero, bounds, false);
            BuildingHistoryManager bh = BuildingHistoryManager.GetInstance();
            bh.AddItem(item);
            bh.ClearRedoStack();
        }
    }

    private void DrawItem(Tilemap targetMap)
    {
        targetMap.SetTile(_currentGridPosition, _selectedObj.TileBase);
        BuildingHistoryItem item = new BuildingHistoryItem(BuildingHistoryType.Placing, targetMap,
            _selectedObj.TileBase, Vector3Int.zero, _currentGridPosition, new BoundsInt(), true);
        BuildingHistoryManager bh = BuildingHistoryManager.GetInstance();
        bh.AddItem(item);
        bh.ClearRedoStack();
        
        tileInfo[targetMap].SetTileAtPosition(_currentGridPosition, _selectedObj);
    }

    private BuildingObjectBase GetTileAtPosition(Vector3Int gridPosition)
    {
        foreach (Tilemap map in generatedTilemaps)
        {
            if (tileInfo[map].GetTileAtPosition(gridPosition) is not null)
            {
                BuildingObjectBase tile = tileInfo[map].GetTileAtPosition(gridPosition);
                if (tile is not null)
                {
                    return tile;
                }
            }
        }
        return null;
    }
}

public class MapTileInfo // 각 타일맵의 모든 타일 정보를 기록하고 있는 딕셔너리 클래스
{
    private Dictionary<Vector3Int, BuildingObjectBase> tileInfo = new Dictionary<Vector3Int, BuildingObjectBase>();

    public BuildingObjectBase GetTileAtPosition(Vector3Int gridPosition)
    {
        BuildingObjectBase objectBase;
        tileInfo.TryGetValue(gridPosition, out objectBase);
        return objectBase;
    }

    public void SetTileAtPosition(Vector3Int gridPosition, BuildingObjectBase targetTile)
    {
        tileInfo[gridPosition] = targetTile;
    }
}