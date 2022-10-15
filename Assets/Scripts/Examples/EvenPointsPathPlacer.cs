using UnityEngine;

[RequireComponent(typeof(PathCreator))]
public class EvenPointsPathPlacer : MonoBehaviour
{
    public float spacing = 0.1f;
    public float resolution = 1;
    PathCreator creator;

    
    void Start()
    {
        creator = GetComponent<PathCreator>();
        Vector2[] points = creator.path.CalculateEvenlySpacedPoints(spacing, resolution);

        //for(int i = 0; i < points.Length; i++)
        foreach(Vector2 p in points)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.transform.position = p;
            g.transform.localScale = Vector3.one * spacing * 0.5f;
        }
    }
}
