using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public Path path;
    public Color anchorPointColor = Color.red;
    public Color controlPointColor = Color.white;
    public Color pathColor = Color.green;
    public Color controlSegmentColor = Color.black;
    public Color selectedPathColor = Color.blue;
    public float anchorPointSize = 0.1f;
    public float controlPointSize = 0.05f;

    public void CreatePath()
    {
        path = new Path(transform.position);
    }

    private void Reset() {
        CreatePath();
    }
}
