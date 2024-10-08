using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public enum BuildingHistoryType
{
    Placing,
    Moving,
    Scripting
}

public class BuildingHistoryManager : Singleton<BuildingHistoryManager>
{
    [SerializeField] private Button undoButton;
    [SerializeField] private Button redoButton;
    private Stack<BuildingHistoryItem> undoStack = new Stack<BuildingHistoryItem>();
    private Stack<BuildingHistoryItem> redoStack = new Stack<BuildingHistoryItem>();

    protected override void Awake()
    {
        undoButton.onClick.AddListener(Undo);
        redoButton.onClick.AddListener(Redo);
    }

    private void Start()
    {
        UpdateButtonActivation();
    }

    public void AddItem(BuildingHistoryItem entry)
    {
        undoStack.Push(entry);
        UpdateButtonActivation();
    }

    public void ClearRedoStack()
    {
        redoStack.Clear();
        UpdateButtonActivation();
    }

    private void UpdateButtonActivation()
    {
        undoButton.interactable = undoStack.Count > 0;
        redoButton.interactable = redoStack.Count > 0;
    }
    
    public void Undo()
    {
        if (undoStack.Count <= 0)
        {
            return;
        }
        
        BuildingHistoryItem poppedItem = undoStack.Pop();
        Execute(poppedItem, true);
        redoStack.Push(poppedItem);
        UpdateButtonActivation();
    }

    public void Redo()
    {
        if (redoStack.Count <= 0)
        {
            return;
        }
        
        BuildingHistoryItem poppedItem = redoStack.Pop();
        Execute(poppedItem, false);
        undoStack.Push(poppedItem);
        UpdateButtonActivation();
    }

    public void Execute(BuildingHistoryItem item, bool isUndo)
    {
        // A에서 B로 이동하는 동작에는 prev A, new B 들어있음
        if (item.BuildingHistoryType == BuildingHistoryType.Moving)
        {
            // undo: new를 지우고 prev에 넣음
            // redo: prev를 지우고 new에 넣음
            item.Map.SetTile(item.NewPosition, isUndo ? null : item.TileBase);
            item.Map.SetTile(item.PrevPosition, isUndo ? item.TileBase : null);
        }
        else if (item.BuildingHistoryType == BuildingHistoryType.Placing)
        {
            if (item.IsSingle) // 단일 설치
            {
                item.Map.SetTile(item.NewPosition, isUndo ? null : item.TileBase);
            }
            else // 범위 설치 (Line, Rectangle)
            {
                BoundsInt bound = item.Bound;
                for (int x = bound.xMin; x <= bound.xMax; x++)
                {
                    for (int y = bound.yMin; y <= bound.yMax; y++)
                    {
                        item.Map.SetTile(new Vector3Int(x, y, 0), isUndo ? null : item.TileBase);
                    }
                }
            }
        }
    }
}


// 
public class BuildingHistoryItem
{
    private BuildingHistoryType buildingHistoryType;
    private Tilemap map;
    private TileBase tileBase;
    private Vector3Int prevPosition;
    private Vector3Int newPosition;
    private BoundsInt bound;
    private bool isSingle;
    
    // todo : script 관련 멤버 추가

    public BuildingHistoryType BuildingHistoryType { get; }
    public Tilemap Map => map;
    public TileBase TileBase => tileBase;
    public Vector3Int PrevPosition => prevPosition;
    public Vector3Int NewPosition => newPosition;
    public BoundsInt Bound => bound;
    public bool IsSingle => isSingle;
    

    public BuildingHistoryItem(BuildingHistoryType type, Tilemap map, TileBase tileBase, Vector3Int prevPosition,
        Vector3Int newPosition, BoundsInt bound, bool isSingle)
    {
        this.buildingHistoryType = type;
        this.map = map;
        this.tileBase = tileBase;
        this.prevPosition = prevPosition;
        this.newPosition = newPosition;
        this.bound = bound;
        this.isSingle = isSingle;
    }
    
}