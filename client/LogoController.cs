using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LogoController : MonoBehaviour
{
	private float _timer = 0f;
	
	void Update ()
	{
		_timer += Time.deltaTime;
		
		if (_timer > 3.84f)
		{
			SceneManager.LoadScene("GameScene");
		}
	}
}
