using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator creator;
    Path path;

    void OnEnable() 
    {
        creator = (PathCreator)target;
        if(creator.path == null)
        {
            creator.CreatePath();
        }
        path = creator.path;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        EditorGUI.BeginChangeCheck();
        if(GUILayout.Button("Create New Path"))
        {
            Undo.RecordObject(creator, "Create New Path");
            creator.CreatePath();
            path = creator.path;
        }

         if(GUILayout.Button("Toggle Closed"))
        {
            Undo.RecordObject(creator, "Toggle Closed");
            path.ToggleClosed();
        }

        bool autoSetControlsPoints = GUILayout.Toggle(path.AutoSetControlsPoints, "Auto Set Controls Points");
        if(autoSetControlsPoints != path.AutoSetControlsPoints)
        {
            Undo.RecordObject(creator, "Toggle auto set controls");
            path.AutoSetControlsPoints = autoSetControlsPoints;
        }
        if(EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    void OnSceneGUI() {
        InputNewPoint();
        DrawHandles ();
    }

    void InputNewPoint()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Undo.RecordObject(creator, "Segment Added");
            path.AddSegments(mousePos);
        }
    }

    void DrawHandles()
    {
        DrawCurve();

        for(int i = 0; i < path.NumPoints; i++)
        {
            Handles.color = PointsColor(i, out float t);
            Vector2 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, t, Vector2.zero, Handles.CylinderHandleCap);

            if(newPos != path[i])
            {
                Undo.RecordObject(creator, "Moved Point");
                path.MovePoint(i, newPos);
            }
        }
    }

   
    void DrawCurve()
    {
        for(int i = 0; i < path.NumSegments; i++)
        {
            Vector2[] points = path.GetPointsInSegment(i);
            Handles.color = Color.black;
            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawBezier(points[0],points[3],points[1],points[2], Color.green,null, 2);
        }
    }

     Color PointsColor(int i, out float t)
    {
        if(i%3 == 0)
        {
            t = 0.1f;
            return Color.red;
        }
        else
        {
            t = 0.05f;
            return Color.white;
        }
    }

  
}
