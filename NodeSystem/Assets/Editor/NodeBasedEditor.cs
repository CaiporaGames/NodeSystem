using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class NodeBasedEditor : EditorWindow
{
    //List with all nodes.
    private List<Node> nodes;
    //List all connection
    private List<Connection> connections;

    //Instances for in and out points
    private ConnectionPoint selectedInPoint;
    private ConnectionPoint selectedOutPoint;

    //Here we give style. Will be similar to animator window
    private GUIStyle nodeStyle;
    //Styles when the node is selected or not
    private GUIStyle selectedNodeStyle;

    //Styles for in and out
    private GUIStyle inPointStyle;
    private GUIStyle outPointStyle;

    //to drag the canvas
    private Vector2 drag;
    private Vector2 offset;

    [MenuItem("Tools/Node Based Editor")]
    private static void OpenWindow()
    {        
        NodeBasedEditor window = GetWindow<NodeBasedEditor>();
        window.titleContent = new GUIContent("Node Based Editor");
        window.wantsMouseMove = true;
    }

    private void OnEnable()
    {
        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);

        inPointStyle = new GUIStyle();
        inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        inPointStyle.border = new RectOffset(4, 4, 12, 12);

        outPointStyle = new GUIStyle();
        outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        outPointStyle.border = new RectOffset(4, 4, 12, 12);

        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
    }

    float zoomScale = 1.0f;

    void OnGUI()
    {
        HandleEvents();

        // The zoom area clipping is sometimes not fully confined to the passed in rectangle. At certain
        // zoom levels you will get a line of pixels rendered outside of the passed in area because of
        // floating point imprecision in the scaling. Therefore, it is recommended to draw the zoom
        // area first and then draw everything else so that there is no undesired overlap.
        DrawZoomArea();

        EditorZoomArea.Begin(_zoom, _zoomArea);
        ProcessEvents(Event.current);
        ProcessNodeEvents(Event.current);

        //To draw the grid on the background
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawConnectionLine(Event.current);

        DrawNodes();
        DrawConnections();

        if (GUI.changed)
        {
            Repaint();
        }
        EditorZoomArea.End();
    }

    //private void OnGUI()
    //{


    //    Matrix4x4 oldMatrix = GUI.matrix;

    //    //Scale my gui matrix
    //    Vector2 vanishingPoint = new Vector2(0, 20);
    //    Matrix4x4 Translation = Matrix4x4.TRS(vanishingPoint, Quaternion.identity, Vector3.one);
    //    Matrix4x4 Scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
    //    GUI.matrix = Translation * Scale * Translation.inverse;


    //    ProcessEvents(Event.current);
    //    ProcessNodeEvents(Event.current);

    //    //To draw the grid on the background
    //    DrawGrid(20, 0.2f, Color.gray);
    //    DrawGrid(100, 0.4f, Color.gray);

    //    DrawConnectionLine(Event.current);

    //    DrawNodes();
    //    DrawConnections();

    //    if (GUI.changed)
    //    {
    //        Repaint();
    //    }



    //    //reset the matrix
    //    GUI.matrix = oldMatrix;
    //    var e = Event.current;
    //    if (e.type == EventType.ScrollWheel)
    //    {
    //        var zoomDelta = 0.1f;
    //        zoomDelta = e.delta.y < 0 ? zoomDelta : -zoomDelta;
    //        zoomScale += zoomDelta;
    //        zoomScale = Mathf.Clamp(zoomScale, 0.25f, 1.25f);
    //        e.Use();
    //    }
    //}

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    //This draw the bezier from the node to the mouse position
    private void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
                selectedInPoint.rect.center,
                e.mousePosition,
                selectedInPoint.rect.center + Vector2.left * 50f,
                e.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
                selectedOutPoint.rect.center,
                e.mousePosition,
                selectedOutPoint.rect.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }
    }

    private void DrawNodes()
    {
        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Draw();
            }
        }
    }

    private void DrawConnections()
    {
        if (connections != null)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].Draw();
            }
        }
    }

    //process the right click and show the add node button.
    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                }
                break;
        }
    }

    //Drag to the canvas
    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Drag(delta);
            }
        }

        GUI.changed = true;
    }

    private void ProcessNodeEvents(Event e)
    {
        if (nodes != null)
        {
            //This runs from backwards because the last node is drawn at the top
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = nodes[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }


    //Create the node.
    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    private void OnClickAddNode(Vector2 mousePosition)
    {
        if (nodes == null)
        {
            nodes = new List<Node>();
        }
        //Place the node on the mouse position with Rect position
        nodes.Add(new Node(mousePosition, 200, 50, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
    }

    private void OnClickRemoveNode(Node node)
    {
        if (connections != null)
        {
            List<Connection> connectionsToRemove = new List<Connection>();

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].inPoint == node.inPoint || connections[i].outPoint == node.outPoint)
                {
                    connectionsToRemove.Add(connections[i]);
                }
            }

            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                connections.Remove(connectionsToRemove[i]);
            }

            connectionsToRemove = null;
        }

        nodes.Remove(node);
    }

    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickRemoveConnection(Connection connection)
    {
        connections.Remove(connection);
    }

    private void CreateConnection()
    {
        if (connections == null)
        {
            connections = new List<Connection>();
        }

        connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
    }

    private void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }



    /////$$$$$$$$$$$$$$$$$$$$$$$$$
    ///


    private const float kZoomMin = 0.2f;
    private const float kZoomMax = 2.0f;

    private readonly Rect _zoomArea = new Rect(0.0f, 0.0f, Screen.width,Screen.height);
    private float _zoom = 1.0f;
    private Vector2 _zoomCoordsOrigin = Vector2.zero;
    private Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords)
    {
        return (screenCoords - _zoomArea.TopLeft()) / _zoom + _zoomCoordsOrigin;
    }
    private void HandleEvents()
    {
        // Allow adjusting the zoom with the mouse wheel as well. In this case, use the mouse coordinates
        // as the zoom center instead of the top left corner of the zoom area. This is achieved by
        // maintaining an origin that is used as offset when drawing any GUI elements in the zoom area.
        if (Event.current.type == EventType.ScrollWheel)
        {
            Vector2 screenCoordsMousePos = Event.current.mousePosition;
            Vector2 delta = Event.current.delta;
            Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(screenCoordsMousePos);
            float zoomDelta = -delta.y / 150.0f;
            float oldZoom = _zoom;
            _zoom += zoomDelta;
            _zoom = Mathf.Clamp(_zoom, kZoomMin, kZoomMax);
            _zoomCoordsOrigin += (zoomCoordsMousePos - _zoomCoordsOrigin) - (oldZoom / _zoom) * (zoomCoordsMousePos - _zoomCoordsOrigin);

            Event.current.Use();
        }

        // Allow moving the zoom area's origin by dragging with the middle mouse button or dragging
        // with the left mouse button with Alt pressed.
        if ((Event.current.modifiers == EventModifiers.Alt && Event.current.button == 2))
           // if (Event.current.type == EventType.MouseDrag &&
           //(Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) ||
           //Event.current.button == 2)
            {
           // Vector2 delta = Event.current.delta;
            Vector2 delta = Event.current.mousePosition;
            delta /= _zoom;
            _zoomCoordsOrigin += delta;

            Event.current.Use();
        }
    }


    private void DrawZoomArea()
    {
        // Within the zoom area all coordinates are relative to the top left corner of the zoom area
        // with the width and height being scaled versions of the original/unzoomed area's width and height.
        //EditorZoomArea.Begin(_zoom, _zoomArea);

       // GUI.Box(new Rect(0.0f - _zoomCoordsOrigin.x, 0.0f - _zoomCoordsOrigin.y, Screen.width, Screen.height), "Zoomed Box");

        //// You can also use GUILayout inside the zoomed area.
        //GUILayout.BeginArea(new Rect(300.0f - _zoomCoordsOrigin.x, 70.0f - _zoomCoordsOrigin.y, 130.0f, 50.0f));
        //GUILayout.Button("Zoomed Button 1");
        //GUILayout.Button("Zoomed Button 2");
        //GUILayout.EndArea();

        //EditorZoomArea.End();
    }


}