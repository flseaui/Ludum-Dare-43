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
    [SerializeField] private TextMeshProUGUI _speedCounter;
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

    public int OxygenProduction;

    private bool _canScheduleShipmentNextDay = true;

    public int NextAttack = 5;
    
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

    public List<CrewStats> CaptainsStats, GunnersStats, MedicsStats, JanitorsStats, ScientistsStats;
    
    public int CrewCount => _captains + _gunners + _medics + _janitors + _scientists;

    private int speed;
    
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
        CaptainsStats = new List<CrewStats>();
        GunnersStats = new List<CrewStats>();
        MedicsStats = new List<CrewStats>();
        JanitorsStats = new List<CrewStats>();
        ScientistsStats = new List<CrewStats>();
        
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
        
        _speedCounter.text = $"Speed: {speed}ppd";
   }

private void DayTick()
{
    bool deathHappened = false;
    if (InShop) return;

    ++Day;

    if (Oxygen < 8)
        Oxygen += OxygenProduction;

    Oxygen -= Holes;

    if (Oxygen <= 0)
    {
        EventManager.Instance.DeathScreen();
    }

    if (CaptainsStats.Count > 0)
    {
        
        if (CaptainsStats[0].Piloting >= 20)
            speed = 4;
        else if (CaptainsStats[0].Piloting >= 15)
            speed = 3;
        else if (CaptainsStats[0].Piloting >= 10)
            speed = 2;
        else if (CaptainsStats[0].Piloting >= 5)
            speed = 1;
        else
            speed = 0;
    }

    ShipmentProgress += speed;
    
    if (Day % 12 == 0)
    {
        ScheduleGameEvent(GameEvent.Shop);
    }

        if (Day == 1) ScheduleGameEvent(GameEvent.Shipment);

        if (ShipmentProgress >= TargetShipment.Length && FirstShipmentSelected && _canScheduleShipmentNextDay)
        {

            ScheduleGameEvent(GameEvent.ShipmentComplete);
            _canScheduleShipmentNextDay = false;
        }

        
        if (Day == 4 || Day == NextAttack)
        {
            NextAttack = Day + UnityEngine.Random.Range(3, 6);
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

        var o2Produce = 0;
        if (ScientistsStats.Count > 0)
        {
            if (ScientistsStats[0].Intelligence > 5)
                o2Produce = 1;
            if (ScientistsStats[0].Intelligence >= 10)
                o2Produce = 2;
            if (ScientistsStats[0].Intelligence >= 17)
                o2Produce = 3;
            if (ScientistsStats[0].Intelligence >= 20)
                o2Produce = 4;
        }

        OxygenProduction = o2Produce;

        ShipManager.Instance.SetTileStats("oxygen_tank", new ShipManager.OxygenTileStats
        {
            Oxygen = o2Produce,
            Status = _scientists > 0
        });

        _canScheduleShipmentNextDay = true;
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
                CaptainsStats.Remove(stats);
                break;
            case CrewStats.MemberRole.Gunner:
                --_gunners;
                GunnersStats.Remove(stats);
                break;
            case CrewStats.MemberRole.Medic:
                --_medics;
                MedicsStats.Remove(stats);
                break;
            case CrewStats.MemberRole.Janitor:
                --_janitors;
                JanitorsStats.Remove(stats);
                break;
            case CrewStats.MemberRole.Scientist:
                --_scientists;
                ScientistsStats.Remove(stats);
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
                CaptainsStats.Add(crewMember.GetComponent<CrewStats>());
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
                GunnersStats.Add(crewMember.GetComponent<CrewStats>());
                break;
            case CrewStats.MemberRole.Medic:
                crewMember.GetComponent<CrewMovement>().GoToPosition(crewMember.transform.position, _crewSpawnPositions[5].position);
                _crewSpawnPositions[5].GetComponent<PositionStatus>().Occupied = true;
                crewMember.GetComponent<CrewStats>().ShipPosition = 5;
                ++_medics;
                MedicsStats.Add(crewMember.GetComponent<CrewStats>());
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
                JanitorsStats.Add(crewMember.GetComponent<CrewStats>());
                break;
            case CrewStats.MemberRole.Scientist:
                crewMember.GetComponent<CrewMovement>().GoToPosition(crewMember.transform.position, _crewSpawnPositions[8].position);
                _crewSpawnPositions[8].GetComponent<PositionStatus>().Occupied = true;
                crewMember.GetComponent<CrewStats>().ShipPosition = 8;
                ++_scientists;
                ScientistsStats.Add(crewMember.GetComponent<CrewStats>());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
