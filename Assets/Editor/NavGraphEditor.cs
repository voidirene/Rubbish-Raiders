using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor (typeof(NavGraph))]
public class NavGraphEditor : Editor
{
    private NavGraph graph;
    private bool editingConnections;
    private bool canChangeConnections;
    private (int a, int b) selectedConnection = (-1, -1);

	private void OnEnable()
	{
		graph = target as NavGraph;
	}

	private void OnSceneGUI()
    {
        if (Event.current.type == EventType.MouseUp)
		{
            canChangeConnections = true;
		}

        DrawNodes();

        if (selectedConnection != (-1, -1))
		{
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10, 10, 150, 50));
            GUILayout.Label($"Between {selectedConnection.a} and {selectedConnection.b}: ");
            
            int current = graph.connections[selectedConnection];
            if (int.TryParse(GUILayout.TextField(graph.connections[selectedConnection].ToString()), out int result) && (result != current))
		    {
                graph.connections[selectedConnection] = result;
                Undo.RecordObject(graph, "NavGraph connection cost");
                EditorUtility.SetDirty(graph);
			}
            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }

    public override void OnInspectorGUI()
	{
        graph = target as NavGraph;

        string buttonText = "";
        if (editingConnections)
		{
            buttonText = "Editing Connections";
		}
        else
		{
            buttonText = "Editing Positions";
		}

        if (GUILayout.Button(buttonText))
		{
            editingConnections = !editingConnections;
        }

        base.OnInspectorGUI();
    }

    private void DrawNodes()
	{
        Vector3 idOffset = new Vector3(0, 1, 0);
        Vector3 connectionCostOffset = new Vector3(0, 1, 0);
        GUIStyle style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;
        
        // Draw all the connections between the nodes
        foreach ((int a, int b) nodePair in graph.connections.Keys)
        {
            NavNode first = graph.GetNodeById(nodePair.a);
            NavNode second = graph.GetNodeById(nodePair.b);
            if (first != null && second != null)
            {
                Handles.DrawDottedLine(first.position, second.position, 4.0f);

                Vector3 directionVector = second.position - first.position;
                float distance = directionVector.magnitude;

                GUI.color = Color.cyan;
                Vector3 midpoint = first.position + (0.5f * distance) * directionVector.normalized;
                Handles.Label(midpoint + connectionCostOffset, graph.connections[nodePair].ToString());
                if (Handles.Button(midpoint, Quaternion.identity, 0.4f, 0.4f, Handles.CubeHandleCap))
			    {
                    if (selectedConnection == (-1, -1) || nodePair != selectedConnection)
				    {
                        selectedConnection = nodePair;
				    }
                    else
				    {
                        selectedConnection = (-1, -1);
				    }
			    }
            }
        }

        // Draw all the nodes
        for (int i = 0; i < graph.nodes.Count; i += 1)
        {
            ref Vector3 nodePosition = ref graph.nodes[i].position;
            GUI.color = Color.white;
            Handles.Label(nodePosition + idOffset, graph.nodes[i].id.ToString(), style);

            if (editingConnections)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 position = Handles.FreeMoveHandle(nodePosition, Quaternion.identity, 0.5f, new Vector3(1, 1, 1), Handles.SphereHandleCap);
                position = Vector3.ProjectOnPlane(position, Vector3.up);

                //Vector3 mousepos = Camera.main.ScreenToWorldPoint(new Vector3(Event.current.mousePosition.x, 0, Event.current.mousePosition.y));
                //Handles.DrawLine(nodePosition, mousepos);

                if (EditorGUI.EndChangeCheck())
				{
                    float minDistance = float.MaxValue;
                    int nearestNode = -1;
                    for (int j = 0; j < graph.nodes.Count; j += 1)
					{
                        // Ignore connections from a node to itself.
                        if (i == j) continue;

                        float distance = (position - graph.nodes[j].position).magnitude;
                        if (distance < minDistance)
						{
                            minDistance = distance;
                            nearestNode = j;
						}
					}

                    if (minDistance < 0.5f && nearestNode != -1)
					{
                        // Add the connections to the nodes, but only if they don't already have that connection their lists.

                        bool connected = false;
                        bool iSmallerThanNearest = false;
                        if (graph.nodes[i].id < graph.nodes[nearestNode].id)
                        {
                            iSmallerThanNearest = true;
                        }
                        
                        if (iSmallerThanNearest)
						{
                            connected = graph.connections.ContainsKey((graph.nodes[i].id, graph.nodes[nearestNode].id)); 
						}
                        else
						{
                            connected = graph.connections.ContainsKey((graph.nodes[nearestNode].id, graph.nodes[i].id));
						}
                        
                        if (!connected && canChangeConnections)
                        {
                            canChangeConnections = false;

                            if (iSmallerThanNearest)
							{
                                graph.connections.Add((graph.nodes[i].id, graph.nodes[nearestNode].id), 1);
							}
                            else
							{
                                graph.connections.Add((graph.nodes[nearestNode].id, graph.nodes[i].id), 1);
                            }

                            Undo.RecordObject(graph, "Connected NavGraph nodes");
                            EditorUtility.SetDirty(graph);
                        }
                        
                        // If the connection was already in both nodes, instead remove the connection.
                        if (connected && canChangeConnections)
						{
                            canChangeConnections = false;

                            if (iSmallerThanNearest)
							{
                                graph.connections.Remove((graph.nodes[i].id, graph.nodes[nearestNode].id));
                            }
                            else
							{
                                graph.connections.Remove((graph.nodes[nearestNode].id, graph.nodes[i].id));
                            }
                            
                            Undo.RecordObject(graph, "Removed NavGraph connection");
                            EditorUtility.SetDirty(graph);
                        }
                    }
                }
            }
            else // Then editing positions
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPosition = Handles.DoPositionHandle(nodePosition, Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(graph, "Moved NavGraph point " + graph.nodes[i].id);
                    EditorUtility.SetDirty(graph);
                    nodePosition = newPosition;
                }
            }
        }
    }
}