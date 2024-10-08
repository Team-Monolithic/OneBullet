using UnityEngine;
using UnityEngine.UI;

public class BuildingButtonHandler : MonoBehaviour
{
    [SerializeField] private BuildingObjectBase item;
    private Button _button;

    private BuildingCreator buildingCreator;

    public BuildingObjectBase Item
    {
        get => item;
        set => item = value;
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(ButtonClicked);
        buildingCreator = BuildingCreator.GetInstance();
    }

    private void ButtonClicked()
    {
        buildingCreator.SelectObject(item);
    }
}
