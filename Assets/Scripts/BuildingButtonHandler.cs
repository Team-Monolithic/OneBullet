using UnityEngine;
using UnityEngine.UI;

public class BuildingButtonHandler : MonoBehaviour
{
    [SerializeField] private BuildingObjectBase item;
    private Button _button;

    private BuildingManager buildingManager;

    public BuildingObjectBase Item
    {
        get => item;
        set => item = value;
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(ButtonClicked);
        buildingManager = BuildingManager.GetInstance();
    }

    private void ButtonClicked()
    {
        buildingManager.SelectObject(item);
    }
}
