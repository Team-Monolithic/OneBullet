using UnityEngine;
using UnityEngine.UI;

public class TileUIHandler : MonoBehaviour
{
    [SerializeField] private BuildingObjectBaseSO baseSO;
    private Button _button;

    private BuildingManager buildingManager;

    public BuildingObjectBaseSO BaseSO
    {
        get => baseSO;
        set => baseSO = value;
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(ButtonClicked);
        buildingManager = BuildingManager.GetInstance();
    }

    private void ButtonClicked()
    {
        buildingManager.selectedObj = baseSO;
    }
}
