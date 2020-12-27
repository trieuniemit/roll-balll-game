using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResize : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float aspect = (float)Screen.height / (float)Screen.width;
        Camera.main.orthographicSize = aspect >= 1.87 ? 7f : 6.2f;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
