using System.Collections;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
    [SerializeField] private GameObject _shipEncounterSplashPrefab;
    
    private GameObject _canvas;
    
    private void Start()
    {
        _canvas = GameObject.Find("Canvas");
    }
    
    public void ShipEncounter()
    {
        var shipEncounterSplash = Instantiate(_shipEncounterSplashPrefab, _canvas.transform);
       
        ShipManager.Instance.BreakRandomPartOfType(1);
        
        StartCoroutine(KillObject(shipEncounterSplash, 3f));
    }
    
    private IEnumerator KillObject(Object obj, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Destroy(obj);
    }
}