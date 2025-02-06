using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        Cursor.visible = false;
#if UNITY_EDITOR
        Cursor.visible = true;
#endif
    }
}
