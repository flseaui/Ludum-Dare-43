
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
        Piloting = Random.Range(0, 10);
        
        var weightChange = Random.Range(0, 10);
        if (weightChange == 9)
            Weight = Random.Range(200, 300);
        else
            Weight = Random.Range(90, 200);

        Strength = Random.Range(0, 20);

        Intelligence = Random.Range(0, 20);
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

    public bool SwitchRoles(MemberRole role)
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
