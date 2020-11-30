using MLAPI;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class HoldableManager : NetworkedBehaviour
{
    public static HoldableManager Singleton;
    //Holdable Data
    [SerializeField]private HoldableObjectData[] HoldableData;

#if UNITY_EDITOR
    private void OnValidate()
    {
        string[] guids = AssetDatabase.FindAssets("t:HoldableObjectData", new[] { "Assets/Content/HoldableData" });
        int count = guids.Length;
        if (HoldableData.Length == count) return;
        HoldableData = new HoldableObjectData[count];
        for (int n = 0; n < count; n++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[n]);
            HoldableData[n] = AssetDatabase.LoadAssetAtPath<HoldableObjectData>(path);
        }
    }
#endif

    private void Awake() 
    {
        Singleton = this;
    }

    //A Held Object has Changed for Player 
    public void HeldObjectChanged(PlayerControlObject controlObject) 
    {
        int holdableId = controlObject.holdableId;
        if (controlObject.holdableObject != null && controlObject.holdableObject.gameObject.activeSelf) //Currently Holding Object
        {
            if (holdableId == 0)
            {
                controlObject.holdableObject.gameObject.SetActive(false);
            }
            else
            {
                GameObject holdable = PooledManager.InstantiatePooledObject(GetHoldablePrefabById(holdableId), controlObject.handParent);
                if(holdable != null) 
                {
                    HoldableObject holdableObject = holdable.GetComponent<HoldableObject>();
                    if(holdableObject != null) 
                    {
                        controlObject.holdableObject = holdableObject;
                    }
                }
            }
        }
        else// Currently no held object
        {
            if(holdableId != 0) 
            {
                GameObject holdable = PooledManager.InstantiatePooledObject(GetHoldablePrefabById(holdableId), controlObject.handParent);
                if (holdable != null)
                {
                    HoldableObject holdableObject = holdable.GetComponent<HoldableObject>();
                    if (holdableObject != null)
                    {
                        controlObject.holdableObject = holdableObject;
                    }
                }
            }
        }
    }

    //A Held Object has Changed for AI
    public void HeldObjectChanged(AIControlObject controlObject) 
    {
        int holdableId = controlObject.holdableId;
        if (controlObject.holdableObject != null && controlObject.holdableObject.gameObject.activeSelf) //Currently Holding Object
        {
            if (holdableId == 0)
            {
                controlObject.holdableObject.gameObject.SetActive(false);
            }
            else
            {
                GameObject holdable = PooledManager.InstantiatePooledObject(GetHoldablePrefabById(holdableId), controlObject.handParent);
                if (holdable != null)
                {
                    HoldableObject holdableObject = holdable.GetComponent<HoldableObject>();
                    if (holdableObject != null)
                    {
                        controlObject.holdableObject = holdableObject;
                    }
                }
            }
        }
        else// Currently no held object
        {
            if (holdableId != 0)
            {
                GameObject holdable = PooledManager.InstantiatePooledObject(GetHoldablePrefabById(holdableId), controlObject.handParent);
                if (holdable != null)
                {
                    HoldableObject holdableObject = holdable.GetComponent<HoldableObject>();
                    if (holdableObject != null)
                    {
                        controlObject.holdableObject = holdableObject;
                    }
                }
            }
        }
    }

    //Get the holdable prefab by ID
    private GameObject GetHoldablePrefabById(int holdableId) 
    {
        for (int i = 0; i < HoldableData.Length; i++)
        {
            if(HoldableData[i].holdableId == holdableId) 
            {
                return HoldableData[i].prefab;
            }
        }
        return null;
    }



}

