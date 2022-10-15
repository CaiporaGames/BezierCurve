using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator creator;
    Path Path
    {
        get
        {
            return creator.path;
        }
    }
    const float segmentSelectDistanceThreshold = 0.1f;
    int selectedSegmentIndex = -1;

    void OnEnable() 
    {
        creator = (PathCreator)target;
        if(creator.path == null)
        {
            creator.CreatePath();
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        EditorGUI.BeginChangeCheck();
        if(GUILayout.Button("Create New Path"))
        {
            Undo.RecordObject(creator, "Create New Path");
            creator.CreatePath();
        }

        bool isClosed = GUILayout.Toggle(Path.IsClosed, "Closed Path");
         if(Path.IsClosed != isClosed)
        {
            Undo.RecordObject(creator, "Toggle Closed");
            Path.IsClosed = isClosed;
        }

        bool autoSetControlsPoints = GUILayout.Toggle(Path.AutoSetControlsPoints, "Auto Set Controls Points");
        if(autoSetControlsPoints != Path.AutoSetControlsPoints)
        {
            Undo.RecordObject(creator, "Toggle auto set controls");
            Path.AutoSetControlsPoints = autoSetControlsPoints;
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

        //Add a segment on left mouse down + dolding shift button
        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            if(selectedSegmentIndex != -1)
            {
                Undo.RecordObject(creator, "Split Segment");
                Path.SplitSegment(mousePos, selectedSegmentIndex);
            }
            else if(!Path.IsClosed)
            {
                Undo.RecordObject(creator, "Add Segment");
                Path.AddSegments(mousePos);
            }
        }

        //Deleting the point on right mouse down
        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDistanceToAnchor = 0.5f;
            int closestAnchorIndex = -1;

            for(int i = 0; i < Path.NumPoints; i+= 3)
            {
                float distance = Vector2.Distance(mousePos, Path[i]);
                if(distance < minDistanceToAnchor)
                {
                    minDistanceToAnchor = distance;
                    closestAnchorIndex = i;
                }
            }

            if(closestAnchorIndex != -1)
            {
                Undo.RecordObject(creator, "Delete Segment");
                Path.DeleteSegment(closestAnchorIndex);
            }
        }

        //Split line code
        if(guiEvent.type == EventType.MouseMove)
        {
            float minimumDistance = segmentSelectDistanceThreshold;
            int newSelectedSegmentIndex = -1;

            for(int i = 0; i < Path.NumSegments; i++)
            {
                Vector2[] points = Path.GetPointsInSegment(i);
                float distance = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                if(distance < minimumDistance)
                {
                    minimumDistance = distance;
                    newSelectedSegmentIndex = i;
                }

            }
            if(newSelectedSegmentIndex != selectedSegmentIndex)
            {
                selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }

        //Avoid selecting up the road mesh on the background when trying to insert a point in the path
        HandleUtility.AddDefaultControl(0);
    }

    void DrawHandles()
    {
        DrawCurve();

        Vector2 newPos;

        for(int i = 0; i < Path.NumPoints; i++)
        {
            Handles.color = PointsColor(i, out float t);

            if(Path.AutoSetControlsPoints && i%3 != 0) continue;

            newPos = Handles.FreeMoveHandle(Path[i], Quaternion.identity, t, Vector2.zero, Handles.CylinderHandleCap);
            if(newPos != Path[i])
            {
                Undo.RecordObject(creator, "Moved Point");
                Path.MovePoint(i, newPos);
            }
        }
    }

   
    void DrawCurve()
    {
        Color segmentColor;

        for(int i = 0; i < Path.NumSegments; i++)
        {
            Vector2[] points = Path.GetPointsInSegment(i);

            if(!Path.AutoSetControlsPoints)
            {
                Handles.color = creator.controlSegmentColor;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
            }
            segmentColor = selectedSegmentIndex == i && Event.current.shift ? creator.selectedPathColor : creator.pathColor;

            Handles.DrawBezier(points[0],points[3],points[1],points[2], segmentColor,null, 2);
        }
    }

     Color PointsColor(int i, out float t)
    {
        if(i%3 == 0)
        {
            t = creator.anchorPointSize;
            return creator.anchorPointColor;
        }
        else
        {
            t = creator.controlPointSize;
            return  creator.controlPointColor;
        }
    }

  
}
