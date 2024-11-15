using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

static class Constants
{
    public const int MAX_TRIGGER_NUM = 10;
}

public class ScriptingManager : Singleton<ScriptingManager>
{
    [SerializeField] private RectTransform layoutGroup;
    [SerializeField] private GameObject tileImage;
    [SerializeField] private GameObject tileName;

    [SerializeField] private GameObject tileInfo;
    [SerializeField] private GameObject selectTileText;
    
    [SerializeField] private GameObject eventCategoryPrefab;
    [SerializeField] private GameObject eventCategoryItemPrefab;
    [SerializeField] private GameObject eventUIPrefab;
    [SerializeField] private Transform eventCategoryParent;
    [SerializeField] private Transform eventsParent;
    [SerializeField] private Button eventAddButton;

    [SerializeField] private GameObject actionCategoryPrefab;
    [SerializeField] private GameObject actionCategoryItemPrefab;
    [SerializeField] private GameObject actionUIPrefab;
    [SerializeField] private Transform actionCategoryParent;

    [SerializeField] public GameObject actionCategoryTemp; // 미리 만들어 놓는 카테고리들의 부모 오브젝트

    private Dictionary<EventCategorySO, GameObject> eventCategories = new Dictionary<EventCategorySO, GameObject>();
    private Dictionary<EventCategorySO, Transform> eventCategorySlot = new Dictionary<EventCategorySO, Transform>();
    private Dictionary<ActionCategorySO, GameObject> actionCategories = new Dictionary<ActionCategorySO, GameObject>();
    private Dictionary<ActionCategorySO, Transform> actionCategorySlot = new Dictionary<ActionCategorySO, Transform>();
    private BuildingTile selectedTile;

    public BuildingTile SelectedTile
    {
        get => selectedTile;
        set => selectedTile = value;
    }

    public void ToggleSelectedTileMode(bool selected)
    {
        tileInfo.gameObject.SetActive(selected);
        selectTileText.gameObject.SetActive(!selected);
    }

    public void SetTileInfo(BuildingTile buildingObjectBase)
    {
        Tile tile = (Tile)buildingObjectBase.baseSO.TileBase;
        tileImage.GetComponent<Image>().sprite = tile.sprite;
        tileName.GetComponent<TextMeshProUGUI>().text = buildingObjectBase.baseSO.DisplayName;
    }

    public void SetTileEvents(BuildingTile buildingTile)
    {
        foreach (Transform child in eventsParent)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Event tileEvent in buildingTile.events)
        {
            MakeEventUI(tileEvent);
        }
    }

    // 타일이 가진 각각의 이벤트를 UI로 뿌린다. 이벤트 프리팹을 이용해 Instantiate하고 이후 makeActionUI()로 액션들을 추가한다.
    public void MakeEventUI(Event tileEvent) 
    {
        GameObject eventGameObject = Instantiate(eventUIPrefab);
        eventGameObject.transform.SetParent(eventsParent);
        eventGameObject.GetComponent<EventItem>().TargetEvent = tileEvent;
        eventGameObject.GetComponent<EventItem>().eventTitleText.text = tileEvent.eventSO.EventDisplayName;
        eventGameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        eventGameObject.GetComponent<EventItem>().AddActionCategory();
        eventGameObject.GetComponent<EventItem>().MakePropertyUI(tileEvent);
        eventGameObject.GetComponent<EventItem>().MakeActionUI(tileEvent); // 이 이벤트가 가진 액션들을 생성. event->actions 참조함
    }

    private void Start()
    {
        EventPublisher.GetInstance().TriggerEvent(0); // 0번은 '게임 시작 시' 트리거
        BuildEventCategories();
        BuildActionCategories();
    }

    private void Update()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
    }

    private void BuildEventCategories() // Scriptable Object들로부터 이벤트 카테고리를 추가
    {
        EventCategorySO[] categories = Resources.LoadAll<EventCategorySO>("Scriptables/EventCategories");
        foreach (EventCategorySO category in categories)
        {
            GameObject categoryGameObject = Instantiate(eventCategoryPrefab);
            categoryGameObject.transform.SetParent(eventCategoryParent);
            categoryGameObject.name = "EventCategory_" + category.name;
            categoryGameObject.GetComponentInChildren<TextMeshProUGUI>().text = category.name;
            categoryGameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            
            eventCategories[category] = categoryGameObject;
            eventCategorySlot[category] = categoryGameObject.transform.Find("Items");
        }
        
        EventSO[] events = Resources.LoadAll<EventSO>("Scriptables/Events");
        foreach (EventSO ev in events)
        {
            GameObject eventGameObject = Instantiate(eventCategoryItemPrefab);
            eventGameObject.transform.SetParent(eventCategorySlot[ev.EventCategory]);
            eventGameObject.name = ev.name;
            eventGameObject.GetComponentInChildren<TextMeshProUGUI>().text = ev.name;
            eventGameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            eventGameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                EventCategoryButtonClicked(ev);
            });
        }
    }
    private void BuildActionCategories() // 액션 카테고리를 추가. (임시 오브젝트에 만들어 두고 사용)
    {
        ActionCategorySO[] categories = Resources.LoadAll<ActionCategorySO>("Scriptables/ActionCategories");
        foreach (ActionCategorySO category in categories)
        {
            GameObject categoryGameObject = Instantiate(actionCategoryPrefab);
            categoryGameObject.transform.SetParent(actionCategoryTemp.transform);
            categoryGameObject.name = "ActionCategory_" + category.name;
            categoryGameObject.GetComponentInChildren<TextMeshProUGUI>().text = category.name;
            categoryGameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            actionCategories[category] = categoryGameObject;
            actionCategorySlot[category] = categoryGameObject.transform.Find("Items");
        }
        
        ActionSO[] actions = Resources.LoadAll<ActionSO>("Scriptables/Actions");
        foreach (ActionSO action in actions)
        {
            GameObject actionGameObject = Instantiate(actionCategoryItemPrefab);
            actionGameObject.transform.SetParent(actionCategorySlot[action.ActionCategory]);
            actionGameObject.name = action.name;
            actionGameObject.GetComponentInChildren<TextMeshProUGUI>().text = action.name;
            actionGameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            
            // 실제 action 연결하기
            actionGameObject.GetComponent<ActionHandler>()._actionSO = action;
        }
    }

    public void EventAddButtonClicked()
    {
        eventCategoryParent.gameObject.SetActive(true);
        eventAddButton.gameObject.SetActive(false);
    }
    
    public void EventCategoryButtonClicked(EventSO eventSO) // 새로운 이벤트를 생성
    {
        eventCategoryParent.gameObject.SetActive(false);
        eventAddButton.gameObject.SetActive(true);

        Event newEvent = new Event();
        selectedTile.events.Add(newEvent);
        newEvent.eventSO = eventSO;
        newEvent.ownerTile = selectedTile; // 현재 선택된 tile을 ownerTile로 설정 -> 이후 프로퍼티 설정 시에 참조할 수 있도록 함
        MakeEventUI(newEvent);
    }
}
public class Event
{
    public EventSO eventSO;
    public Dictionary<string, EventProperty> properties = new Dictionary<string, EventProperty>(); // 이벤트가 갖는 프로퍼티의 실질 값
    public List<Action> actions = new List<Action>();
    public BuildingTile ownerTile; // 어떤 타일에 부착된 event인지

    public void AddAction(Action inAction)
    {
        actions.Add(inAction);
    }
}

public class Action
{
    public ActionSO actionSO;
    public Event ownerEvent;
    public Dictionary<string, ActionProperty> properties = new Dictionary<string, ActionProperty>();

    public void executeAction()
    {
        actionSO.Execute(in properties, ref ownerEvent.ownerTile);
    }
}

public delegate void TriggerHandler();

public class EventPublisher : Singleton<EventPublisher>
{
    private TriggerHandler[] triggerHandlers = new TriggerHandler[Constants.MAX_TRIGGER_NUM];

    public void TriggerEvent(int idx)
    {
        if (0 < idx && idx < Constants.MAX_TRIGGER_NUM && triggerHandlers[idx] != null) // 0번은 '게임 시작 시' 트리거
        {
            triggerHandlers[idx].Invoke();
        }
    }

    public void RegisterTrigger(int idx, TriggerHandler handler)
    {
        if (IsRegistered(idx, handler) == true)
        {
            return;
        }
        
        if (0 < idx && idx < Constants.MAX_TRIGGER_NUM) // 0번은 '게임 시작 시' 트리거
        {
            triggerHandlers[idx] += handler;
        }
    }

    public void UnregisterTrigger(int idx, TriggerHandler handler)
    {
        if (IsRegistered(idx, handler) == false)
        {
            return;
        }

        if (0 < idx && idx < Constants.MAX_TRIGGER_NUM) // 0번은 '게임 시작 시' 트리거
        {
            triggerHandlers[idx] -= handler;
        }
    }

    public bool IsRegistered(int idx, TriggerHandler handler)
    {
        if (triggerHandlers[idx] != null)   
        {
            foreach (Delegate existingHandler in triggerHandlers[idx].GetInvocationList())
            {
                // 해당 델리게이트가 이미 바인딩되어 있다면 아무 작업도 하지 않는다
                if (existingHandler.Method == handler.Method && existingHandler.Target == handler.Target)
                {
                    return true;
                }
            }
        }
        return false;
    }
}