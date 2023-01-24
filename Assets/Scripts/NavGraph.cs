using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavNode
{
    [SerializeField]
    public int id;
    [SerializeField]
    public Vector3 position;
 
    // Used for pathfinding so we don't care about serializing them.
    [System.NonSerialized]
    public NavNode parent;
    [System.NonSerialized]
    public int g;
}

[System.Serializable]
public class NavGraph : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    public List<NavNode> nodes = new List<NavNode>();
    private int numNodes;

    [HideInInspector]
    public ConnectionsDict connections = new ConnectionsDict();

    public void OnBeforeSerialize()
    {
        if (numNodes == 0) numNodes = nodes.Count;

        if (numNodes > nodes.Count)
        {
            numNodes = nodes.Count;

            bool found = false;
            int removedId = -1;
            for (int i = 0; i < nodes.Count; i += 1)
            {
                if (nodes[i].id != i)
                {
                    removedId = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                removedId = nodes.Count;
            }

            ConnectionsDict cd = new ConnectionsDict();
            foreach ((int a, int b) nodePair in connections.Keys)
            {
                if (nodePair.a != removedId && nodePair.b != removedId)
                {
                    // If the connection contains a node that has an id above the one we removed,
                    // we need to decrement each of the connection's indices,
                    // to keep it in line with the new node ids.
                    (int a, int b) toAdd = nodePair;
                    if (nodePair.a > removedId) toAdd.a -= 1;
                    if (nodePair.b > removedId) toAdd.b -= 1;
                    
                    cd.Add(toAdd, connections[nodePair]);
                }
            }
            connections = cd;
        }
        else if (numNodes < nodes.Count)
        {
            numNodes = nodes.Count;
        }

        int id = 0;
        foreach (NavNode n in nodes)
        {
            n.id = id;
            id += 1;
        }
    }
    
    public void OnAfterDeserialize()
    {
    }

    public NavNode GetNodeById(int id)
    {
        if (id < 0 || id > nodes.Count) 
        {
            return null;
        }

        for (int i = 0; i < nodes.Count; i += 1)
        {
            if (id == nodes[i].id)
            {
                return nodes[i];
            }
        }

        // Getting to this point means we have an id that doesn't exist in the navgraph,
        // which means we should remove all instances of this id from the keys of the dictionary
        // This probably doesn't happen anymore. - Amie 13/04/22
        ConnectionsDict cd = new ConnectionsDict();
        foreach ((int a, int b) nodePair in connections.Keys)
        {
            if (nodePair.a != id && nodePair.b != id)
            {
                cd.Add(nodePair, connections[nodePair]);
            }
        }
        connections = cd;
        return null;
    }

    private List<int> GetConnectionsToNode(int id)
    {
        List<int> result = new List<int>();
        foreach ((int a, int b) nodePair in connections.Keys)
        {
            if (nodePair.a == id)
            {
                result.Add(nodePair.b);
            }
            else if (nodePair.b == id)
            {
                result.Add(nodePair.a);
            }
        }
        return result;
    }

    private NavNode GetNearestNode(Vector3 position)
    {
        float minimumDistance = float.MaxValue;
        NavNode result = null;

        foreach (NavNode node in nodes)
        {
            float distance = (position - node.position).magnitude;
            if (distance < minimumDistance)
            {
                minimumDistance = distance;
                result = node;
            }
        }

        return result;
    }

    public List<Vector3> GetPath(Vector3 start, Vector3 end)
    {
        NavNode startNode = GetNearestNode(start);
        NavNode endNode   = GetNearestNode(end);

        int NodeSort(NavNode a, NavNode b) {
            if (a.g < b.g) return -1;
            else if (a.g == b.g) return 0;
            else return 1;
        }

        // A* algorithm or at least an attempt at one.
        Stack<NavNode> closedList = new Stack<NavNode>();
        List<NavNode> openList = new List<NavNode>(); // Priority queue

        openList.Add(startNode);
        List<int> visitedIds = new List<int>();
        visitedIds.Add(startNode.id);

        while (openList.Count > 0)
        {
            openList.Sort(NodeSort);

            NavNode currentNode = openList[0];
            openList.RemoveAt(0);

            closedList.Push(currentNode);

            if (currentNode == endNode)
            {
                return FoundPath(currentNode);
            }

            List<int> connectionsToCurrentNode = GetConnectionsToNode(currentNode.id);
            for (int i = 0; i < connectionsToCurrentNode.Count; i += 1)
            {
                NavNode n = GetNodeById(connectionsToCurrentNode[i]);
                if (closedList.Contains(n) || n == null) continue;

                int g = 0;
                if (currentNode.id < connectionsToCurrentNode[i])
                {
                    g = currentNode.g + connections[(currentNode.id, connectionsToCurrentNode[i])];
                }
                else
                {
                    g = currentNode.g + connections[(connectionsToCurrentNode[i], currentNode.id)];
                }

                if (g <= n.g)
                {
                    n.g = g;
                }
                if (!openList.Contains(n))
                {
                    n.g = g;
                    openList.Add(n);
                    n.parent = currentNode;
                }
            }
        }
    
        // We had better never get here; that means the algorithm never found a path from the start to the end.
        throw new System.InvalidProgramException();
    }

    private List<Vector3> FoundPath(NavNode endNode)
    {
        List<Vector3> result = new List<Vector3>();
        result.Add(endNode.position);
        while (endNode.parent != null)
        {
            result.Add(endNode.parent.position);
            endNode = endNode.parent;
        }
        result.Reverse();
        ResetNodes();
        return result;
    }

    private void ResetNodes()
    {
        foreach (NavNode node in nodes)
        {
            node.parent = null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);

        foreach (NavNode node in nodes)
        {
            Gizmos.DrawCube(node.position, size);
        }
    }
#endif
}
