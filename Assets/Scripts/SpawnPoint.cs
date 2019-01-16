using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	public Transform PointTransform { get; private set; }
	private float _distanceToClosestEnemy;
	private float _distanceToClosestFriend;
    public bool isAvailable;    //Spawn noktası seçiminden sonra 2 saniye boyunca kullanılamaz olduğunu gösterecek flag tanımlandı.

	public float SpawnTimer { get; private set; }

	public float DistanceToClosestEnemy
	{
		get
		{
			return _distanceToClosestEnemy;
		}

		set
		{
			_distanceToClosestEnemy = value;
		}
	}

	public float DistanceToClosestFriend
	{
		get
		{
			return _distanceToClosestFriend;
		}

		set
		{
			_distanceToClosestFriend = value;
		}
	}

	void Awake()
	{
        isAvailable = true;  //İlk açılışta spawn noktasını seçilebilir kılmak için isAvailable flag'i true yapıldı.

		PointTransform = transform;
#if UNITY_EDITOR
        if (transform.rotation.eulerAngles.x != 0 || transform.rotation.eulerAngles.z != 0)
        {
            Debug.LogError("This spawn point has some rotation issues : " + name + " rotation : " + transform.rotation.eulerAngles);
        }
#endif
    }
    public void StartTimer()
	{
		SpawnTimer = 2f;
	}

	private void Update()
	{
		if (SpawnTimer > 0)
		{
            isAvailable = false; //Spawn noktasının, timer aktif durumdayken kullanılabilir olmaması için bu flag false yapıldı.
			SpawnTimer -= Time.deltaTime;
		}
        else
        {
            isAvailable = true;  //Spawn noktasının, timer pasif durumdayken kullanılabilir olması için bu flag true yapıldı.
        }
	}
}

