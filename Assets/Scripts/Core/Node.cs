using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("Node Settings")]
    public Color hoverColor = Color.white;
    public Color notEnoughMoneyColor = Color.red;
    public Vector3 positionOffset = Vector3.zero;
    
    [HideInInspector]
    public GameObject tower;
    
    private Renderer rend;
    private Color startColor;
    
    private void Start()
    {
        rend = GetComponent<Renderer>();
        startColor = rend.material.color;
    }
    
    public Vector3 GetBuildPosition()
    {
        return transform.position + positionOffset;
    }
    
    private void OnMouseDown()
    {
        if (GameManager.GameIsOver) return;
        
        if (tower != null)
        {
            BuildManager.Instance.SelectNode(this);
            return;
        }
        
        if (!BuildManager.Instance.CanBuild) return;
        
        BuildTower();
    }
    
    private void BuildTower()
    {
        if (BuildManager.Instance.BuildTowerOn(this))
        {
            Debug.Log("Tower built successfully!");
        }
        else
        {
            Debug.Log("Cannot build tower!");
        }
    }
    
    private void OnMouseEnter()
    {
        if (GameManager.GameIsOver) return;
        
        if (!BuildManager.Instance.CanBuild) return;
        
        if (BuildManager.Instance.HasMoney)
        {
            rend.material.color = hoverColor;
        }
        else
        {
            rend.material.color = notEnoughMoneyColor;
        }
    }
    
    private void OnMouseExit()
    {
        rend.material.color = startColor;
    }
}