using Spirit;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class BillboardingMono : MonoBehaviour
{
    public bool IsTesting;
    public Transform RefTransform;
    public Transform Root;


    private void Awake()
    {
        Cashe();
    }
    private void Cashe()
    {
        if(RefTransform == null) RefTransform = Camera.main.GetComponent<Transform>();
        if (Root == null) Root = GetComponent<Transform>();
    }
    private void Update()
    {
        Cashe();
        Root.localEulerAngles = new Vector3(Root.localEulerAngles.x, (IsTesting ? 180- RefTransform.eulerAngles.y : RefTransform.eulerAngles.y), Root.localEulerAngles.z);
    }
}