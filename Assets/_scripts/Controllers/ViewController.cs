using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewController : MonoBehaviour
{
    [Header("Unit menu")]
    [SerializeField] private Image infoImage;
    [SerializeField] private Text infoText;
    [SerializeField] private GameObject unitMenu;

    [Header("GamePanel")]
    [SerializeField] private Text cashText;

    [Header("GameEndPanel")]
    [SerializeField] private GameEndView gameEndDisplay;

    GameController gameController;
    Model model;
    BuildingRTS currentSelectedBuilding;

    private void Awake()
    {
        gameController = FindObjectOfType<GameController>();
        model = FindObjectOfType<Model>();
    }
    private void Start()
    {
        ClearInfo();
    }
    private void Update()
    {
        cashText.text = $"Cash:{model.Cash}";
    }

    public void OnGameStartClicked()
    {
        gameController.StartGame();
    }
    public void OnReturnClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Preloading");
    }
    public void OnBuildClicked(int buildingType)
    {
        gameController.SpawnBuilding((BuildingType) buildingType);
    }
    public void OnPurchaseUnitClicked()
    {
        gameController.OnUnitPurchased(currentSelectedBuilding);
        UpdateUnitMenu(currentSelectedBuilding);
    }
    public void OnUpgradeUnitClicked()
    {
        gameController.OnUnitUpgraded(currentSelectedBuilding.UnitType);
        UpdateUnitMenu(currentSelectedBuilding);
    }
    public void UpdatePendingText(BuildingRTS requester, int count)
    {
        if (requester != currentSelectedBuilding)
            return;

        Text pendingText = unitMenu.transform.FindDeepChild("PendingText").GetComponent<Text>();
        pendingText.text = count.ToString();
    }

    public void OnBuildingSelected(BuildingRTS building)
    {
        currentSelectedBuilding = building;

        infoImage.sprite = Resources.Load<Sprite>("Sprites/" + building.NameRTS);
        infoText.text = building.NameRTS;

        if (building.Type == BuildingType.MainTarget || building.Type == BuildingType.Fountain)
            return;
        
        UpdatePendingText(building, building.Pending);
        UpdateUnitMenu(building);
        unitMenu.gameObject.SetActive(true);
    }
    public void OnBuildingDeselected()
    {
        currentSelectedBuilding = null;
        ClearInfo();
    }
    public void OnUnitSelected(UnitRTS unit)
    {
        infoImage.sprite = Resources.Load<Sprite>("Sprites/" + unit.NameRTS);
        infoText.text = unit.NameRTS + "\r\n" + "Level " + unit.Level;

        unitMenu.gameObject.SetActive(false);
    }

    public void OnEverythingDeselected()
    {
        currentSelectedBuilding = null;
        ClearInfo();
    }

    public void ShowGameEndDisplay(GameResult result, int points)
    {
        gameEndDisplay.UpdateView(result, points);
        gameEndDisplay.Show();
    }

    private void OnCashUpdated(int newValue)
    {
        cashText.text = "Cash: " + newValue;
        if (currentSelectedBuilding != null)
            UpdateUnitMenu(currentSelectedBuilding);
    }
    //edit this, if you want to factory more units in one building
    private void UpdateUnitMenu(BuildingRTS building)
    {
        Transform unitTransform = unitMenu.transform.FindDeepChild("Unit1");
        int unitLevel = model.GetCurrentUnitLevel(building.UnitType);

        Button buyButton = unitTransform.FindDeepChild("BuyUnitButton").GetComponent<Button>();
        buyButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + building.UnitType.ToString());
        buyButton.interactable = model.GetUnitPrice(building.UnitType, unitLevel) < model.Cash;
        buyButton.GetComponentInChildren<Text>().text = building.Pending.ToString();


        Text levelInfo = unitTransform.FindDeepChild("UnitLevelInfo").GetComponent<Text>();
        levelInfo.text = "Level " + unitLevel;

        Button upgradeButton = unitTransform.FindDeepChild("UpgradeButton").GetComponent<Button>();
        upgradeButton.interactable = (model.GetUpgradeCost() < model.Cash) && model.HasUpgrade(building.UnitType);
    }
    private void ClearInfo()
    {
        infoImage.sprite = null;
        infoText.text = string.Empty;
        unitMenu.gameObject.SetActive(false);
    }
}
