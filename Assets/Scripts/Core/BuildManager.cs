using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;
    
    [Header("Build Settings")]
    public Material hoverMaterial;
    public Material buildableMaterial;
    public Material notBuildableMaterial;
    
    private TowerData towerToBuild;
    private Node selectedNode;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public bool CanBuild { get { return towerToBuild != null; } }
    public bool HasMoney { get { return GameManager.Instance.currentMoney >= towerToBuild.cost; } }
    
    public void SelectTowerToBuild(TowerData tower)
    {
        towerToBuild = tower;
        DeselectNode();
    }
    
    public void SelectNode(Node node)
    {
        if (selectedNode == node)
        {
            DeselectNode();
            return;
        }
        
        selectedNode = node;
        towerToBuild = null;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNodeUI(node);
        }
    }
    
    public void DeselectNode()
    {
        selectedNode = null;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideNodeUI();
        }
    }
    
    public TowerData GetTowerToBuild()
    {
        return towerToBuild;
    }
    
    public bool BuildTowerOn(Node node)
    {
        if (GameManager.Instance.currentMoney < towerToBuild.cost)
        {
            Debug.Log("Not enough money to build that!");
            return false;
        }
        
        GameManager.Instance.SpendMoney(towerToBuild.cost);
        
        GameObject tower = Instantiate(towerToBuild.prefab, node.GetBuildPosition(), Quaternion.identity);
        node.tower = tower;
        
        Tower towerScript = tower.GetComponent<Tower>();
        if (towerScript != null)
        {
            towerScript.Initialize(towerToBuild);
        }
        
        Debug.Log("Tower built! Money left: " + GameManager.Instance.currentMoney);
        return true;
    }
    
    public void SellTower()
    {
        if (selectedNode == null || selectedNode.tower == null)
            return;
        
        Tower towerScript = selectedNode.tower.GetComponent<Tower>();
        int sellValue = towerScript.GetSellValue();
        
        GameManager.Instance.AddMoney(sellValue);
        
        Destroy(selectedNode.tower);
        selectedNode.tower = null;
        
        DeselectNode();
    }
    
    public void UpgradeTower()
    {
        if (selectedNode == null || selectedNode.tower == null)
            return;
        
        Tower towerScript = selectedNode.tower.GetComponent<Tower>();
        if (towerScript.CanUpgrade() && GameManager.Instance.SpendMoney(towerScript.towerData.upgradeCost))
        {
            towerScript.Upgrade();
        }
    }
}