using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerSelectUI : MonoBehaviour
{
    [Header("Tower Buttons")]
    public Button[] towerButtons;
    public TowerData[] towerData;
    
    [Header("UI Components")]
    public TextMeshProUGUI[] costTexts;
    public Image[] towerIcons;
    
    private void Start()
    {
        SetupButtons();
        UpdateButtonStates();
    }
    
    private void Update()
    {
        UpdateButtonStates();
    }
    
    private void SetupButtons()
    {
        for (int i = 0; i < towerButtons.Length; i++)
        {
            if (i < towerData.Length)
            {
                int index = i;
                towerButtons[i].onClick.AddListener(() => SelectTower(index));
                
                if (costTexts.Length > i && costTexts[i] != null)
                {
                    costTexts[i].text = "$" + towerData[i].cost.ToString();
                }
                
                if (towerIcons.Length > i && towerIcons[i] != null && towerData[i].icon != null)
                {
                    towerIcons[i].sprite = towerData[i].icon;
                }
            }
        }
    }
    
    private void UpdateButtonStates()
    {
        for (int i = 0; i < towerButtons.Length; i++)
        {
            if (i < towerData.Length)
            {
                bool canAfford = GameManager.Instance.currentMoney >= towerData[i].cost;
                towerButtons[i].interactable = canAfford && !GameManager.GameIsOver;
                
                if (costTexts.Length > i && costTexts[i] != null)
                {
                    costTexts[i].color = canAfford ? Color.green : Color.red;
                }
            }
        }
    }
    
    private void SelectTower(int index)
    {
        if (index < towerData.Length)
        {
            BuildManager.Instance.SelectTowerToBuild(towerData[index]);
        }
    }
}