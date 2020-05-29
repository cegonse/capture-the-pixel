using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour
{
	public float _speed = 10f;
	public float _destructionTime = 1f;
	public GameObject _explosion;
	
	public Sprite[] _rpgSpr0;
	public Sprite[] _rpgSpr22;
	public Sprite[] _rpgSpr45;
	public Sprite[] _rpgSpr67;
	
	public Vector3 _direction = Vector3.zero;
	private float _timer = 0f;
	private PlayerController.WeaponType _weapon;
	private Game.GameTeam _team;
	
	// Update is called once per frame
	void Update()
	{
		transform.position += _direction * _speed * Time.deltaTime;
		_timer += Time.deltaTime;
		
		if (_timer > _destructionTime)
		{
			if (_weapon == PlayerController.WeaponType.RPG)
			{
				GameObject.Instantiate(_explosion, transform.position, Quaternion.identity);
			}
			
			GameObject.Destroy(gameObject);
		}
	}
	
	public void SetWeaponType(PlayerController.WeaponType wep)
	{
		_weapon = wep;
	}
	
	public PlayerController.WeaponType GetWeaponType()
	{
		return _weapon;
	}
	
	public void SetDirection(Vector3 d)
	{
		_direction = d;
		
		if (_weapon == PlayerController.WeaponType.RPG)
		{
			float shootAngle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            Debug.Log(shootAngle.ToString());
			if (shootAngle > 78.5f && shootAngle <= 101.25f)
			{
				shootAngle = 90f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr0;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > 56.25f && shootAngle <= 78.75f)
			{
				shootAngle = 90f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr22;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > 33.75f && shootAngle <= 56.25f)
			{
				shootAngle = 90f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr45;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > 11.25f && shootAngle <= 33.75f)
			{
				shootAngle = 90f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr67;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > 101.25f && shootAngle <= 123.75)
			{
				shootAngle = 180f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr67;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > 123.75 && shootAngle <= 145.25)
			{
				shootAngle = 180f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr45;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > 145.25f && shootAngle <= 167.75)
			{
				shootAngle = 180f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr22;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if ((shootAngle > 167.75f && shootAngle <= 180f)
				|| (shootAngle > -180f && shootAngle <= -168.75f))
			{
				shootAngle = 180f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr0;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > -168.75 && shootAngle <= -146.25f)
			{
				shootAngle = -90f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr67;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > -146.25 && shootAngle <= -123.75f)
			{
				shootAngle = -90f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr45;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > -123.75 && shootAngle <= -101.25)
			{
				shootAngle = -90f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr22;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > -101.25f && shootAngle <= -78.75f)
			{
				shootAngle = -90f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr0;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > -78.75f && shootAngle <= -56.25f)
			{
				shootAngle = 0f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr67;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > -56.25f && shootAngle <= -33.75f)
			{
				shootAngle = 0f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr45;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if (shootAngle > -33.75f && shootAngle <= -11.25f)
			{
				shootAngle = 0f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr22;
                spriteAnimator.PlaySpriteAnimator();
			}
			else if ((shootAngle < 45f && shootAngle >= 0f)
				|| (shootAngle >= -45f && shootAngle < 0f)) 
			{
				shootAngle = 0f;
                SpriteAnimatorSimple spriteAnimator = GetComponent<SpriteAnimatorSimple>();
                spriteAnimator._frames = _rpgSpr0;
                spriteAnimator.PlaySpriteAnimator();
			}
			
			transform.localRotation = Quaternion.AngleAxis(shootAngle, Vector3.forward);
			//GetComponent<SpriteAnimatorSimple>()._isRunning = true;
        }
        else if (_weapon == PlayerController.WeaponType.Flamethrower)
        {
            transform.localRotation = Quaternion.AngleAxis(90*Random.Range(0,3), Vector3.forward);
        }
	}
	
	public Vector3 GetDirection()
	{
		return _direction;
	}
	
	public void SetTeam(Game.GameTeam t)
	{
		_team = t;
	}
	
	public Game.GameTeam GetTeam()
	{
		return _team;
	}
	
	void OnCollisionEnter2D(Collision2D col)
	{
        PlayerController playCtrl = col.gameObject.GetComponent<PlayerController>();
        if ((playCtrl == null || playCtrl.GetTeam() != _team))
        {
            if (_weapon == PlayerController.WeaponType.RPG)
            {
                GameObject.Instantiate(_explosion, transform.position, Quaternion.identity);
            }

            GameObject.Destroy(this.gameObject);
        }
		
	}
}
