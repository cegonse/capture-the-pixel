using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
	public GameObject _mainCamera;
	
	public GameObject _bullet;
	public GameObject _flamethrowerBullet;
	public GameObject _rpgBullet;
	
	public GameObject _uiStaminaBar;
	public GameObject _uiHealthBar;
	public GameObject _deathTimer;
	public GameObject _uiTimer;
	public GameObject _uiScore;
	
	public GameObject _explosionPrefab;
    public GameObject[] _deathPrefabs;
	
	public GameObject _audioSource1;
	public GameObject _audioSource2;
	public GameObject _audioSource3;
	
	public float _speed;
	public float _flagSpeed;
	public float _sprintMultiplier;
	
	public float _pistolShootCadence = 0.6f;
	public float _machineShootCadence = 0.1f;
	public float _flameShootCadence = 0.05f;
	public float _rpgShootCadence = 2f;
	
	public float _pistolShootDamage= 0.3f;
	public float _machineShootDamage = 0.1f;
	public float _flameShootDamage = 0.05f;
	public float _rpgShootDamage = 10f;
	
	public int _pistolMaxAmmo = -1;
	public int _machineMaxAmmo = 20;
	public int _flameMaxAmmo = 60;
	public int _rpgMaxAmmo = 10;
	
	public float _staminaTime = 3f;
	public float _maxHealth = 3f;
	
	public Sprite _22Deg, _45Deg, _67Deg, _0Deg;
	public Sprite Timer5,Timer4,Timer3,Timer2,Timer1,TimerReady;
	
	public AudioClip _pistolAudio;
	public AudioClip[] _machineAudio;
	public AudioClip _flameAudio;
	public AudioClip _rpgAudio;
	public AudioClip _hitAudio;
	public AudioClip _boomAudio;
	public AudioClip[] _walkAudio;
	
	public enum WeaponType : byte
	{
		Pistol,
		Machinegun,
		Flamethrower,
		RPG
	}
	
	private enum MovementDirection : byte
	{
		Up = 0,
		Down,
		Left,
		Right,
		UpRight,
		UpLeft,
		DownRight,
		DownLeft,
		None
	}
	
	private enum State : byte
	{
		Playing,
		Respawning,
		Dead
	}
	
	private MovementDirection _direction = MovementDirection.Up;
	private State _state = State.Respawning;
	private bool _moving = false;
	private bool _running = false;
    private bool _shooting = false;
	private Vector3 _vectorDirection = Vector3.zero;
	private Vector3 _mousePosition;
	
	private float _shootAngle;
	private WeaponType _currentWeapon = WeaponType.Pistol;
	private int _currentAmmo = -1;
	private float _shootCadence = 0.1f;
	
	private float _shootTimer = 0f;
	private float _staminaTimer = 0f;
	private Game.GameTeam _team;
	private Game _game;
	private bool _isPlayer = true;
	private byte _id = 0;
	
	private List<KeyCode> _activeKeys;
	private bool _nextShoot = false;
	private bool _nextShield = false;
	private byte _rotationByte = 0x00;
	private byte _previousRotation = 0x00;

    //Pixel indicators
    public GameObject _redIndPrefab;
    public GameObject _blueIndPrefab;

    public GameObject _redInd;
    public GameObject _blueInd;

    //Animation
    private int _animationDirection = 0;
    private int _animationLock = 0;
    private int _animationOffset = 0;
	
	private bool _isDead = false;
	public float _health = 0f;
	private Vector3 _spawnPoint;
	private int _spawnTime = 5;
	private float _spawnTimer = 1f;
	private float _deathWait = 0f;
	private GameObject _deathAnimInstance;
	private Vector3 _uiHealthBarPos;
	private bool _carriesFlag = false;
	
	// Use this for initialization
	void Start ()
	{
		_mousePosition = Input.mousePosition;
		_staminaTimer = _staminaTime;
		_activeKeys = new List<KeyCode>();
		
		_health = _maxHealth;
		transform.position = new Vector3(1000f,0f,0f);
		
		if (_uiHealthBar != null) _uiHealthBarPos = _uiHealthBar.transform.localPosition;

        if (_isPlayer)
        {
            _redInd = GameObject.Instantiate(_redIndPrefab);
            _blueInd = GameObject.Instantiate(_blueIndPrefab);
        }
        

	}
	
	public void OnScore()
	{
		_uiScore.GetComponent<PointsController>().ShowScore(true,3);
	}
	
	public void OnTabDown()
	{
		_uiScore.GetComponent<PointsController>().ShowScore(true);
	}
	
	public void OnTabUp()
	{
		_uiScore.GetComponent<PointsController>().ShowScore(false);
	}
	
	public void OnRoundStarted()
	{
		_uiScore.GetComponent<SpriteRenderer>().enabled = false;
	}
	
	public void SetCarriesFlag(bool f)
	{
		_carriesFlag = f;
	}
	
	public WeaponType GetWeapon()
	{
		return _currentWeapon;
	}
	
	public void SetWeapon(WeaponType wep)
	{
		_currentWeapon = wep;
		
		switch (wep)
		{
			case WeaponType.Flamethrower:
				_currentAmmo = _flameMaxAmmo;
				break;
			
			case WeaponType.Machinegun:
				_currentAmmo = _machineMaxAmmo;
				break;
			
			case WeaponType.RPG:
				_currentAmmo = _rpgMaxAmmo;
				break;
		}
	}
	
	public void SetPlayer(bool p)
	{
		_isPlayer = p;
		if (!p) _state = State.Playing;
	}
	
	public void SetTeam(Game.GameTeam team)
	{
		_team = team;
	}
	
	public Game.GameTeam GetTeam()
	{
		return _team;
	}
	
	public void SetGame(Game g)
	{
		_game = g;
	}
	
	public void SetId(byte id)
	{
		_id = id;
	}
	
	public byte GetId()
	{
		return _id;
	}
	
	public byte GetRotation()
	{
		return _rotationByte;
	}
	
	public void SetRotation(byte r)
	{
		_rotationByte = r;
	}
	
	public void Shoot()
	{
		_nextShoot = true;
	}
	
	public void StopShoot()
	{
		_nextShoot = false;
	}
	
	public void SetSpawnPoint(Vector3 sp)
	{
		_spawnPoint = sp;
	}
	
	public void OnKeyDown(KeyCode c)
	{
		if (_activeKeys.Count == 0)
		{
			_activeKeys.Add(c);
		}
		else
		{
			for (int i = 0; i < _activeKeys.Count; i++)
			{
				if (!_activeKeys.Contains(c))
				{
					_activeKeys.Add(c);
				}
			}
		}
	}
	
	public void OnKeyUp(KeyCode c)
	{
		for (int i = 0; i < _activeKeys.Count; i++)
		{
			if (_activeKeys.Contains(c))
			{
				_activeKeys.Remove(c);
			}
		}
	}
	
	public void OnChangeTimer(int minute, int second)
	{
		if (_uiTimer)
		{
			_uiTimer.GetComponent<TimerController>().OnChangeTimer(minute, second);
		}
	}
	
	public void OnRoundFinished()
	{
		_mainCamera.transform.position = Vector3.zero;
		_uiScore.GetComponent<SpriteRenderer>().enabled = true;
		_state = State.Respawning;
	}
	
	// Update is called once per frame
	void Update ()
	{
		_moving = true;
		_running = false;
		_direction = MovementDirection.None;
		
		if (_state == State.Respawning)
		{
			_spawnTimer -= Time.deltaTime;
			
			if (_spawnTimer > 0.66f)
			{
			}
			else if (_spawnTimer > 0f && _spawnTimer < 0.33f)
			{
			}
			else if (_spawnTimer > 0.33f && _spawnTimer < 0.66f)
			{
				if (_deathTimer) _deathTimer.transform.position = Vector3.zero;
			}
			else if (_spawnTimer < 0f)
			{
				_spawnTimer = 1f;
				_spawnTime--;
				
				if (_isPlayer)
				{
					_deathTimer.transform.position = Vector3.zero;
				}
				
				if (_spawnTime == 4) if (_isPlayer) _deathTimer.GetComponent<SpriteRenderer>().sprite = Timer4;
				if (_spawnTime == 3) if (_isPlayer) _deathTimer.GetComponent<SpriteRenderer>().sprite = Timer3;
				if (_spawnTime == 2) if (_isPlayer) _deathTimer.GetComponent<SpriteRenderer>().sprite = Timer2;
				if (_spawnTime == 1) if (_isPlayer) _deathTimer.GetComponent<SpriteRenderer>().sprite = Timer1;
				if (_spawnTime == 0) if (_isPlayer) _deathTimer.GetComponent<SpriteRenderer>().sprite = TimerReady;
				if (_spawnTime == -1)
				{
					_spawnTime = 5;
					
					if (_isPlayer)
					{
						_deathTimer.GetComponent<SpriteRenderer>().sprite = Timer5;
						_deathTimer.transform.position = new Vector3(4.5f,_deathTimer.transform.position.y,0f);
					}
					
					transform.position = _spawnPoint;
					_state = State.Playing;
				}
			}
		}
		else if (_state == State.Playing)
		{
			// Poll the controls and update the movement direction
			if (_activeKeys.Contains(KeyCode.W))
			{
				_direction = MovementDirection.Up;
			}
			
			if (_activeKeys.Contains(KeyCode.S))
			{
				_direction = MovementDirection.Down;
			}
			
			if (_activeKeys.Contains(KeyCode.A))
			{
				_direction = MovementDirection.Left;
			}
			
			if (_activeKeys.Contains(KeyCode.D))
			{
				_direction = MovementDirection.Right;
			}
			
			if (_activeKeys.Contains(KeyCode.W) && _activeKeys.Contains(KeyCode.D))
			{
				_direction = MovementDirection.UpRight;
			}
			
			if (_activeKeys.Contains(KeyCode.W) && _activeKeys.Contains(KeyCode.A))
			{
				_direction = MovementDirection.UpLeft;
			}
			
			if (_activeKeys.Contains(KeyCode.S) && _activeKeys.Contains(KeyCode.D))
			{
				_direction = MovementDirection.DownRight;
			}
			
			if (_activeKeys.Contains(KeyCode.S) && _activeKeys.Contains(KeyCode.A))
			{
				_direction = MovementDirection.DownLeft;
			}
			
			if (_direction == MovementDirection.None)
			{
				_moving = false;
			}
			else
			{
				_moving = true;
			}
			
			// Is the player running?
			if (_activeKeys.Contains(KeyCode.LeftShift))
			{
				if (_direction == MovementDirection.None)
				{
					if (_staminaTimer < _staminaTime)
					{
						_staminaTimer += Time.deltaTime*0.25f;
						if (_uiStaminaBar != null) _uiStaminaBar.transform.position += new Vector3(Time.deltaTime*0.25f,0f,0f);
					}
				}
				else
				{
					if (_staminaTimer > 0f)
					{
						_running = true;
						_staminaTimer -= Time.deltaTime;
						if (_uiStaminaBar != null) _uiStaminaBar.transform.position -= new Vector3(Time.deltaTime,0f,0f);
					}
					else
					{
						_running = false;
					}
				}
			}
			else
			{
				_running = false;
				
				if (_staminaTimer < _staminaTime)
				{
					_staminaTimer += Time.deltaTime*0.25f;
					if (_uiStaminaBar != null) _uiStaminaBar.transform.position += new Vector3(Time.deltaTime*0.25f,0f,0f);
				}
			}

			if (_moving)
			{
				switch (_direction)
				{
					case MovementDirection.Up:
						_vectorDirection.x = 0;
						_vectorDirection.y = 1;
						break;
					
					case MovementDirection.Down:
						_vectorDirection.x = 0;
						_vectorDirection.y = -1;
						break;
					
					case MovementDirection.Left:
						_vectorDirection.x = -1;
						_vectorDirection.y = 0;
						break;
						
					case MovementDirection.Right:
						_vectorDirection.x = 1;
						_vectorDirection.y = 0;
						break;
						
					case MovementDirection.UpRight:
						_vectorDirection.x = 0.5f;
						_vectorDirection.y = 0.5f;
						break;
					
					case MovementDirection.DownRight:
						_vectorDirection.x = 0.5f;
						_vectorDirection.y = -0.5f;
						break;
						
					case MovementDirection.UpLeft:
						_vectorDirection.x = -0.5f;
						_vectorDirection.y = 0.5f;
						break;
						
					case MovementDirection.DownLeft:
						_vectorDirection.x = -0.5f;
						_vectorDirection.y = -0.5f;
						break;
				}
				
				_vectorDirection = _vectorDirection.normalized;
				Vector3 tgDelta = _vectorDirection * (_carriesFlag ? _flagSpeed : _speed) * Time.deltaTime;
				
				if (_running)
				{
					tgDelta *= _sprintMultiplier;
				}
				
				if (!_audioSource1.GetComponent<AudioSource>().isPlaying)
				{
					_audioSource1.GetComponent<AudioSource>().clip = _walkAudio[Random.Range(0,_walkAudio.Length)];
					_audioSource1.GetComponent<AudioSource>().Play();
				}
				
				transform.position += tgDelta;
			}
			
			// Camera movements and rotations are only handled if
			// the character is player controlled. If it's not,
			// the server updates the rotations
			if (_isPlayer)
			{
				// Get the world space mouse coordinates
				_mousePosition = Input.mousePosition;

				//Get the offset of the real 64x64 screen on the screen device
				int mouse_x_offset = (Screen.width - Screen.height) / 2;
				_mousePosition.x -= mouse_x_offset;
				//Adjust the _mousePosition.x to fit only the space that fills the 64x64 screen on de real device
				_mousePosition.x = Mathf.Min(Mathf.Max(0, _mousePosition.x), Screen.height);
                //Adjust the _mousePosition.y too
                _mousePosition.y = Mathf.Min(Mathf.Max(0, _mousePosition.y), Screen.height);

				//Make the operations to normalize the _mousePosition vector
				_mousePosition.x /= Screen.height * 0.5f;
				_mousePosition.y /= Screen.height * 0.5f;
				_mousePosition.x -= 1f;
				_mousePosition.y -= 1f;
				_mousePosition.z = 0f;


				
				// Make the character look at the crosshair position
				Vector3 tgDir = _mousePosition;
				
				// Check if the player wants to shoot
				// First, set the cadence to the current weapon
				// Second, check if the player has ennough ammo
				bool canShoot = false;
				
				switch (_currentWeapon)
				{
					case WeaponType.Pistol:
						_shootCadence = _pistolShootCadence;
						break;
					
					case WeaponType.Machinegun:
						_shootCadence = _machineShootCadence;
						break;
					
					case WeaponType.Flamethrower:
						_shootCadence = _flameShootCadence;
						break;
					
					case WeaponType.RPG:
						_shootCadence = _rpgShootCadence;
						break;
				}
				
				if (_currentAmmo == -1)
				{
					canShoot = true;
				}
				else if (_currentAmmo > 0)
				{
					canShoot = true;
				}
				else
				{
					_currentWeapon = WeaponType.Pistol;
					_currentAmmo = -1;
				}
				
				// Finally, perform the shoot
				if (canShoot && _nextShoot && _shootTimer > _shootCadence)
				{
					GameObject goPref = _bullet;
					_currentAmmo--;
				    
					if (_currentWeapon == PlayerController.WeaponType.Flamethrower)
					{
						goPref = _flamethrowerBullet;
					}
					else if (_currentWeapon == PlayerController.WeaponType.RPG)
					{
						goPref = _rpgBullet;
					}
					
					GameObject goBullet = GameObject.Instantiate(goPref);
					goBullet.transform.position = transform.position + tgDir.normalized*0.5f;
					goBullet.GetComponent<BulletController>().SetWeaponType(_currentWeapon);
					goBullet.GetComponent<BulletController>().SetDirection(tgDir.normalized);
					goBullet.GetComponent<BulletController>().SetTeam(_team);
					
					// Send the bullet to the other players
					NetworkController.instance.ShootEvent(_id, tgDir.normalized, _currentWeapon);
					
					_shootTimer = 0f;
				}
				
				_shootTimer += Time.deltaTime;
				_shootAngle = Mathf.Atan2(tgDir.y, tgDir.x) * Mathf.Rad2Deg;
				
				if (_shootAngle > 78.5f && _shootAngle <= 101.25f)
				{
					_shootAngle = 90f;
					_animationDirection = 0;
					_rotationByte = 0x00;
				}
				else if (_shootAngle > 56.25f && _shootAngle <= 78.75f)
				{
					_shootAngle = 90f;
					_animationDirection = 22;
					_rotationByte = 0x01;
				}
				else if (_shootAngle > 33.75f && _shootAngle <= 56.25f)
				{
					_shootAngle = 90f;
					_animationDirection = 45;
					_rotationByte = 0x02;
				}
				else if (_shootAngle > 11.25f && _shootAngle <= 33.75f)
				{
					_shootAngle = 90f;
					_animationDirection = 67;
					_rotationByte = 0x03;
				}
				else if (_shootAngle > 101.25f && _shootAngle <= 123.75)
				{
					_shootAngle = 180f;
					_animationDirection = 67;
					_rotationByte = 0x04;
				}
				else if (_shootAngle > 123.75 && _shootAngle <= 145.25)
				{
					_shootAngle = 180f;
					_animationDirection = 45;
					_rotationByte = 0x05;
				}
				else if (_shootAngle > 145.25f && _shootAngle <= 167.75)
				{
					_shootAngle = 180f;
					_animationDirection = 22;
					_rotationByte = 0x06;
				}
				else if ((_shootAngle > 167.75f && _shootAngle <= 180f)
					|| (_shootAngle > -180f && _shootAngle <= -168.75f))
				{
					_shootAngle = 180f;
					_animationDirection = 0;
					_rotationByte = 0x07;
				}
				else if (_shootAngle > -168.75 && _shootAngle <= -146.25f)
				{
					_shootAngle = -90f;
					_animationDirection = 67;
					_rotationByte = 0x08;
				}
				else if (_shootAngle > -146.25 && _shootAngle <= -123.75f)
				{
					_shootAngle = -90f;
					_animationDirection = 45;
					_rotationByte = 0x09;
				}
				else if (_shootAngle > -123.75 && _shootAngle <= -101.25)
				{
					_shootAngle = -90f;
					_animationDirection = 22;
					_rotationByte = 0x0A;
				}
				else if (_shootAngle > -101.25f && _shootAngle <= -78.75f)
				{
					_shootAngle = -90f;
					_animationDirection = 0;
					_rotationByte = 0x0B;
				}
				else if (_shootAngle > -78.75f && _shootAngle <= -56.25f)
				{
					_shootAngle = 0f;
					_animationDirection = 67;
					_rotationByte = 0x0C;
				}
				else if (_shootAngle > -56.25f && _shootAngle <= -33.75f)
				{
					_shootAngle = 0f;
					_animationDirection = 45;
					_rotationByte = 0x0D;
				}
				else if (_shootAngle > -33.75f && _shootAngle <= -11.25f)
				{
					_shootAngle = 0f;
					_animationDirection = 22;
					_rotationByte = 0x0E;
				}
				else if ((_shootAngle < 45f && _shootAngle >= 0f)
					|| (_shootAngle >= -45f && _shootAngle < 0f)) 
				{
					_shootAngle = 0f;
					_animationDirection = 0;
					_rotationByte = 0x0F;
				}
				
				if (_previousRotation != _rotationByte)
				{
					_game.ForceSync();
				}
				
				_previousRotation = _rotationByte;
				
				transform.localRotation = Quaternion.AngleAxis(_shootAngle, Vector3.forward);
				
				Vector3 tgPos = transform.position + 1.3f * _mousePosition;
				tgPos.z = -1;
				
				_mainCamera.transform.position = tgPos;
			}
			else
			{
				switch (_rotationByte)
				{
					case 0x00:
						{
							_shootAngle = 90f;
							_animationDirection = 0;
						}
						break;
					case 0x01:
						{
							_shootAngle = 90f;
							_animationDirection = 22;
						}
						break;
					case 0x02:
						{
							_shootAngle = 90f;
							_animationDirection = 45;
						}
						break;
					case 0x03:
						{
							_shootAngle = 90f;
							_animationDirection = 67;
						}
						break;
					case 0x04:
						{
							_shootAngle = 180f;
							_animationDirection = 67;
						}
						break;
					case 0x05:
						{
							_shootAngle = 180f;
							_animationDirection = 45;
						}
						break;
					case 0x06:
						{
							_shootAngle = 180f;
							_animationDirection = 22;
						}
						break;
					case 0x07:
						{
							_shootAngle = 180f;
							_animationDirection = 0;
						}
						break;
					case 0x08:
						{
							_shootAngle = -90f;
							_animationDirection = 67;
						}
						break;
					case 0x09:
						{
							_shootAngle = -90f;
							_animationDirection = 45;
						}
						break;
					case 0x0A:
						{
							_shootAngle = -90f;
							_animationDirection = 22;
						}
						break;
					case 0x0B:
						{
							_shootAngle = -90f;
							_animationDirection = 0;
						}
						break;
					case 0x0C:
						{
							_shootAngle = 0f;
							_animationDirection = 67;
						}
						break;
					case 0x0D:
						{
							_shootAngle = 0f;
							_animationDirection = 45;
						}
						break;
					case 0x0E:
						{
							_shootAngle = 0f;
							_animationDirection = 22;
						}
						break;
					case 0x0F:
						{
							_shootAngle = 0f;
							_animationDirection = 0;
						}
						break;
				}
				
				transform.localRotation = Quaternion.AngleAxis(_shootAngle, Vector3.forward);
			}


            SpriteAnimator spriteAnimator = GetComponent<SpriteAnimator>();

            if (_team == Game.GameTeam.Blue)
            {
                _animationOffset = 24;
            }
            else
            {
                _animationOffset = 0;
            }

            switch (_currentWeapon)
            {
                case WeaponType.Pistol:
                    _animationOffset += 0;

                    spriteAnimator._animations.row[4].column[1].timeToNext = 0.5f;
                    spriteAnimator._animations.row[5].column[1].timeToNext = 0.5f;
                    spriteAnimator._animations.row[6].column[1].timeToNext = 0.5f;
                    spriteAnimator._animations.row[7].column[1].timeToNext = 0.5f;

                    break;

                case WeaponType.Machinegun:
                    _animationOffset += 0;
                    spriteAnimator._animations.row[4].column[1].timeToNext = 0.1f;
                    spriteAnimator._animations.row[5].column[1].timeToNext = 0.1f;
                    spriteAnimator._animations.row[6].column[1].timeToNext = 0.1f;
                    spriteAnimator._animations.row[7].column[1].timeToNext = 0.1f;

                    break;

                case WeaponType.Flamethrower:
                    _animationOffset += 8;
                    break;

                case WeaponType.RPG:
                    _animationOffset += 16;
                    break;
            }

			//Animation
			if (_nextShoot)
			{
				if (!_audioSource2.GetComponent<AudioSource>().isPlaying)
				{
					switch (_currentWeapon)
					{
						case WeaponType.Pistol:
							_audioSource2.GetComponent<AudioSource>().clip = _pistolAudio;
							break;
						
						case WeaponType.Machinegun:
							_audioSource2.GetComponent<AudioSource>().clip = _machineAudio[Random.Range(0,_machineAudio.Length)];
							break;
							
						case WeaponType.Flamethrower:
							_audioSource2.GetComponent<AudioSource>().clip = _flameAudio;
							break;
						
						case WeaponType.RPG:
							_audioSource2.GetComponent<AudioSource>().clip = _rpgAudio;
							break;
					}
					
					_audioSource2.GetComponent<AudioSource>().Play();
				}
				
				switch (_animationDirection)
				{
					case 0:
						if (_animationLock != 4)
						{
                            spriteAnimator.SetActiveAnimation(4 + _animationOffset);
							_animationLock = 4;
						}
						break;

					case 22:
						if (_animationLock != 5)
						{
                            spriteAnimator.SetActiveAnimation(5 + _animationOffset);
							_animationLock = 5;
						}
						break;

					case 45:
						if (_animationLock != 6)
						{
                            spriteAnimator.SetActiveAnimation(6 + _animationOffset);
							_animationLock = 6;
						}
						break;

					case 67:
						if (_animationLock != 7)
						{
                            spriteAnimator.SetActiveAnimation(7 + _animationOffset);
							_animationLock = 7;
						}
						break;
				}
			}
			else
			{
				switch (_animationDirection)
				{
					case 0:
						if (_animationLock != 0)
						{
                            spriteAnimator.SetActiveAnimation(0 + _animationOffset);
							_animationLock = 0;
						}                    
						break;

					case 22:
						if (_animationLock != 1)
						{
                            spriteAnimator.SetActiveAnimation(1 + _animationOffset);
							_animationLock = 1;
						}                    
						break;

					case 45:
						if (_animationLock != 2)
						{
                            spriteAnimator.SetActiveAnimation(2 + _animationOffset);
							_animationLock = 2;
						}
						break;

					case 67:
						if (_animationLock != 3)
						{
                            spriteAnimator.SetActiveAnimation(3 + _animationOffset);
							_animationLock = 3;
						}
						break;
				}

				if (!_moving)
				{
                    spriteAnimator.SetActiveAnimation(spriteAnimator.GetActiveAnimation(), 0);
					spriteAnimator.SetAnimationIndex(-1);
					_animationLock = -1;
				}
			}
		}
		else if (_state == State.Dead)
		{
			_deathWait += Time.deltaTime;
			
			if (_deathWait > 0.45f)
			{
				_deathWait = 0f;
                //La animacion de muerte se destruira sola
				//if (_deathAnimInstance != null) GameObject.Destroy(_deathAnimInstance);
				_health = _maxHealth;
				if (_uiHealthBar != null) _uiHealthBar.transform.localPosition = _uiHealthBarPos;
				_state = State.Respawning;
			}
		}

        //Pixel indicators
        if (_isPlayer)
        {
            if(_game._npcBluePixel != null)
            {
                float shootAngle = Mathf.Atan2(_game._npcBluePixel.transform.position.y, _game._npcBluePixel.transform.position.x) * Mathf.Rad2Deg;
                _blueInd.transform.position = transform.position + new Vector3(Mathf.Cos(shootAngle), Mathf.Sin(shootAngle));
            }
            else
            {
                float shootAngle = Mathf.Atan2(_game._bluePixelInstance.transform.position.y, _game._bluePixelInstance.transform.position.x) * Mathf.Rad2Deg;
                _blueInd.transform.position = transform.position + new Vector3(Mathf.Cos(shootAngle), Mathf.Sin(shootAngle));
            }

            if (_game._npcRedPixel != null)
            {
                float shootAngle = Mathf.Atan2(_game._npcRedPixel.transform.position.y, _game._npcRedPixel.transform.position.x) * Mathf.Rad2Deg;
                _redInd.transform.position = transform.position + new Vector3(Mathf.Cos(shootAngle), Mathf.Sin(shootAngle));
            }
            else
            {
                float shootAngle = Mathf.Atan2(_game._greenPixelInstance.transform.position.y, _game._greenPixelInstance.transform.position.x) * Mathf.Rad2Deg;
                _redInd.transform.position = transform.position + new Vector3(Mathf.Cos(shootAngle), Mathf.Sin(shootAngle));
            }
            
        }

   }//END update()
    
	public void OnHit()
	{
	}
	
	public void OnReset()
	{
		_carriesFlag = false;
		_activeKeys.Clear();
		_health = _maxHealth;
		_currentAmmo = -1;
		_currentWeapon = WeaponType.Pistol;
	}
	
	public void OnDeath()
	{
        switch (_team)
        {
            case Game.GameTeam.Green:

                switch (_currentWeapon)
                {
                    case WeaponType.Pistol:
                    case WeaponType.Machinegun:
                        _deathAnimInstance = (GameObject)GameObject.Instantiate(_deathPrefabs[Random.Range(0,1)], transform.position, Quaternion.identity);
                        break;

                    case WeaponType.Flamethrower:
                        _deathAnimInstance = (GameObject)GameObject.Instantiate(_deathPrefabs[2], transform.position, Quaternion.identity);
                        break;

                    case WeaponType.RPG:
                        _deathAnimInstance = (GameObject)GameObject.Instantiate(_deathPrefabs[Random.Range(3, 4)], transform.position, Quaternion.identity);
                        break;
                }

                break;

            case Game.GameTeam.Blue:

                switch (_currentWeapon)
                {
                    case WeaponType.Pistol:
                    case WeaponType.Machinegun:
                        _deathAnimInstance = (GameObject)GameObject.Instantiate(_deathPrefabs[Random.Range(5, 6)], transform.position, Quaternion.identity);
                        break;

                    case WeaponType.Flamethrower:
                        _deathAnimInstance = (GameObject)GameObject.Instantiate(_deathPrefabs[7], transform.position, Quaternion.identity);
                        break;

                    case WeaponType.RPG:
                        _deathAnimInstance = (GameObject)GameObject.Instantiate(_deathPrefabs[Random.Range(8, 9)], transform.position, Quaternion.identity);
                        break;
                }

                break;
        }
        
		//_deathAnimInstance = (GameObject)GameObject.Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        _deathAnimInstance.transform.localRotation = Quaternion.AngleAxis(_shootAngle, Vector3.forward);
		transform.position = new Vector3(1000f,0f,0f);
		
        //Los sonidos estan en las animaciones de muerte
		//_audioSource3.GetComponent<AudioSource>().Stop();
		//_audioSource3.GetComponent<AudioSource>().clip = _boomAudio;
		//_audioSource3.GetComponent<AudioSource>().Play();
		
		_state = State.Dead;
	}
	
	void OnCollisionEnter2D(Collision2D col)
	{
		if (_state == State.Playing)
		{
			if (col.gameObject.name.Contains("bullet"))
			{
				if (!_audioSource3.GetComponent<AudioSource>().isPlaying)
				{
					_audioSource3.GetComponent<AudioSource>().clip = _hitAudio;
					_audioSource3.GetComponent<AudioSource>().Play();
				}
				
				Vector3 stdir = col.gameObject.GetComponent<BulletController>().GetDirection()*Time.deltaTime;
				transform.position += stdir;
				
				// Get damage
				if (_isPlayer && _team != col.gameObject.GetComponent<BulletController>().GetTeam())
				{
					WeaponType wt = col.gameObject.GetComponent<BulletController>().GetWeaponType();
					
					switch (wt)
					{
						case WeaponType.Pistol:
							_health -= _pistolShootDamage;
							break;
						
						case WeaponType.Machinegun:
							_health -= _machineShootDamage;
							break;
						
						case WeaponType.Flamethrower:
							_health -= _flameShootDamage;
							break;
						
						case WeaponType.RPG:
							_health -= _rpgShootDamage;
							break;
					}
					
					_uiHealthBar.transform.position -= new Vector3(0.3f,0f,0f);
					NetworkController.instance.Hit(_id, wt);
					
					// Drop the flag
					_carriesFlag = false;
					_game.OnDropFlag(transform.position.x + stdir.x*4f, transform.position.y + stdir.y*4f, _id);
					
					// Dead
					if (_health < 0f)
					{
						_deathAnimInstance = (GameObject)GameObject.Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
						_mainCamera.transform.position = transform.position;
						//transform.position = new Vector3(1000f,0f,0f);
						
						_audioSource3.GetComponent<AudioSource>().Stop();
						_audioSource3.GetComponent<AudioSource>().clip = _boomAudio;
						_audioSource3.GetComponent<AudioSource>().Play();
						
						NetworkController.instance.Death(_id);
						
						_state = State.Dead;
					}
				}
			}
			else if (col.gameObject.name.Contains("Weapon"))
			{
				if (_isPlayer)
				{
					string[] nn = col.gameObject.name.Split('_');
					WeaponType wp = WeaponType.Pistol;
					byte loc = byte.Parse(nn[2]);
					
					if (nn[1] == "Machinegun")
					{
						wp = WeaponType.Machinegun;
					}
					else if (nn[1] == "Flamethrower")
					{
						wp = WeaponType.Flamethrower;
					}
					else if (nn[1] == "RPG")
					{
						wp = WeaponType.RPG;
					}
						
					SetWeapon(wp);
					NetworkController.instance.PickWeapon(_id, loc, wp);
					_game.RemoveWeapon(loc);
				}
			}
		}
	}
	
	void OnTriggerEnter2D(Collider2D col)
	{
		if (_state == State.Playing)
		{
			if (col.gameObject.name.Contains("Blue Flag Spawn"))
			{
				if (_team == Game.GameTeam.Blue && _carriesFlag)
				{
					_carriesFlag = false;
					
					if (_isPlayer)
					{
						_game.OnShipFlag(_id);
						NetworkController.instance.ShipFlag(_id);
					}
				}
			}
			else if (col.gameObject.name.Contains("Green Flag Spawn"))
			{
				if (_team == Game.GameTeam.Green && _carriesFlag)
				{
					_carriesFlag = false;
					
					if (_isPlayer)
					{
						_game.OnShipFlag(_id);
						NetworkController.instance.ShipFlag(_id);
					}
				}
			}
			else if (col.gameObject.name.Contains("Pixel_Blue"))
			{
				if (_isPlayer)
				{
					if (_team == Game.GameTeam.Blue)
					{
						_carriesFlag = false;
						float dd = Vector3.Distance(col.gameObject.transform.position, _game._blueTeamBase.transform.position);
						
						if (dd > 1f)
						{
							if (_isPlayer)
							{
								_game.OnGrabFlag(_id, _team);
								NetworkController.instance.GrabFlag(_id, _team);
							}
						}
						else
						{
							col.gameObject.transform.position = _game._blueTeamBase.transform.position;
						}
					}
					else
					{
						_carriesFlag = true;
						
						if (_isPlayer)
						{
							_game.OnGrabFlag(_id, Game.GameTeam.Blue);
							NetworkController.instance.GrabFlag(_id, Game.GameTeam.Blue);
						}
					}
				}
			}
			else if (col.gameObject.name.Contains("Pixel_Red"))
			{
				if (_team == Game.GameTeam.Green)
				{
					_carriesFlag = false;
					float dd = Vector3.Distance(col.gameObject.transform.position, _game._greenTeamBase.transform.position);
					
					if (dd > 1f)
					{
						if (_isPlayer)
						{
							_game.OnGrabFlag(_id, _team);
							NetworkController.instance.GrabFlag(_id, _team);
						}
					}
					else
					{
						col.gameObject.transform.position = _game._greenTeamBase.transform.position;
					}
				}
				else
				{
					_carriesFlag = true;
					
					if (_isPlayer)
					{
						_game.OnGrabFlag(_id, Game.GameTeam.Green);
						NetworkController.instance.GrabFlag(_id, Game.GameTeam.Green);
					}
				}
			}
		}
    }
}
