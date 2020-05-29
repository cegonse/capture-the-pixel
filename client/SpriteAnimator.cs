using UnityEngine;
using System.Collections;

public struct AnimationFrame
{
	public float timeToNext;
	public Sprite sprite;
	public bool interval;
}

public class SpriteAnimator : MonoBehaviour
{

    //public ArrayList <List<AnimationFrame>> _animations;
    public ArraySpriteLayout _animations;
    public Sprite sprite_default;
    private int _activeAnimation;
    private int _animationIndex = -1;
    private float _animationTimer = 0f;
    private float _animationTimerScaler = 1f;

    // Use this for initialization
    void Start()
    {


    }
    
    // Update is called once per frame
    void Update()
    {

        if (_animationIndex != -1)
        {
            if (_animations.row[_activeAnimation].column[_animationIndex].timeToNext > _animationTimer * _animationTimerScaler)
            {
                _animationTimer += Time.deltaTime;
            }
            else
            {
                if (_animationIndex < _animations.row[_activeAnimation].column.Length - 1)
                {
                    _animationIndex++;
                }
                else
                {
                    _animationIndex = 0;
                }

                GetComponent<SpriteRenderer>().sprite = _animations.row[_activeAnimation].column[_animationIndex].sprite;
                _animationTimer = 0f;
            }
        }

    }

    public void SetActiveAnimation(int animation, int animationIndex = 0)
    {
        _activeAnimation = animation;
        _animationIndex = animationIndex;
        GetComponent<SpriteRenderer>().sprite = _animations.row[_activeAnimation].column[_animationIndex].sprite;
        
    }

    public void SetAnimationIndex(int animationIndex)
    {
        _animationIndex = animationIndex;
    }

    public int GetAnimationIndex()
    {
       return  _animationIndex;
    }

    public void SetAnimationTimerScaler(float scaler)
    {
        _animationTimerScaler = scaler;
    }

    public float GetAnimationTimerScaler()
    {
        return _animationTimerScaler;
    }

    public bool IsTheLastFrame()
    {
        return _animationIndex == _animations.row[_activeAnimation].column.Length - 1;
    }

    public int GetActiveAnimation()
    {
        return _activeAnimation;
    }
    
}
