using System;
using System.Collections;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShipEncounter();
        }
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

        shipmentComplete.transform.Find("MoneyText").GetComponent<TextMeshProUGUI>().text =
            GameManager.Instance.TargetShipment.Price.ToString() + '$';
        
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

        var gunPower = 0;
        foreach (var gunner in GameManager.Instance.GunnersStats)
        {
            gunPower += gunner.Strength;
        }

        int chance = 0;
        int subtract = 0;
        switch (gunPower)
        {
            case 10:
                chance = 1;
                subtract = 1;
                break;
            case 20:
                chance = 2;
                subtract = 1;
                break;
            case 30:
                chance = 2;
                subtract = 2;
                
                break;
            case 40:
                chance = 2;
                subtract = 3;
                
                break;
            case 50:
                chance = 3;
                subtract = 3;
                break;
            case 60:
                chance = 3;
                subtract = 4;
                break;
            case 70:
                chance = 3;
                subtract = 5;
                break;
            case 80:
                chance = 3;
                subtract = 10;
                break;    
        }

        int numBreaks = Math.Min(5, GameManager.Instance.NumShipEncounters);
        
        var random = UnityEngine.Random.Range(0, 4);
        if (random <= chance)
        {
            numBreaks -= subtract;
        }
        
        if (GameManager.Instance.NumShipEncounters == 0 || GameManager.Instance.NumShipEncounters == 1)
            ShipManager.Instance.BreakRandomPartOfType(2);
        else
            ShipManager.Instance.BreakRandomPartOfType(numBreaks);
        
        StartCoroutine(KillObject(shipEncounterSplash, 3f));
    }
    
    private IEnumerator KillObject(Object obj, float delayTime, Action onFinish = null)
    {
        yield return new WaitForSeconds(delayTime);
        Destroy(obj);
        onFinish?.Invoke();
    }
}