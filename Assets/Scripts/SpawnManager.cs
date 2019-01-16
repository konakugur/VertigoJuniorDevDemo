using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum PlayerTeam
{
    None,
    BlueTeam,

    RedTeam
}

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> _sharedSpawnPoints = new List<SpawnPoint>();
    System.Random _random = new System.Random();
	float _closestDistance;
    [Tooltip("This will be used to calculate the second filter where algorithm looks for closest friends, if the friends are away from this value, they will be ignored")]
    [SerializeField] private float _maxDistanceToClosestFriend = 30;
    [Tooltip("This will be used to calculate the first filter where algorithm looks for enemies that are far away from this value. Only enemies which are away from this value will be calculated.")]
    [SerializeField] private float _minDistanceToClosestEnemy = 10;
    [Tooltip("This value is to prevent friendly player spawning on top of eachothers. If a player is within the range of this value to a spawn point, that spawn point will be ignored")]
    [SerializeField] private float _minMemberDistance = 2;


    public DummyPlayer PlayerToBeSpawned;
    public DummyPlayer[] DummyPlayers;

    private void Awake()
    {
		_sharedSpawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());

		DummyPlayers = FindObjectsOfType<DummyPlayer>();
    }

    #region SPAWN ALGORITHM
    public SpawnPoint GetSharedSpawnPoint(PlayerTeam team)
    {
        List<SpawnPoint> spawnPoints = new List<SpawnPoint>(_sharedSpawnPoints.Count);
        CalculateDistancesForSpawnPoints(team);
        GetSpawnPointsByDistanceSpawning(team, ref spawnPoints);
        if (spawnPoints.Count <= 0)
        {
            GetSpawnPointsBySquadSpawning(team, ref spawnPoints);
        }
        SpawnPoint spawnPoint = spawnPoints[0];
        spawnPoint.StartTimer();
        return spawnPoint;
    }

    private void GetSpawnPointsByDistanceSpawning(PlayerTeam team, ref List<SpawnPoint> suitableSpawnPoints)
    {
        for(int i=0; i<_sharedSpawnPoints.Count;i++)
        {
            suitableSpawnPoints.Add(_sharedSpawnPoints[i]);
        }
        float maxDistance;
        maxDistance = suitableSpawnPoints[0].DistanceToClosestEnemy;
        var tempSpawnPoint = suitableSpawnPoints[0];
        print("compare with other");
        for (int i=0; i<suitableSpawnPoints.Count;i++)
        {
            print(suitableSpawnPoints[i].DistanceToClosestEnemy);
            print(_sharedSpawnPoints[i].DistanceToClosestEnemy);
            if( (suitableSpawnPoints[i].DistanceToClosestEnemy > maxDistance) && (suitableSpawnPoints[i].DistanceToClosestEnemy > _minMemberDistance) && (suitableSpawnPoints[i].DistanceToClosestFriend > _minMemberDistance) && (suitableSpawnPoints[i].isAvailable) )
            {
                //print("swapping");
                tempSpawnPoint = suitableSpawnPoints[0];
                suitableSpawnPoints[0] = suitableSpawnPoints[i];
                suitableSpawnPoints[i] = tempSpawnPoint;
            }
        }
    }

    private void GetSpawnPointsBySquadSpawning(PlayerTeam team, ref List<SpawnPoint> suitableSpawnPoints)
    {
        if (suitableSpawnPoints == null)
        {
            suitableSpawnPoints = new List<SpawnPoint>();
        }
        suitableSpawnPoints.Clear();
        _sharedSpawnPoints.Sort(delegate (SpawnPoint a, SpawnPoint b)
        {
            if (a.DistanceToClosestFriend == b.DistanceToClosestFriend)
            {
                return 0;
            }
            if (a.DistanceToClosestFriend > b.DistanceToClosestFriend)
            {
                return 1;
            }
            return -1;
        });
        for (int i = 0; i < _sharedSpawnPoints.Count && _sharedSpawnPoints[i].DistanceToClosestFriend <= _maxDistanceToClosestFriend; i++)
        {
            if (!(_sharedSpawnPoints[i].DistanceToClosestFriend <= _minMemberDistance) && !(_sharedSpawnPoints[i].DistanceToClosestEnemy <= _minMemberDistance) && _sharedSpawnPoints[i].SpawnTimer <= 0)
            {
                suitableSpawnPoints.Add(_sharedSpawnPoints[i]);
            }
        }
        if (suitableSpawnPoints.Count <= 0)
        {
            suitableSpawnPoints.Add(_sharedSpawnPoints[0]);
        }

    }

    private void CalculateDistancesForSpawnPoints(PlayerTeam playerTeam)
    {
        for (int i = 0; i < _sharedSpawnPoints.Count; i++)
        {
            _sharedSpawnPoints[i].DistanceToClosestFriend = GetDistanceToClosestMember(_sharedSpawnPoints[i].PointTransform.position, playerTeam);
            _sharedSpawnPoints[i].DistanceToClosestEnemy = GetDistanceToClosestMember(_sharedSpawnPoints[i].PointTransform.position, playerTeam == PlayerTeam.BlueTeam ? PlayerTeam.RedTeam : playerTeam == PlayerTeam.RedTeam ? PlayerTeam.BlueTeam : PlayerTeam.None);
        }
    }

    private float GetDistanceToClosestMember(Vector3 position, PlayerTeam playerTeam)
    {
        _closestDistance = float.MaxValue;   //Burası bulmam gereken hata diye düşünüyorum. Bu _closestDistance maximum float value olarak initialize edilmeli.
        foreach (var player in DummyPlayers)
        {
            if (!player.Disabled && player.PlayerTeamValue != PlayerTeam.None && player.PlayerTeamValue == playerTeam && !player.IsDead())
            {
                float playerDistanceToSpawnPoint = Vector3.Distance(position, player.Transform.position);
                //print(playerDistanceToSpawnPoint);
                if (playerDistanceToSpawnPoint < _closestDistance)
                {
                    _closestDistance = playerDistanceToSpawnPoint;
                }
            }
        }
        //print(_closestDistance);
        return _closestDistance;
    }

    #endregion
	/// <summary>
	/// Test için paylaşımlı spawn noktalarından en uygun olanını seçer.
	/// Test oyuncusunun pozisyonunu seçilen spawn noktasına atar.
	/// </summary>
    public void TestGetSpawnPoint()
    {
    	SpawnPoint spawnPoint = GetSharedSpawnPoint(PlayerToBeSpawned.PlayerTeamValue);
    	PlayerToBeSpawned.Transform.position = spawnPoint.PointTransform.position;
    }

}