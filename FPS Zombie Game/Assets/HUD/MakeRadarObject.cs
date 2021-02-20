using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeRadarObject : MonoBehaviour
{
    public Image image;

    // Start is called before the first frame update
    void Start()
    {
        Radar.RegisterRadarObject(this.gameObject, image);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        Radar.RemoveRadarObject(this.gameObject);
    }
}
