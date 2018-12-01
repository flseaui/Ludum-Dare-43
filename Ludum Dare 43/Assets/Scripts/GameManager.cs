using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    public int Day;
    public int Oxygen = 8;
    public int Money;
    
    [SerializeField] private GameObject _crewMemberPrefab;

    [SerializeField] private TextMeshProUGUI _dayCounter;
    [SerializeField] private TextMeshProUGUI _oxygenCounter;
    [SerializeField] private TextMeshProUGUI _moneyCounter;

    private const int StartingCrewMembers = 5;
    
    /*
    0 _captainPos;
    1 _gunnerPos0;
    2 _gunnerPos1;
    3 _gunnerPos2;
    4 _gunnerPos3;
    5 _medicPos;
    6 _janitorPosStart;
    7 _janitorPosEnd;
    */
    [SerializeField] private Transform[] _crewSpawnPositions;

    private int _captains = 0;
    private int _gunners = 0;
    private int _medics = 0;
    private int _janitors = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < StartingCrewMembers; i++)
        {
            SpawnCrewMember();
        }
        InvokeRepeating(nameof(DayTick), 1, 10);
    }
    
    // Update is called once per frame
    void Update()
    {

    }

    private void DayTick()
    {
        ++Day;
        
        if (Oxygen < 8)
            ++Oxygen;
        
        if (Day == 2) EventManager.Instance.ShipEncounter();
        
        _dayCounter.text = $"Day: {Day}";
        var o2Bars = "";
        for (var i = 0; i < Oxygen; i++)
        {
            o2Bars += '|';
        }

        _oxygenCounter.text = $"O2: {o2Bars}";
        
    }
    
    void SpawnCrewMember()
    {
        var crewMember = Instantiate(_crewMemberPrefab);
        
        crewMember.GetComponent<CrewStats>().RandomizeStats();

        crewMember.GetComponent<CrewStats>().RandomizeRole(GetBlacklistedRoles().ToArray());
        
        MoveCrewMember(crewMember);
    }

    public List<CrewStats.MemberRole> GetBlacklistedRoles()
    {
        var blacklist = new List<CrewStats.MemberRole>();
        
        if (_gunners > 3) blacklist.Add(CrewStats.MemberRole.Gunner);
        if (_captains > 0) blacklist.Add(CrewStats.MemberRole.Captain);
        if (_medics > 0) blacklist.Add(CrewStats.MemberRole.Medic);
        if (_janitors > 0) blacklist.Add(CrewStats.MemberRole.Janitor);

        return blacklist;
    }

    public void CalcCrewMemberCounts(CrewStats stats)
    {
        switch (stats.Role)
        {
            case CrewStats.MemberRole.Captain:
                --_captains;
                break;
            case CrewStats.MemberRole.Gunner:
                --_gunners;
                break;
            case CrewStats.MemberRole.Medic:
                --_medics;
                break;
            case CrewStats.MemberRole.Janitor:
                --_janitors;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _crewSpawnPositions[stats.ShipPosition].GetComponent<PositionStatus>().Occupied = false;
    }
    
    public void MoveCrewMember(GameObject crewMember)
    {
        switch (crewMember.GetComponent<CrewStats>().Role)
        {
            case CrewStats.MemberRole.Captain:
                crewMember.GetComponent<CrewMovement>().GoToPosition(crewMember.transform.position, _crewSpawnPositions[0].position);
                _crewSpawnPositions[0].GetComponent<PositionStatus>().Occupied = true;
                crewMember.GetComponent<CrewStats>().ShipPosition = 0;
                ++_captains;
                break;
            case CrewStats.MemberRole.Gunner:
                for (var i = 1; i < 5; ++i)
                {
                    if (!_crewSpawnPositions[i].GetComponent<PositionStatus>().Occupied)
                    {
                        crewMember.GetComponent<CrewMovement>().GoToPosition(crewMember.transform.position, _crewSpawnPositions[i].position);
                        _crewSpawnPositions[i].GetComponent<PositionStatus>().Occupied = true;
                        crewMember.GetComponent<CrewStats>().ShipPosition = i;
                        ++_gunners;
                        break;
                    }
                }
               
                break;
            case CrewStats.MemberRole.Medic:
                crewMember.GetComponent<CrewMovement>().GoToPosition(crewMember.transform.position, _crewSpawnPositions[5].position);
                _crewSpawnPositions[5].GetComponent<PositionStatus>().Occupied = true;
                crewMember.GetComponent<CrewStats>().ShipPosition = 5;
                ++_medics;
                break;
            case CrewStats.MemberRole.Janitor:
                crewMember.GetComponent<CrewMovement>().GoToPosition(crewMember.transform.position, _crewSpawnPositions[6].position);
                _crewSpawnPositions[6].GetComponent<PositionStatus>().Occupied = true;
                crewMember.GetComponent<CrewStats>().ShipPosition = 6;
                ++_janitors;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
