using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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

    [SerializeField] public GameObject actionCategoryTemp;
    
    private Dictionary<int, List<ActionSO>> triggerActions;
    private Dictionary<EventCategorySO, GameObject> eventCategories = new Dictionary<EventCategorySO, GameObject>();
    private Dictionary<EventCategorySO, Transform> eventCategorySlot = new Dictionary<EventCategorySO, Transform>();
    private Dictionary<ActionCategorySO, GameObject> actionCategories = new Dictionary<ActionCategorySO, GameObject>();
    private Dictionary<ActionCategorySO, Transform> actionCategorySlot = new Dictionary<ActionCategorySO, Transform>();
    private BuildingObjectBase selectedTile;

    public BuildingObjectBase SelectedTile
    {
        get => selectedTile;
        set => selectedTile = value;
    }

    public void ToggleSelectedTileMode(bool selected)
    {
        tileInfo.gameObject.SetActive(selected);
        selectTileText.gameObject.SetActive(!selected);
    }

    public void SetTileInfo(BuildingObjectBase buildingObjectBase)
    {
        Tile tile = (Tile)buildingObjectBase.TileBase;
        tileImage.GetComponent<Image>().sprite = tile.sprite;
        tileName.GetComponent<TextMeshProUGUI>().text = buildingObjectBase.DisplayName;
    }

    public void SetTileEvents(BuildingObjectBase buildingObjectBase)
    {
        foreach (Transform child in eventsParent)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Event tileEvent in buildingObjectBase.events)
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
        eventGameObject.GetComponent<EventItem>().eventTitleText.text = tileEvent.eventType.ToString();
        eventGameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        eventGameObject.GetComponent<EventItem>().AddActionCategory();
        eventGameObject.GetComponent<EventItem>().MakeActionUI(tileEvent); // 이 이벤트가 가진 액션들을 생성. event->actions 참조함
    }

    private void Start()
    {
        // FireTrigger(0);
        BuildEventCategories();
        BuildActionCategories();
    }

    private void Update()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
    }

    public void FireTrigger(int triggerNumber)
    {
        if (triggerActions.ContainsKey(triggerNumber) == false)
        {
            return;
        }
        
        foreach (ActionSO action in triggerActions[triggerNumber])
        {
            ExecuteAction(action);
        }
    }

    public void ExecuteAction(ActionSO targetAction)
    {
        Debug.Log(targetAction.ActionDisplayName + " Executed");
    }

    private void BuildEventCategories()
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
                EventCategoryButtonClicked(ev.EventType);
            });
        }
    }
    // Action Category를 미리 만들어 두고 재사용
    private void BuildActionCategories()
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
    
    public void EventCategoryButtonClicked(EventType eventType) // 새로운 이벤트를 생성
    {
        eventCategoryParent.gameObject.SetActive(false);
        eventAddButton.gameObject.SetActive(true);

        Event newEvent = new Event(eventType);
        selectedTile.events.Add(newEvent);
        newEvent.ownerTile = selectedTile; // 현재 선택된 tile을 ownerTile로 설정 -> 이후 프로퍼티 설정 시에 참조할 수 있도록 함
        MakeEventUI(newEvent);
    }
}
public class Event
{
    public EventType eventType;
    public List<Action> actions = new List<Action>();
    public BuildingObjectBase ownerTile; // 어떤 타일에 부착된 event인지
    
    public Event(EventType type)
    {
        eventType = type;
    }

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

}
