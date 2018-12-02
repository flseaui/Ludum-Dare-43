using System;
using System.Collections;
using DefaultNamespace;
using UnityEngine;
using Object = UnityEngine.Object;

public class EventManager : Singleton<EventManager>
{
    [SerializeField] private GameObject _shipEncounterSplashPrefab;

    [SerializeField] private GameObject _shopPrefab;
    [SerializeField] private GameObject _shipmentSelectorPrefab;
    [SerializeField] private GameObject _shipmentCompletePrefab;
    [SerializeField] private GameObject _deathPrefab;
    
    private GameObject _canvas;
    
    private void Start()
    {
        _canvas = GameObject.Find("Canvas");
    }

    public void ShopEncounter()
    {
        if (MenuManager.Instance.MenuOpened)
        {
            MenuManager.Instance.OpenedMenu.GetComponent<CrewMemberMenu>().RoleDropdown.Hide();
            Destroy(MenuManager.Instance.OpenedMenu);
        }
        
        var shop = Instantiate(_shopPrefab, _canvas.transform);
        shop.GetComponent<Animator>().SetTrigger("ShopOpened");
        GameManager.Instance.InShop = true;
    }

    public void ShipmentEncounter()
    {
        var shipmentSelector = Instantiate(_shipmentSelectorPrefab, _canvas.transform);
        
        GameManager.Instance.InShop = true;
    }
    
    public void ShipmentCompleted()
    {
        GameManager.Instance.InShop = true;
        var shipmentComplete = Instantiate(_shipmentCompletePrefab, _canvas.transform);
        
        StartCoroutine(KillObject(shipmentComplete, 2f, delegate
        {
            GameManager.Instance.InShop = false;
            GameManager.Instance.ShipmentProgress = 0;
            ShipmentEncounter();
        }));
    }

    public void DeathScreen()
    {
        GameManager.Instance.InShop = true;
        var deathScreen = Instantiate(_deathPrefab, _canvas.transform);
    }
    
    public void ShipEncounter()
    {
        var shipEncounterSplash = Instantiate(_shipEncounterSplashPrefab, _canvas.transform);
       
        if (GameManager.Instance.NumShipEncounters == 0 || GameManager.Instance.NumShipEncounters == 1)
            ShipManager.Instance.BreakRandomPartOfType(2);
        else
            ShipManager.Instance.BreakRandomPartOfType(Math.Min(5, GameManager.Instance.NumShipEncounters));
        
        StartCoroutine(KillObject(shipEncounterSplash, 3f));
    }
    
    private IEnumerator KillObject(Object obj, float delayTime, Action onFinish = null)
    {
        yield return new WaitForSeconds(delayTime);
        Destroy(obj);
        onFinish?.Invoke();
    }
}