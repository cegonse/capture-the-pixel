using UnityEngine;
using System.Collections;

public class TimerController : MonoBehaviour
{
	public Sprite[] _numbers;
	
	public GameObject _minuteUi;
	public GameObject _1SecUi;
	public GameObject _2SecUi;
	
	private SpriteRenderer _minuteRend;
	private SpriteRenderer _1SecRend;
	private SpriteRenderer _2SecRend;
	
	void Start()
	{
		_minuteRend = _minuteUi.GetComponent<SpriteRenderer>();
		_1SecRend = _1SecUi.GetComponent<SpriteRenderer>();
		_2SecRend = _2SecUi.GetComponent<SpriteRenderer>();
	}
	
	public void OnChangeTimer(int minute, int second)
	{
		int s1 = second / 10;
		int s2 = second % 10;
		
		_minuteRend.sprite = _numbers[minute];
		_1SecRend.sprite = _numbers[s1];
		_2SecRend.sprite = _numbers[s2];
	}
}
