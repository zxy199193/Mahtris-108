using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private const float minValueX = -2.5f;
    private const float minValueY = -5f;
    void Update()
    {
        BlockMove();
    }

    void BlockMove()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)|| Input.GetKeyDown(KeyCode.A))
        {
            transform.position += new Vector3(x: -0.5f, y: 0, z: 0);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            transform.position += new Vector3(x: 0.5f, y: 0, z: 0);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            this.transform.Rotate(xAngle: 0, yAngle: 0, zAngle: 90);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            transform.position += new Vector3(x: 0, y: -0.5f, z: 0);
        }

    }

    void UpdateMap()
    {
        Map.DeleteBlockFromMap(transform);
        foreach (Transform block in transform)
        {
            //???
            int x = DecimalToInteger(value: block.position.x, minValueX);
            int y = DecimalToInteger(value: block.position.y, minValueY);
            //???
            Map.map[x, y] = block;
        }
    }

    private int DecimalToInteger(float value, float minValue)
    {
        //???
        return Mathf.RoundToInt(f: (value - minValue) / 0.5f);
    }


}
