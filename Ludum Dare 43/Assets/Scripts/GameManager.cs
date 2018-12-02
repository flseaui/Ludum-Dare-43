using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class GameManager : Singleton<GameManager>
{

    public int Day;
    public int Oxygen = 8;
    public int Money;
    
    [SerializeField] private GameObject _crewMemberPrefab;

    [SerializeField] private TextMeshProUGUI _dayCounter;
    [SerializeField] private TextMeshProUGUI _oxygenCounter;
    [SerializeField] private TextMeshProUGUI _moneyCounter;
    [SerializeField] private Slider _progressBar;

    public int Holes = 0;
    
    private readonly int StartingCrewMembers = 4;

    public readonly int MaxCrewMembers = 9;

    public bool InShop;

    public struct Shipment
    {
        // 0 - easy, 1 - intermediate, 2 - difficult
        public int Difficulty;
        public int Length;
        public int Price;
    }

    public Shipment TargetShipment;

    public int ShipmentProgress = 0;

    public int NumShipEncounters;

    public bool FirstShipmentSelected;

    private enum GameEvent
    {
        Ship,
        Shop,
        Shipment,
        ShipmentComplete
    }

    private Queue<GameEvent> _scheduledEvents;

    /*
    0 _captainPos;
    1 _gunnerPos0;
    2 _gunnerPos1;
    3 _gunnerPos2;
    4 _gunnerPos3;
    5 _medicPos;
    6 _janitorPosStart;
    7 _janitorPosEnd;
    8 scientist
    */
    [SerializeField] private Transform[] _crewSpawnPositions;

    // TODO: Add carrot boss fight
    
    [SerializeField] private int _captains = 0;
    [SerializeField] private int _gunners = 0;
    [SerializeField] private int _medics = 0;
    [SerializeField] private int _janitors = 0;
    [SerializeField] private int _scientists = 0;

    public int CrewCount => _captains + _gunners + _medics + _janitors + _scientists;

    public void SetTargetShipment(Shipment shipment)
    {
        TargetShipment = shipment;
        _progressBar.maxValue = shipment.Length;
    }
    
    private void ScheduleGameEvent(GameEvent gameEvent)
    {
        _scheduledEvents.Enqueue(gameEvent);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _scheduledEvents = new Queue<GameEvent>();
        for (int i = 0; i < StartingCrewMembers; i++)
        {
            SpawnCrewMember();
        }
        InvokeRepeating(nameof(DayTick), 1, 7);
    }
    
    // Update is called once per frame
    void Update()
    {
        _moneyCounter.text = Money.ToString() + '$';
        _progressBar.value = ShipmentProgress;
    }

    private void DayTick()
    {
        bool deathHappened = false;
        if (InShop) return;
        
        ++Day;
        
        if (Oxygen < 8)
            ++Oxygen;

        Oxygen -= Holes;

        if (Oxygen <= 0)
        {
            EventManager.Instance.DeathScreen();
        }
        
        ++ShipmentProgress;

        if (Day % 10 == 0)
        {
            ScheduleGameEvent(GameEvent.Shop);
        }
        
        if (Day == 1) ScheduleGameEvent(GameEvent.Shipment);

        if (ShipmentProgress >= TargetShipment.Length && FirstShipmentSelected)
        {
            ScheduleGameEvent(GameEvent.ShipmentComplete);
        }

        if (Day % 4 == 0)
        {
            ScheduleGameEvent(GameEvent.Ship);
        }
        
        
        _dayCounter.text = $"Day: {Day}";
        var o2Bars = "";
        for (var i = 0; i < Oxygen; i++)
        {
            o2Bars += '|';
        }

        _oxygenCounter.text = $"O2: {o2Bars}";
        
        if (_scheduledEvents.Count > 0)
        {
            var gameEvent = _scheduledEvents.Dequeue();
            switch (gameEvent)
            {
                case GameEvent.Ship:
                    ++NumShipEncounters;
                    EventManager.Instance.ShipEncounter();
                    break;
                case GameEvent.Shop:
                    EventManager.Instance.ShopEncounter();
                    break;
                case GameEvent.Shipment:
                    EventManager.Instance.ShipmentEncounter();
                    break;
                case GameEvent.ShipmentComplete:
                    Money += TargetShipment.Price;
                    EventManager.Instance.ShipmentCompleted();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    public struct CrewStatsStruct
    {
        public CrewStats.MemberRole Role;
    
        public int Piloting;
        public int Weight;
        public int Strength;
        public int Intelligence;
    }

    public void SpawnCrewMember(CrewStatsStruct stats)
    {
        var crewMember = Instantiate(_crewMemberPrefab);
        
        crewMember.GetComponent<CrewStats>().RandomizeStats();

        if (GetBlacklistedRoles().Contains(stats.Role))
        {
            crewMember.GetComponent<CrewStats>().RandomizeRole(GetBlacklistedRoles().ToArray());
            MoveCrewMember(crewMember);
        }
        else
            crewMember.GetComponent<CrewStats>().SwitchRoles(stats.Role, true);
    }
    
    public void SpawnCrewMember()
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
        if (_janitors > 1) blacklist.Add(CrewStats.MemberRole.Janitor);
        if (_scientists > 0) blacklist.Add(CrewStats.MemberRole.Scientist);

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
            case CrewStats.MemberRole.Scientist:
                --_scientists;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _crewSpawnPositions[stats.ShipPosition].GetComponent<PositionStatus>().Occupied = false;
    }
    
    public void MoveCrewMember(GameObject crewMember)
    {
        var random = new Random();
        switch (crewMember.GetComponent<CrewStats>().Role)
        {
            case CrewStats.MemberRole.Captain:
                crewMember.GetComponent<CrewMovement>().GoToPosition(crewMember.transform.position, _crewSpawnPositions[0].position);
                _crewSpawnPositions[0].GetComponent<PositionStatus>().Occupied = true;
                crewMember.GetComponent<CrewStats>().ShipPosition = 0;
                ++_captains;
                break;
            case CrewStats.MemberRole.Gunner:
                var unoccupiedGunner = new List<int>();
                for (var i = 1; i < 5; ++i)
                {
                    if (!_crewSpawnPositions[i].GetComponent<PositionStatus>().Occupied)
                    {
                        unoccupiedGunner.Add(i);
                    }
                }

                var gunnerSpot = random.Next(unoccupiedGunner.Count);
                crewMember.GetComponent<CrewMovement>().GoToPosition(crewMember.transform.position, _crewSpawnPositions[unoccupiedGunner[gunnerSpot]].position);
                _crewSpawnPositions[unoccupiedGunner[gunnerSpot]].GetComponent<PositionStatus>().Occupied = true;
                crewMember.GetComponent<CrewStats>().ShipPosition = unoccupiedGunner[gunnerSpot];
                ++_gunners;
                break;
            case CrewStats.MemberRole.Medic:
                crewMember.GetComponent<CrewMovement>().GoToPosition(crewMember.transform.position, _crewSpawnPositions[5].position);
                _crewSpawnPositions[5].GetComponent<PositionStatus>().Occupied = true;
                crewMember.GetComponent<CrewStats>().ShipPosition = 5;
                ++_medics;
                break;
            case CrewStats.MemberRole.Janitor:
                var unoccupiedJanitor = new List<int>();
                for (var i = 6; i < 8; ++i)
                {
                    if (!_crewSpawnPositions[i].GetComponent<PositionStatus>().Occupied)
                    {
                        unoccupiedJanitor.Add(i);
                    }
                }
               
                var janitorSpot = random.Next(unoccupiedJanitor.Count);
                crewMember.GetComponent<CrewMovement>().GoToPosition(crewMember.transform.position, _crewSpawnPositions[unoccupiedJanitor[janitorSpot]].position);
                _crewSpawnPositions[unoccupiedJanitor[janitorSpot]].GetComponent<PositionStatus>().Occupied = true;
                crewMember.GetComponent<CrewStats>().ShipPosition = unoccupiedJanitor[janitorSpot];
                ++_janitors;
                break;
            case CrewStats.MemberRole.Scientist:
                crewMember.GetComponent<CrewMovement>().GoToPosition(crewMember.transform.position, _crewSpawnPositions[8].position);
                _crewSpawnPositions[8].GetComponent<PositionStatus>().Occupied = true;
                crewMember.GetComponent<CrewStats>().ShipPosition = 8;
                ++_scientists;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
