using UnityEngine;
using System.Collections;

[System.Serializable]
public class ArraySpriteLayout
{
    [System.Serializable]
    public struct rowData
    {
        public frameData[] column;
    }

    [System.Serializable]
    public struct frameData
    {
        public Sprite sprite;
        public float timeToNext;
    }

    public rowData[] row = new rowData[6];
}
