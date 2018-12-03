
using System;
using System.Linq;
using System.Text.RegularExpressions;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CrewStats : MonoBehaviour
{
    [SerializeField] private Sprite[] _roleSprites;

    public int ShipPosition;
    
    public enum MemberRole
    {
        Captain,
        Gunner,
        Medic,
        Janitor,
        Scientist
    }

    public MemberRole Role;
    
    public int Piloting;
    public int Weight;
    public int Strength;
    public int Intelligence;

    public void RandomizeStats()
    {
        var pilRand = Random.Range(0, 5);
        if (pilRand < 4)
            Piloting = Random.Range(0, 14);
        else
            Piloting = Random.Range(14, 21);
        
        var weightChange = Random.Range(0, 10);
        if (weightChange == 9)
            Weight = Random.Range(200, 301);
        else if (weightChange == 8 || weightChange == 7)
            Weight = Random.Range(165, 201);
        else
            Weight = Random.Range(90, 165);
        
        var strRand = Random.Range(0, 5);
        if (strRand < 3)
            Strength = Random.Range(0, 14);
        else
            Strength = Random.Range(14, 21);

        var intRand = Random.Range(0, 5);
        if (intRand < 4)
            Intelligence = Random.Range(0, 14);
        else
            Intelligence = Random.Range(14, 21);
    }

    public void RandomizeRole(MemberRole[] blacklistedRoles = null)
    {
        while (true)
        {
            SetRole((MemberRole) Random.Range(0, 5));
            if (blacklistedRoles == null) break;
            if (!blacklistedRoles.Contains(Role)) break;
        }
    }

    public bool SwitchRoles(MemberRole role, bool dontSubtract)
    {
        var blacklistedRoles = GameManager.Instance.GetBlacklistedRoles();
        if (blacklistedRoles.Contains(role))
        {
            return false;
        }

        if (MenuManager.Instance.MenuOpened)
        {
            Destroy(MenuManager.Instance.OpenedMenu);
            MenuManager.Instance.MenuOpened = false;
        }

        if (!dontSubtract)
            GameManager.Instance.CalcCrewMemberCounts(this);
        
        SetRole(role);
        GameManager.Instance.MoveCrewMember(gameObject);
            
        return true;
    }
    
    public void SetRole(MemberRole role)
    {
        Role = role;
        transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = _roleSprites[Math.Min(_roleSprites.Length, (int) Role)];    
    }
    
}
