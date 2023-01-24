using System.Collections.Generic;
using UnityEngine;

// Unity doesn't know how to serialize (save) dictionaries, so I found this thing
// https://answers.unity.com/questions/460727/how-to-serialize-dictionary-with-unity-serializati.html
// that explains a way to get around it.
// Unity *does* know how to serialize Lists, so let's save and load using those.
[System.Serializable]
public class ConnectionsDict : Dictionary<(int a, int b), int>, ISerializationCallbackReceiver
{
    [SerializeField] private List<int> keysA = new List<int>();
    [SerializeField] private List<int> keysB = new List<int>();
    [SerializeField] private List<int> values = new List<int>();

    // Right before serialization, format the data into a form that Unity can serialize.
    public void OnBeforeSerialize()
    {
        keysA.Clear();
        keysB.Clear();
        values.Clear();

        foreach (KeyValuePair<(int a, int b), int> kv in this)
        {
            // Always put the lower id into the keysA list.
            // Keypairs are guaranteed to never be the same value e.g. (1, 1).
            if (kv.Key.a < kv.Key.b)
            {
                keysA.Add(kv.Key.a);
                keysB.Add(kv.Key.b);
            }
            else
            {
                keysA.Add(kv.Key.b);
                keysB.Add(kv.Key.a);
            }
            values.Add(kv.Value);
        }
    }

    // Then when deserializing, transform the data back into a dictionary. No one is any the wiser.
    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keysA.Count != values.Count || keysB.Count != values.Count || keysA.Count != keysB.Count)
        {
            throw new System.Exception(string.Format("If you see this please let Amie know :) Error when deserializing the connections dictionary. There are ({0},{1}) keys and {2} values. All of these numbers must be the same.", keysA.Count, keysB.Count, values.Count));
        }

        for (int i = 0; i < keysA.Count; i += 1)
        {
            this.Add((keysA[i], keysB[i]), values[i]);
        }
    }
}

[System.Serializable]
public class WasteDict : Dictionary<string, GameObject>, ISerializationCallbackReceiver
{
    [SerializeField] private List<string> keys = new List<string>();
    [SerializeField] private List<GameObject> values = new List<GameObject>();

    // Right before serialization, format the data into a form that Unity can serialize.
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (KeyValuePair<string, GameObject> kv in this)
        {
            keys.Add(kv.Key);
            values.Add(kv.Value);
        }
    }

    // Then when deserializing, transform the data back into a dictionary. No one is any the wiser.
    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
        {
            throw new System.Exception(string.Format("If you see this please let Amie know :) Error when deserializing the waste names dictionary. There are {0} keys and {1} values. These numbers must be the same.", keys.Count, values.Count));
        }

        for (int i = 0; i < keys.Count; i += 1)
        {
            this.Add(keys[i], values[i]);
        }
    }
}
