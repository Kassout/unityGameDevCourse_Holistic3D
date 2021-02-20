using UnityEngine;
using UnityEngine.UI;

public class MakeRadarObject : MonoBehaviour
{
    public Image image;
    
    // Start is called before the first frame update
    void Start()
    {
        Radar.RegisterRadarObject(gameObject, image);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Radar.RemoveRadarObject(gameObject);
    }
}
