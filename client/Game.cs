using UnityEngine;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
	public GameObject _renderSetupPrefab;
	public GameObject _playerPrefab;
	public GameObject _npcPrefab;
	
	public GameObject _bullet;
	public GameObject _flamethrowerBullet;
	public GameObject _rpgBullet;
	
	public GameObject _machinePrefab;
	public GameObject _flamePrefab;
	public GameObject _rpgPrefab;
	
	public GameObject _bluePixel;
	public GameObject _greenPixel;
	
	public GameObject[] _weaponSpawns;
	public GameObject[] _greenTeamSpawns;
	public GameObject[] _blueTeamSpawns;
	
	public GameObject _greenTeamBase;
	public GameObject _blueTeamBase;
	
	public float _keepAliveTime;
	
	public enum GameState
	{
		Connecting,
		Playing,
		Waiting
	}
	
	public enum ConnectionState
	{
		Connect,
		WaitForAnswer,
		Connected
	}
	
	public enum GameTeam : byte
	{
		Green = 0x00,
		Blue
	}
	
	private GameState _state = GameState.Connecting;
	private ConnectionState _connState = ConnectionState.Connect;
	
	private float _keepAliveTimer = 0f;
	private float _infoTimer = 0f;
	
	private GameObject _player;
	private List<GameObject> _npcs;
	private GameObject[] _weapons;
	private byte _id;
	
	private bool _connected = false;
	private byte _maxTime = 0; //0 Disabled
	private byte _maxPoints = 0; //0 Disabled
	private byte _greenPoints;
	private byte _bluePoints;
	private byte _totalPlayers;
	
	public GameObject _bluePixelInstance;
    public GameObject _greenPixelInstance;
    public GameObject _npcRedPixel;
    public GameObject _npcBluePixel;

	private Vector3 _blueBasePosition;
	private Vector3 _greenBasePosition;
	
	// Use this for initialization
	void Start ()
	{
		// Instantiate the viewport camera
		GameObject.Instantiate(_renderSetupPrefab);
		_npcs = new List<GameObject>();
		_weapons = new GameObject[_weaponSpawns.Length];
		
		_bluePixelInstance = (GameObject)GameObject.Instantiate(_bluePixel, _blueTeamBase.transform.position, Quaternion.identity);
		_greenPixelInstance = (GameObject)GameObject.Instantiate(_greenPixel, _greenTeamBase.transform.position, Quaternion.identity);
		
		_blueBasePosition = _blueTeamBase.transform.position;
		_greenBasePosition = _greenTeamBase.transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		switch (_state)
		{
			case GameState.Connecting:
				ConnectEventLoop();
				break;
				
			case GameState.Waiting:
				WaitEventLoop();
				break;
				
			case GameState.Playing:
				GameEventLoop();
				break;
		}
	}
	
	private void WaitEventLoop()
	{
	}
	
	private void GameEventLoop()
	{
		if (_connected)
		{
			_keepAliveTimer += Time.deltaTime;
			_infoTimer += Time.deltaTime;
			
			if (_keepAliveTimer > _keepAliveTime)
			{
				ForceSync();
			}
			
			if (_infoTimer > 1f)
			{
				NetworkController.instance.GetServerInfo();
				_infoTimer = 0f;
			}
			
			// Check if the player wants to exit
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				NetworkController.instance.Disconnect();
				Application.Quit();
			}
			
			// Poll the controls and update the player
			PollKeysDown();
			PollKeysUp();
			PollMouse();
			
			_blueTeamBase.transform.position = _blueBasePosition;
			_greenTeamBase.transform.position = _greenBasePosition;
			
			// Tab - only client side
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				_player.GetComponent<PlayerController>().OnTabDown();
			}
			
			if (Input.GetKeyUp(KeyCode.Tab))
			{
				_player.GetComponent<PlayerController>().OnTabUp();
			}
		}
	}
	
	public void ForceSync()
	{
		NetworkController.instance.SyncPosition(_id,
				_player.transform.position.x, _player.transform.position.y,
				_player.GetComponent<PlayerController>().GetTeam(), 
				_player.GetComponent<PlayerController>().GetRotation(),
				_player.GetComponent<PlayerController>().GetWeapon());
			
		_keepAliveTimer = 0f;
	}
	
	public void RemoveWeapon(byte location)
	{
		if (_weapons[location] != null)
		{
			GameObject.Destroy(_weapons[location]);
		}
	}
	
	private void PollMouse()
	{
		if (Input.GetMouseButtonDown(0))
		{
			_player.GetComponent<PlayerController>().Shoot();
			NetworkController.instance.KeyDownEvent(_id, (KeyCode)1);
		}
		
		if (Input.GetMouseButtonUp(0))
		{
			_player.GetComponent<PlayerController>().StopShoot();
			NetworkController.instance.KeyUpEvent(_id, (KeyCode)1);
		}
	}
	
	private void PollKeysDown()
	{
		if (_state == GameState.Playing)
		{
			if (Input.GetKeyDown(KeyCode.W))
			{
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.W);
				NetworkController.instance.KeyDownEvent(_id,KeyCode.W);
			}
			
			if (Input.GetKeyDown(KeyCode.S))
			{
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.S);
				NetworkController.instance.KeyDownEvent(_id,KeyCode.S);
			}
			
			if (Input.GetKeyDown(KeyCode.A))
			{
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.A);
				NetworkController.instance.KeyDownEvent(_id,KeyCode.A);
			}
			
			if (Input.GetKeyDown(KeyCode.D))
			{
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.D);
				NetworkController.instance.KeyDownEvent(_id,KeyCode.D);
			}
			
			if (Input.GetKeyDown(KeyCode.W) && Input.GetKeyDown(KeyCode.D))
			{
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.W);
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.D);
				NetworkController.instance.KeyDownEvent(_id,KeyCode.W);
				NetworkController.instance.KeyDownEvent(_id,KeyCode.D);
			}
			
			if (Input.GetKeyDown(KeyCode.W) && Input.GetKeyDown(KeyCode.A))
			{
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.W);
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.A);
				NetworkController.instance.KeyDownEvent(_id,KeyCode.W);
				NetworkController.instance.KeyDownEvent(_id,KeyCode.A);
			}
			
			if (Input.GetKeyDown(KeyCode.S) && Input.GetKeyDown(KeyCode.D))
			{
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.S);
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.D);
				NetworkController.instance.KeyDownEvent(_id,KeyCode.S);
				NetworkController.instance.KeyDownEvent(_id,KeyCode.D);
			}
			
			if (Input.GetKeyDown(KeyCode.S) && Input.GetKeyDown(KeyCode.A))
			{
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.S);
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.A);
				NetworkController.instance.KeyDownEvent(_id,KeyCode.S);
				NetworkController.instance.KeyDownEvent(_id,KeyCode.A);
			}
			
			if (Input.GetKeyDown(KeyCode.LeftShift))
			{
				_player.GetComponent<PlayerController>().OnKeyDown(KeyCode.LeftShift);
				NetworkController.instance.KeyDownEvent(_id, 0);
			}
		}
	}
	
	private void PollKeysUp()
	{
		if (_state == GameState.Playing)
		{
			if (Input.GetKeyUp(KeyCode.W))
			{
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.W);
				NetworkController.instance.KeyUpEvent(_id,KeyCode.W);
			}
			
			if (Input.GetKeyUp(KeyCode.S))
			{
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.S);
				NetworkController.instance.KeyUpEvent(_id,KeyCode.S);
			}
			
			if (Input.GetKeyUp(KeyCode.A))
			{
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.A);
				NetworkController.instance.KeyUpEvent(_id,KeyCode.A);
			}
			
			if (Input.GetKeyUp(KeyCode.D))
			{
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.D);
				NetworkController.instance.KeyUpEvent(_id,KeyCode.D);
			}
			
			if (Input.GetKeyUp(KeyCode.W) && Input.GetKeyUp(KeyCode.D))
			{
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.W);
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.D);
				NetworkController.instance.KeyUpEvent(_id,KeyCode.W);
				NetworkController.instance.KeyUpEvent(_id,KeyCode.D);
			}
			
			if (Input.GetKeyUp(KeyCode.W) && Input.GetKeyUp(KeyCode.A))
			{
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.W);
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.A);
				NetworkController.instance.KeyUpEvent(_id,KeyCode.W);
				NetworkController.instance.KeyUpEvent(_id,KeyCode.A);
			}
			
			if (Input.GetKeyUp(KeyCode.S) && Input.GetKeyUp(KeyCode.D))
			{
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.S);
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.D);
				NetworkController.instance.KeyUpEvent(_id,KeyCode.S);
				NetworkController.instance.KeyUpEvent(_id,KeyCode.D);
			}
			
			if (Input.GetKeyUp(KeyCode.S) && Input.GetKeyUp(KeyCode.A))
			{
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.S);
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.A);
				NetworkController.instance.KeyUpEvent(_id,KeyCode.S);
				NetworkController.instance.KeyUpEvent(_id,KeyCode.A);
			}
			
			if (Input.GetKeyUp(KeyCode.LeftShift))
			{
				_player.GetComponent<PlayerController>().OnKeyUp(KeyCode.LeftShift);
				NetworkController.instance.KeyUpEvent(_id, 0);
			}
		}
	}
	
	private void ConnectEventLoop()
	{
		if (NetworkController.instance != null)
		{
			switch (_connState)
			{
				case ConnectionState.Connect:
					NetworkController.instance.SetGame(this);
					
					NetworkController.instance.Connect();
					_connState = ConnectionState.WaitForAnswer;
					break;
					
				case ConnectionState.WaitForAnswer:
					break;
				
				case ConnectionState.Connected:
					_state = GameState.Waiting;
					break;
			}
		}
	}
	
	public void OnPlayerSpawn(byte id, int spawnLocation, GameTeam team, Vector3 spawnPosition)
	{
		GameObject go = null;
		Vector3 sp = Vector3.zero;
		
		if (team == GameTeam.Blue)
		{
			if (spawnLocation >= _blueTeamSpawns.Length)
			{
				sp = _blueTeamSpawns[_blueTeamSpawns.Length - 1].transform.position;
			}
			else if (spawnLocation != -1)
			{
				sp = _blueTeamSpawns[spawnLocation].transform.position;
			}
		}
		else
		{
			if (spawnLocation >= _greenTeamSpawns.Length)
			{
				sp = _greenTeamSpawns[_greenTeamSpawns.Length - 1].transform.position;
			}
			else if (spawnLocation != -1)
			{
				sp = _greenTeamSpawns[spawnLocation].transform.position;
			}
		}
		
		if (_player == null)
		{
			// Spawn our player
			_player = GameObject.Instantiate(_playerPrefab, spawnPosition, Quaternion.identity) as GameObject;
			_player = _player.transform.Find("Player").gameObject;
			
			_player.GetComponent<PlayerController>().SetGame(this);
			_player.GetComponent<PlayerController>().SetId(_id);
			_player.GetComponent<PlayerController>().SetTeam(team);
			_player.GetComponent<PlayerController>().SetPlayer(true);
			_player.GetComponent<PlayerController>().SetSpawnPoint(sp);
		}
		else
		{
			// Spawn another player
			go = GameObject.Instantiate(_npcPrefab, spawnPosition, Quaternion.identity) as GameObject;
			
			go.GetComponent<PlayerController>().SetGame(this);
			go.GetComponent<PlayerController>().SetId(id);
			go.GetComponent<PlayerController>().SetTeam(team);
			go.GetComponent<PlayerController>().SetPlayer(false);
			
			if (spawnLocation == -1)
			{
				go.transform.position = spawnPosition;
			}
			
			_npcs.Add(go);
		}
	}
	
	public void OnPlayerConnect(byte id)
	{
		if (_state == GameState.Connecting && _connState == ConnectionState.WaitForAnswer)
		{
			_id = id;
			_connected = true;
			_connState = ConnectionState.Connected;
		}
	}
	
	public void OnPlayerDisconnect(byte id)
	{
		if (_state == GameState.Playing)
		{
			for (int i = 0; i < _npcs.Count; i++)
			{
				if (_npcs[i].GetComponent<PlayerController>().GetId() == id)
				{
					GameObject go = _npcs[i];
					_npcs.RemoveAt(i);
					GameObject.Destroy(go);
				}
			}
		}
	}
	
	void OnApplicationQuit()
	{
		NetworkController.instance.Disconnect();
	}
	
	public void OnKeyDownEvent(byte id, KeyCode c)
	{
		if (_state == GameState.Playing)
		{
			for (int i = 0; i < _npcs.Count; i++)
			{
				if (_npcs[i].GetComponent<PlayerController>().GetId() == id)
				{
					if (c == 0)
					{
						_npcs[i].GetComponent<PlayerController>().OnKeyDown(KeyCode.LeftShift);
					}
					else if (c == (KeyCode)1)
					{
						_npcs[i].GetComponent<PlayerController>().Shoot();
					}
					else
					{
						_npcs[i].GetComponent<PlayerController>().OnKeyDown(c);
					}						
				}
			}
		}
	}
	
	public void OnKeyUpEvent(byte id, KeyCode c)
	{
		if (_state == GameState.Playing)
		{
			for (int i = 0; i < _npcs.Count; i++)
			{
				if (_npcs[i].GetComponent<PlayerController>().GetId() == id)
				{
					if (c == 0)
					{
						_npcs[i].GetComponent<PlayerController>().OnKeyUp(KeyCode.LeftShift);
					}
					else if (c == (KeyCode)1)
					{
						_npcs[i].GetComponent<PlayerController>().StopShoot();
					}
					else
					{
						_npcs[i].GetComponent<PlayerController>().OnKeyUp(c);
					}
				}
			}
		}
	}
	
	public void OnSyncPosition(byte id, float x, float y, Game.GameTeam team, byte rotation, PlayerController.WeaponType weapon)
	{
		if (_state == GameState.Playing)
		{
			Vector3 np = new Vector3(x,y,0f);
			bool found = false;
			
			for (int i = 0; i < _npcs.Count; i++)
			{
				if (_npcs[i].GetComponent<PlayerController>().GetId() == id)
				{
					_npcs[i].transform.position = np;
					_npcs[i].GetComponent<PlayerController>().SetRotation(rotation);
					
					if (_npcs[i].GetComponent<PlayerController>().GetWeapon() != weapon)
					{
						_npcs[i].GetComponent<PlayerController>().SetWeapon(weapon);
					}
					
					found = true;
				}
			}
			
			if (!found && id != _id)
			{
				OnPlayerSpawn(id, -1, team, np);
			}
		}
	}
	
	public void OnShootEvent(byte id, float x, float y, PlayerController.WeaponType wep)
	{
		if (_state == GameState.Playing)
		{
			Vector3 tgDir = new Vector3(x,y,0f);
			
			for (int i = 0; i < _npcs.Count; i++)
			{
				if (_npcs[i].GetComponent<PlayerController>().GetId() == id)
				{
					GameObject goPref = _bullet;
					
					if (wep == PlayerController.WeaponType.Flamethrower)
					{
						goPref = _flamethrowerBullet;
					}
					else if (wep == PlayerController.WeaponType.RPG)
					{
						goPref = _rpgBullet;
					}
					
					GameObject goBullet = GameObject.Instantiate(goPref);
					goBullet.transform.position = _npcs[i].transform.position + tgDir.normalized*0.5f;
					goBullet.GetComponent<BulletController>().SetWeaponType(wep);
					goBullet.GetComponent<BulletController>().SetDirection(tgDir.normalized);
					goBullet.GetComponent<BulletController>().SetTeam(_npcs[i].GetComponent<PlayerController>().GetTeam());
				}
			}
		}
	}
	
	public void OnHitEvent(byte id, PlayerController.WeaponType wep)
	{
		if (_state == GameState.Playing)
		{
			for (int i = 0; i < _npcs.Count; i++)
			{
				if (_npcs[i].GetComponent<PlayerController>().GetId() == id)
				{
					_npcs[i].GetComponent<PlayerController>().OnHit();
				}
			}
		}
	}
	
	public void OnDeathEvent(byte id)
	{
		if (_state == GameState.Playing)
		{
			for (int i = 0; i < _npcs.Count; i++)
			{
				if (_npcs[i].GetComponent<PlayerController>().GetId() == id)
				{
					_npcs[i].GetComponent<PlayerController>().OnDeath();
				}
			}
		}
	}
	
	public void OnWeaponSpawn(byte location, PlayerController.WeaponType type)
	{
		Vector3 sp;
		
		if (location >= _weaponSpawns.Length)
		{
			sp = _weaponSpawns[_weaponSpawns.Length - 1].transform.position;
		}
		else
		{
			sp = _weaponSpawns[location].transform.position;
		}
		
		switch (type)
		{
			case PlayerController.WeaponType.Machinegun:
				_weapons[location] = (GameObject)GameObject.Instantiate(_machinePrefab, sp, Quaternion.identity);
				_weapons[location].name = "Weapon_Machinegun_" + location.ToString();
				break;
				
			case PlayerController.WeaponType.Flamethrower:
				_weapons[location] = (GameObject)GameObject.Instantiate(_flamePrefab, sp, Quaternion.identity);
				_weapons[location].name = "Weapon_Flamethrower_" + location.ToString();
				break;
			
			case PlayerController.WeaponType.RPG:
				_weapons[location] = (GameObject)GameObject.Instantiate(_rpgPrefab, sp, Quaternion.identity);
				_weapons[location].name = "Weapon_RPG_" + location.ToString();
				break;
		}
	}
	
	public void OnWeaponPick(byte id, byte location, PlayerController.WeaponType type)
	{
		if (_state == GameState.Playing)
		{
			RemoveWeapon(location);
			
			for (int i = 0; i < _npcs.Count; i++)
			{
				if (_npcs[i].GetComponent<PlayerController>().GetId() == id)
				{
					_npcs[i].GetComponent<PlayerController>().SetWeapon(type);
				}
			}
		}
	}
	
	public void OnServerInfo(byte greenPoints, byte bluePoints, byte currentMinute, byte currentSecond, byte totPlayers)
	{
        int minute = currentMinute;
        int second = currentSecond;

        if(_maxTime > 0)
        {
            minute = _maxTime - currentMinute;
            if(currentSecond > 0)
            {
                minute--;
                second = 60 - currentSecond;
            }
        }

		if (_player) 
		{
			_player.GetComponent<PlayerController>().OnChangeTimer(minute, second);
			_player.GetComponent<PlayerController>()._uiScore.GetComponent<PointsController>().OnChangeScore(bluePoints, greenPoints);
		}
		
		_greenPoints = greenPoints;
		_bluePoints = bluePoints;
		_totalPlayers = totPlayers;
	}
	
	public void OnSpawnFlag(float x, float y, GameTeam team)
	{
		if (team == GameTeam.Blue)
		{
			_bluePixelInstance.transform.position = new Vector3(x, y, 0f);
		}
		else
		{
			_greenPixelInstance.transform.position = new Vector3(x, y, 0f);
		}
	}
	
	public void OnGrabFlag(byte id, GameTeam team)
	{
		GameTeam plTeam = GameTeam.Blue;
		
		// Get the caller's team
		if (id == _id)
		{
			plTeam = _player.GetComponent<PlayerController>().GetTeam();
		}
		else
		{
			for (int i = 0; i < _npcs.Count; i++)
			{
				if (_npcs[i].GetComponent<PlayerController>().GetId() == id)
				{
					plTeam = _npcs[i].GetComponent<PlayerController>().GetTeam();
				}
			}
		}
		
		if (team == plTeam)
		{
			if (team == GameTeam.Blue)
			{
				_bluePixelInstance.transform.position = _blueBasePosition;
				_npcBluePixel = null;
			}
			else
			{
				_greenPixelInstance.transform.position = _greenBasePosition;
				_npcRedPixel = null;
			}
		}
		else
		{
			if (_id == id)
			{
				_player.GetComponent<PlayerController>().SetCarriesFlag(true);
			}
			else
			{
				for (int i = 0; i < _npcs.Count; i++)
				{
					if (_npcs[i].GetComponent<PlayerController>().GetId() == id)
					{
						_npcs[i].GetComponent<PlayerController>().SetCarriesFlag(true);
					}
				}
			}
			
			if (team == GameTeam.Blue)
			{
				_bluePixelInstance.transform.position = new Vector3(1000f,0f,0f);
			}
			else
			{
				_greenPixelInstance.transform.position = new Vector3(1000f,0f,0f);
			}
		}
	}
	
	public void OnDropFlag(float x, float y, byte id)
	{
		Vector3 dp = new Vector3(x,y,0f);
		GameTeam t = GameTeam.Blue;
		
		for (int i = 0; i < _npcs.Count; i++)
		{
			if (_npcs[i].GetComponent<PlayerController>().GetId() == id)
			{
				t = _npcs[i].GetComponent<PlayerController>().GetTeam();
				_npcs[i].GetComponent<PlayerController>().SetCarriesFlag(false);
				
			}
		}
		
		if (t == GameTeam.Blue)
		{
			_bluePixelInstance.transform.position = dp;
			_npcBluePixel = null;
		}
		else
		{
			_greenPixelInstance.transform.position = dp;
			_npcRedPixel = null;
		}
	}
	
	public void OnShipFlag(byte id)
	{
		GameTeam t = GameTeam.Blue;
		
		if (id == _id)
		{
			t = _player.GetComponent<PlayerController>().GetTeam();
		}
		else
		{
			for (int i = 0; i < _npcs.Count; i++)
			{
				if (_npcs[i].GetComponent<PlayerController>().GetId() == id)
				{
					t = _npcs[i].GetComponent<PlayerController>().GetTeam();
					_npcs[i].GetComponent<PlayerController>().SetCarriesFlag(false);
				}
			}
		}
		
		if (t == GameTeam.Green)
		{
			_greenPixelInstance.transform.position = _greenTeamBase.transform.position;
		}
		else
		{
			_bluePixelInstance.transform.position = _blueTeamBase.transform.position;
		}
		
		if (_player) _player.GetComponent<PlayerController>().OnScore();
	}
	
	public void OnRoundFinish(byte finishType, byte greenPoints, byte bluePoints)
	{
		if (_state == GameState.Playing)
		{
			if (_player)
			{
				_player.GetComponent<PlayerController>().OnRoundFinished();
				_player.GetComponent<PlayerController>().OnReset();
			}
			
			_bluePixelInstance.transform.position = _blueBasePosition;
			_greenPixelInstance.transform.position = _greenBasePosition;
			
			_state = GameState.Waiting;
		}
	}
	
	public void OnRoundStart()
	{
		if (_state == GameState.Waiting)
		{
			if (_player)
			{
				_player.GetComponent<PlayerController>().OnRoundStarted();
			}
			
			_state = GameState.Playing;
		}
	}
	
	public void OnServerConfig(byte maxTime, byte maxPoints)
	{
		_maxPoints = maxPoints;
		_maxTime = maxTime;
	}
}
