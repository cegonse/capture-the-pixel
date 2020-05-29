using UnityEngine;
using System.Collections;

public class PointsController : MonoBehaviour
{
	public GameObject _blueScore0;
	public GameObject _blueScore1;
	
	public GameObject _greenScore0;
	public GameObject _greenScore1;
	
	public GameObject _slash;
	
	public Sprite[] _blueNumbers;
	public Sprite[] _greenNumbers;
	
	private SpriteRenderer _blue0Rend;
	private SpriteRenderer _blue1Rend;
	private SpriteRenderer _green0Rend;
	private SpriteRenderer _green1Rend;
	private SpriteRenderer _slashRend;
	
	private float _timeoutTimer = 0f;
	private bool _waitingTimeout;
	
	public void ShowScore(bool s, float timeout = 0f)
	{
		_blue0Rend.enabled = s;
		_blue1Rend.enabled = s;
		_green0Rend.enabled = s;
		_green1Rend.enabled = s;
		_slashRend.enabled = s;
		
		if (timeout != 0f)
		{
			_waitingTimeout = true;
			_timeoutTimer = timeout;
		}
	}
	
	void Update()
	{
		if (_waitingTimeout)
		{
			_timeoutTimer -= Time.deltaTime;
			
			if (_timeoutTimer < 0f)
			{
				_blue0Rend.enabled = false;
				_blue1Rend.enabled = false;
				_green0Rend.enabled = false;
				_green1Rend.enabled = false;
				_slashRend.enabled = false;
				
				_waitingTimeout = false;
			}
		}
	}
	
	void Start()
	{
		_blue0Rend = _blueScore0.GetComponent<SpriteRenderer>();
		_blue1Rend = _blueScore1.GetComponent<SpriteRenderer>();
		_green0Rend = _greenScore0.GetComponent<SpriteRenderer>();
		_green1Rend = _greenScore1.GetComponent<SpriteRenderer>();
		_slashRend = _slash.GetComponent<SpriteRenderer>();
	}
	
	public void OnChangeScore(int blue, int green)
	{
		int b0 = blue % 10;
		int b1 = blue / 10;
		int g0 = green % 10;
		int g1 = green / 10;
		
		_blue0Rend.sprite = _blueNumbers[b0];
		_blue1Rend.sprite = _blueNumbers[b1];
		_green0Rend.sprite = _greenNumbers[g0];
		_green1Rend.sprite = _greenNumbers[g1];
	}
}
