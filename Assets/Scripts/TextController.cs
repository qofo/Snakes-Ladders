using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextController : MonoBehaviour
{
    public Vector2 floorPosition;
    void Update()
    {
        this.transform.position = Camera.main.WorldToScreenPoint(floorPosition);
    }

    public void SetPosition(Vector2 position)
    {
        this.floorPosition = position;
    }
}
