using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        UnityEngine.Cursor.visible = false;
#if UNITY_EDITOR
        //UnityEngine.Cursor.visible = true;
#endif
    }
}
