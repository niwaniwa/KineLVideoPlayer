
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TargetSetTransform : UdonSharpBehaviour
{
    public GameObject targetObject;
    public GameObject toTransformObject;

    public void Move()
    {
        targetObject.transform.position = toTransformObject.transform.position;
        targetObject.transform.rotation = toTransformObject.transform.rotation;
    }
    
}
