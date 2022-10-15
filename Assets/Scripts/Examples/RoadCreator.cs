using UnityEngine;

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadCreator : MonoBehaviour
{
    public float roadWidth = 1;
    [Range(0.05f,1.5f)]
    public float spacing = 1, v, tiling = 1;
    Vector2 forward;
    public bool autoUpdate;

    public void UpdateRoad()
    {
        Path path = GetComponent<PathCreator>().path;
        Vector2[] points = path.CalculateEvenlySpacedPoints(spacing);
        GetComponent<MeshFilter>().mesh = CreateRoadMesh(points, path.IsClosed);

        int textureRepeat = Mathf.RoundToInt(tiling*points.Length*spacing*0.05f);
        GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
    }

   Mesh CreateRoadMesh(Vector2[] points, bool isClosed)
   {
        Vector3[] vertices = new Vector3[points.Length*2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int trianglesNumber = 2*(points.Length-1) + (isClosed ? 2 : 0);
        int[] triangles = new int[trianglesNumber*3];
        int vertexIndex = 0;
        int trianglesIndex = 0;
        Vector2 left;

        float completionPercentage = 0;
        
        for(int i=0; i <points.Length; i++)
        {
            forward = Vector2.zero;

            if(i<points.Length-1 || isClosed)//if is not the last point
            {
                forward+= points[(i+1)%points.Length]-points[i];
            }
            if(i>0 || isClosed)//if is not the first point
            {
                forward += points[i]-points[(i-1+points.Length)%points.Length];
            }

            forward.Normalize();//if it passes by the two above if's, than it will be the avarage. It gives a smooth curvuture between road segments.
            left = new Vector2(-forward.y, forward.x);//Calculate the left road point

            vertices[vertexIndex] = points[i]+left*roadWidth*0.5f;
            vertices[vertexIndex+1] = points[i]-left*roadWidth*0.5f;

            completionPercentage = i/(float)(points.Length-1);
            v = 1-Mathf.Abs(2*completionPercentage-1);
            uvs[vertexIndex] = new Vector2(0, v);
            uvs[vertexIndex+1] = new Vector2(1, v);

            if(i<points.Length-1 || isClosed)
            {
                triangles[trianglesIndex] = vertexIndex;
                triangles[trianglesIndex+1] = (vertexIndex+2)%vertices.Length;
                triangles[trianglesIndex+2] = vertexIndex+1;
                
                triangles[trianglesIndex+3] = vertexIndex+1;
                triangles[trianglesIndex+4] = (vertexIndex+2)%vertices.Length;
                triangles[trianglesIndex+5] = (vertexIndex+3)%vertices.Length;

            }

            vertexIndex += 2;
            trianglesIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh;
   }
}
