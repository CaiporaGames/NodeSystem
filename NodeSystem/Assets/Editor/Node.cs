using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

//Will be responsable for draw itself and handle events
public class Node
{
    public Rect rect;//Node position
    public string title;//Node title

    public bool isDragged;
    public bool isSelected;//When true allows the code to display a light blue to tell that this node is highlighted

    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;

    public Action<Node> OnRemoveNode;//Delete to tell that this node must be remuved from the node list

    //This styles is to know when the node is select or not
    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    //Constructor to set the node parameters.
    public Node(Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<Node> OnClickRemoveNode)
    {       
        rect = new Rect(position.x, position.y, width, height);
        style = nodeStyle;
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);

        //Set the styles when the node is select or unselected
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;

        OnRemoveNode = OnClickRemoveNode;
    }


    //Update the node position when is dragged.
    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }
    Rect pos;
    //Draw the nodes and its star/end entries.
    public void Draw()
    {
       
        inPoint.Draw();
        outPoint.Draw();
        GUI.Box(rect, title, style);
    }

//Process the input. For each input an action.
    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        style = selectedNodeStyle;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
                    }
                }
                //Verification when removing a node
                if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                {
                    ProcessContextMenu();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag://Handle mouse drag
                if (e.button == 0 && isDragged)
                {
                    Drag(e.delta);
                    //We use Use method this is to avoid dragging the whole canvas. It prevents the event being used by other process
                    e.Use();
                    return true;
                }
                break;
        }
        return false;
    }

    //Construct the menu to the Remove button to remove a node
    private void ProcessContextMenu()
    {
        //Create a custom menu.
        GenericMenu genericMenu = new GenericMenu();
        //Add the popup button asking to remove node. 
        genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        //Display the button from above.
        genericMenu.ShowAsContext();
    }

    //Send a message to remove this node from the nodes list
    private void OnClickRemoveNode()
    {
        if (OnRemoveNode != null)
        {
            OnRemoveNode(this);
        }
    }    
}