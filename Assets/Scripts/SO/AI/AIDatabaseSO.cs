using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AISO", menuName = "AIDatabase")]
public class AIDatabaseSO : ScriptableObject
{
    public List<AISO> aiSO = new List<AISO>();

    private Dictionary<int, AISO> aiById;
    private Dictionary<string, AISO> aiByName;

    public void Initialize()
    {
        aiById = new Dictionary<int, AISO>();
        aiByName = new Dictionary<string, AISO>();

        foreach (var ai in aiSO)
        {
            aiById[ai.AiID] = ai;
            aiByName[ai.name] = ai;
        }
    }

    public AISO GetItemById(int id)
    {
        if (aiById == null)
        {
            Initialize();
        }

        if (aiById.TryGetValue(id, out AISO ai))
            return ai;

        return null;
    }

    public AISO GetItemByName(string name)
    {
        if (aiByName == null)
        {
            Initialize();
        }

        if (aiByName.TryGetValue(name, out AISO ai))
            return ai;

        return null;
    }

    public List<AISO> GetItemByType(RoleType type)
    {
        return aiSO.FindAll(stage => stage.roleType == type);
    }
}
