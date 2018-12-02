using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Shop : MonoBehaviour
{
    [SerializeField] private Image[] _cageImages;
    [SerializeField] private Sprite[] _crewSprites;
    [SerializeField] private TextMeshProUGUI[] _pricesText;
    [SerializeField] private TextMeshProUGUI _currentMoney;
    [SerializeField] private TextMeshProUGUI _crewCounter;
    
    private int[] _prices;

    private int[] _roles;

    private bool[] _purchased;
    private int _tempNewCrewCount;
    
    private List<CrewStats.MemberRole> _purchasedCrew;
    
    private void Start()
    {
        _purchasedCrew = new List<CrewStats.MemberRole>();
        _purchased = new bool[4];
        _prices = new []
        {
            Random.Range(10, 100),
            Random.Range(10, 100),
            Random.Range(10, 100),
            Random.Range(10, 100)
        };
        _roles = new []
        {
            Random.Range(0, 5),
            Random.Range(0, 5),
            Random.Range(0, 5),
            Random.Range(0, 5)
        };
        
        _cageImages[0].sprite = _crewSprites[_roles[0]];
        _cageImages[1].sprite = _crewSprites[_roles[1]];
        _cageImages[2].sprite = _crewSprites[_roles[2]];
        _cageImages[3].sprite = _crewSprites[_roles[3]];
        _pricesText[0].text = _prices[0].ToString() + '$';
        _pricesText[1].text = _prices[1].ToString() + '$';
        _pricesText[2].text = _prices[2].ToString() + '$';
        _pricesText[3].text = _prices[3].ToString() + '$';
        
        _tempNewCrewCount = 0;
    }

    public void Update()
    {
        _currentMoney.text = GameManager.Instance.Money.ToString() + '$';
        _crewCounter.text = $"{GameManager.Instance.CrewCount + _tempNewCrewCount} / {GameManager.Instance.MaxCrewMembers}";
    }
    
    public void PurchaseCrewMember(int cageNum)
    {        
        if (GameManager.Instance.Money >= _prices[cageNum]
            && GameManager.Instance.CrewCount + _tempNewCrewCount < GameManager.Instance.MaxCrewMembers
            && _purchased[cageNum] == false)
        {
            _purchased[cageNum] = true;
            _pricesText[cageNum].text = "___";
            _cageImages[cageNum].color = Color.clear;
            GameManager.Instance.Money -= _prices[cageNum];
            ++_tempNewCrewCount;
            _purchasedCrew.Add((CrewStats.MemberRole) _roles[cageNum]);
        }
    }

    public void CloseShop()
    {
        GameManager.Instance.InShop = false;
        Destroy(gameObject);
        foreach (var role in _purchasedCrew)
        {
            GameManager.Instance.SpawnCrewMember(GenStatsFromRole(role));
        }
    }

    public GameManager.CrewStatsStruct RandomizeStats()
    {
        var piloting = 0;
        var weight = 0;
        var strength = 0;
        var intelligence = 0;
        
        piloting = Random.Range(0, 10);
        
        var weightChange = Random.Range(0, 10);
        if (weightChange == 9)
            weight = Random.Range(200, 300);
        else
            weight = Random.Range(90, 200);

        strength = Random.Range(0, 20);

        intelligence = Random.Range(0, 20);

        return new GameManager.CrewStatsStruct
        {
            Piloting = piloting,
            Weight = weight,
            Strength = strength,
            Intelligence = intelligence,
            Role = CrewStats.MemberRole.Janitor
        };
    }
    
    private GameManager.CrewStatsStruct GenStatsFromRole(CrewStats.MemberRole role)
    {
        var baseStruct = RandomizeStats();
        switch (role)
        {
            case CrewStats.MemberRole.Captain:
                baseStruct.Piloting = Random.Range(7, 10);
                break;
            case CrewStats.MemberRole.Gunner:
                baseStruct.Strength = Random.Range(10, 20);
                break;
            case CrewStats.MemberRole.Medic:
                baseStruct.Intelligence = Random.Range(10, 20);
                break;
            case CrewStats.MemberRole.Janitor:

                break;
            case CrewStats.MemberRole.Scientist:
                baseStruct.Intelligence = Random.Range(15, 20);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }

        return baseStruct;
    }
    
}