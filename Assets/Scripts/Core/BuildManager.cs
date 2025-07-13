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
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public bool CanBuild { get { return towerToBuild != null; } }
    public bool HasMoney { get { return towerToBuild != null && GameManager.Instance.currentMoney >= towerToBuild.cost; } }
    
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
        if (UIManager.Instance != null) UIManager.Instance.ShowNodeUI(node);
    }
    
    public void DeselectNode()
    {
        selectedNode = null;
        if (UIManager.Instance != null) UIManager.Instance.HideNodeUI();
    }
    
    public TowerData GetTowerToBuild() { return towerToBuild; }
    
    public bool BuildTowerOn(Node node)
    {
        if (towerToBuild == null)
        {
            Debug.LogError("No tower selected to build!");
            return false;
        }
        
        if (towerToBuild.prefab == null)
        {
            Debug.LogError($"Tower {towerToBuild.name} has no prefab assigned!");
            return false;
        }
        
        if (node == null)
        {
            Debug.LogError("Node is null!");
            return false;
        }
        
        if (node.tower != null)
        {
            Debug.LogWarning($"Node already has a tower: {node.tower.name}");
            return false;
        }
        
        if (GameManager.Instance.currentMoney < towerToBuild.cost)
        {
            Debug.Log("Not enough money to build that!");
            return false;
        }
        
        GameManager.Instance.SpendMoney(towerToBuild.cost);
        
        Vector3 buildPos = node.GetBuildPosition();
        Debug.Log($"Building {towerToBuild.name} at position {buildPos}");
        
        GameObject tower = Instantiate(towerToBuild.prefab, buildPos, Quaternion.identity);
        if (tower == null)
        {
            Debug.LogError("Failed to instantiate tower prefab!");
            GameManager.Instance.AddMoney(towerToBuild.cost); // Refund money
            return false;
        }
        
        node.tower = tower;
        GameManager.Instance.RegisterTower(tower);
        tower.transform.localScale = Vector3.one * 1.0f;
        
        Tower towerScript = tower.GetComponent<Tower>();
        if (towerScript != null) 
        {
            towerScript.Initialize(towerToBuild);
            Debug.Log($"Tower {tower.name} initialized successfully");
        }
        else
        {
            Debug.LogWarning($"Tower prefab {tower.name} missing Tower component!");
        }
        
        Debug.Log($"Tower built! Money left: {GameManager.Instance.currentMoney}, Tower active: {tower.activeInHierarchy}");
        return true;
    }
    
    public void SellTower()
    {
        if (selectedNode == null || selectedNode.tower == null) return;
        
        Tower towerScript = selectedNode.tower.GetComponent<Tower>();
        int sellValue = towerScript.GetSellValue();
        
        GameManager.Instance.AddMoney(sellValue);
        
        GameManager.Instance.UnregisterTower(selectedNode.tower); // GameManager에서 타워 제거
        Destroy(selectedNode.tower);
        selectedNode.tower = null;
        
        DeselectNode();
    }
    
    public void UpgradeTower()
    {
        if (selectedNode == null || selectedNode.tower == null) return;
        
        Tower towerScript = selectedNode.tower.GetComponent<Tower>();
        if (towerScript.CanUpgrade() && GameManager.Instance.SpendMoney(towerScript.towerData.upgradeCost))
        {
            towerScript.Upgrade();
        }
    }
}