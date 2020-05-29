using UnityEngine;
using System.Collections;

public class ExplosionController : MonoBehaviour
{
	public float _destructionTime = 0.6f;
	private float _timer = 0f;
	
	void Update ()
	{
		_timer += Time.deltaTime;
		
		if (_timer > _destructionTime)
		{
			GetComponent<AudioSource>().Play();
			GameObject.Destroy(this.gameObject);
		}
	}
}
