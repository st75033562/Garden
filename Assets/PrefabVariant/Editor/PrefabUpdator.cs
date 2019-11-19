using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PrefabVariant.Editor
{
    class PrefabUpdator
    {
        private readonly GameObject m_prefab;

        private class NewComponentInfo
        {
            public Component parentComp; // the parent component to copy
            public GameObject targetGo; // the prefab game object to add the new component
        }

        private readonly List<NewComponentInfo> m_newComps = new List<NewComponentInfo>();
        private readonly List<GameObject> m_newGos = new List<GameObject>();

        public PrefabUpdator(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException("prefab");
            }

            if (!prefab.GetComponent<ReferenceCollection>())
            {
                throw new ArgumentException("not a variant prefab");
            }

            m_prefab = prefab;
        }

        public void Update(bool recordChanges)
        {
            var instance = PrefabUtility.InstantiatePrefab(m_prefab) as GameObject;
            try
            {
                Update(instance, recordChanges);
            }
            finally
            {
                GameObject.DestroyImmediate(instance);

                m_newComps.Clear();
                m_newGos.Clear();
            }
        }

        private void Update(GameObject instance, bool recordChanges)
        {
            var refs = instance.GetComponent<ReferenceCollection>();
            if (!refs)
            {
                Debug.LogError("The prefab is not valid inheritor");
                return;
            }

            if (!refs.parentObject)
            {
                Debug.LogError("parent object does not exist");
                return;
            }

            UpdateHierarchies(refs);

            // update the prefab so that we can get file ids
            PrefabUtility.ReplacePrefab(instance, m_prefab);

            AddNewComponents();
            InitNewGameObjectReferences();

            UpdateProperties(recordChanges);

            EditorUtility.SetDirty(m_prefab);
        }

        private void UpdateHierarchies(ReferenceCollection refs)
        {
            Assert.IsNotNull(refs.parentObject);

            var instance = refs.gameObject;
            var changes = refs.gameObject.GetComponent<ObjectChangeCollection>();

            var ourComps = GetComponents(instance, false);
            var parentComps = GetComponents(refs.parentObject, true);

            foreach (var parentCompKV in parentComps)
            {
                if (refs.IsParentRemoved(parentCompKV.Key))
                {
                    continue;
                }

                var refId = refs.GetIdByParentId(parentCompKV.Key);
                if (refId == 0)
                {
                    // TODO: does not work when the component has required components
                    m_newComps.Add(new NewComponentInfo { 
                        targetGo = (GameObject)PrefabUtility.GetPrefabParent(instance),
                        parentComp = parentCompKV.Value
                    });
                }
                else if (!ourComps.ContainsKey(refId))
                {
                    refs.SetParentRemoved(parentCompKV.Key);
                    changes.Remove(refId);

                    Debug.Log("deleted component " + parentCompKV.Value.GetType().Name);
                }
            }

            foreach (var compKV in ourComps)
            {
                var parentFileId = refs.GetParentId(compKV.Key);
                if (parentFileId != 0)
                {
                    Component parentComp;
                    if (!parentComps.TryGetValue(parentFileId, out parentComp))
                    {
                        Debug.Log("parent removed component " + compKV.Value.GetType().Name);

                        // parent component has been deleted, delete ours
                        Undo.DestroyObjectImmediate(compKV.Value);
                        refs.Remove(compKV.Key);
                        changes.Remove(compKV.Key);
                    }
                }
                // else a new component
            }

            var parentTrans = new List<Transform>();
            foreach (Transform trans in refs.parentObject.transform)
            {
                parentTrans.Add(trans);
            }

            var deletedGos = new List<GameObject>();
            for (int i = 0; i < instance.transform.childCount; ++i)
            {
                var child = instance.transform.GetChild(i).gameObject;
                var childRefs = child.GetComponent<ReferenceCollection>();
                if (childRefs)
                {
                    if (childRefs.parentObject)
                    {
                        parentTrans.Remove(childRefs.parentObject.transform);
                        UpdateHierarchies(childRefs);
                    }
                    else
                    {
                        Debug.Log("parent removed " + child.name);
                        deletedGos.Add(child);
                    }
                }
                // else new game object
            }

            foreach (var deletedGo in deletedGos)
            {
                Undo.DestroyObjectImmediate(deletedGo);
            }

            // new game objects
            foreach (Transform prefabTrans in parentTrans)
            {
                var child = GameObject.Instantiate(prefabTrans.gameObject, refs.transform);
                LinkToParent(child.transform, prefabTrans);
                m_newGos.Add(prefabTrans.gameObject);
            }
        }

        private void AddNewComponents()
        {
            var refMap = ReferenceMap.BuildFrom(m_prefab);

            foreach (var comp in m_newComps)
            {
                var newComp = comp.targetGo.AddComponent(comp.parentComp.GetType());

                var parentSerializedObj = new SerializedObject(comp.parentComp);
                var childSerializedObj = new SerializedObject(newComp);

                UpdateProperties(childSerializedObj, parentSerializedObj, null, refMap, false);
                childSerializedObj.ApplyModifiedProperties();

                var refs = comp.targetGo.GetComponent<ReferenceCollection>();
                refs.Add(childSerializedObj.GetFileId(), parentSerializedObj.GetFileId());
            }
        }

        private void InitNewGameObjectReferences()
        {
            var newRefs = m_prefab.GetComponentsInChildren<ReferenceCollection>(true)
                                  .Where(x => m_newGos.Contains(x.parentObject));
            foreach (var newRef in newRefs)
            {
                InitReferences(newRef);
            }
        }

        private void UpdateProperties(bool recordChanges)
        {
            var refMap = ReferenceMap.BuildFrom(m_prefab);

            foreach (var refs in m_prefab.GetComponentsInChildren<ReferenceCollection>(true))
            {
                var childComps = GetComponents(refs.gameObject, true);
                var changes = refs.GetComponent<ObjectChangeCollection>();

                foreach (var parentCompKV in GetComponents(refs.parentObject, true))
                {
                    var childId = refs.GetIdByParentId(parentCompKV.Key);
                    Component childComp;
                    if (childComps.TryGetValue(childId, out childComp))
                    {
                        UpdateProperties(
                            new SerializedObject(childComp),
                            new SerializedObject(parentCompKV.Value),
                            changes.Get(childId),
                            refMap,
                            recordChanges);
                    }
                }
            }
        }

        public static Dictionary<long, Component> GetComponents(GameObject go, bool isPrefab)
        {
            if (go == null)
            {
                throw new ArgumentNullException("go");
            }

            return go.GetComponents<Component>()
                     .Where(x => !(x is IPrefabComponent))
                     .ToDictionary(x => {
                         var comp = isPrefab ? x : PrefabUtility.GetPrefabParent(x);
                         return SerializedObjectUtils.GetFileId(comp);
                     });
        }

        public static void LinkToParent(Transform child, Transform parent)
        {
            var refs = child.gameObject.AddComponent<ReferenceCollection>();
            refs.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInBuild;
            refs.parentObject = parent.gameObject;

            var changes = child.gameObject.AddComponent<ObjectChangeCollection>();
            changes.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInBuild;

            for (int i = 0; i < child.childCount; ++i)
            {
                LinkToParent(child.GetChild(i), parent.GetChild(i));
            }
        }

        public static void UpdateProperties(
            SerializedObject child, SerializedObject parent, 
            ObjectChange changes, ReferenceMap refMap, bool recordChanges)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            if (refMap == null)
            {
                throw new ArgumentNullException("refMap");
            }

            var parentProp = parent.GetIterator();
            while (parentProp.NextVisible(true))
            {
                // ignore array since we will check each element individually
                if (parentProp.hasVisibleChildren)
                {
                    continue;
                }

                var childProp = child.FindProperty(parentProp.propertyPath);
                var propChange = changes != null ? changes.Get(parentProp.propertyPath) : null;
                if (propChange != null && propChange.type != parentProp.type)
                {
                    changes.Remove(propChange);
                }
                else if (propChange == null && !Equals(childProp, parentProp, refMap))
                {
                    if (!recordChanges)
                    {
                        if (parentProp.propertyPath.EndsWith(".Array.size"))
                        {
                            // Unity will crash if CopyFromSerializedProperty is used to copy Array.size
                            childProp.intValue = parentProp.intValue;
                        }
                        else if (parentProp.propertyType != SerializedPropertyType.ObjectReference)
                        {
                            childProp.serializedObject.CopyFromSerializedProperty(parentProp);
                        }
                        else
                        {
                            if (parentProp.objectReferenceValue != null)
                            {
                                var localRef = refMap.GetObjectByParentId(SerializedObjectUtils.GetFileId(parentProp.objectReferenceValue));
                                childProp.objectReferenceValue = localRef ?? parentProp.objectReferenceValue;
                            }
                            else
                            {
                                childProp.objectReferenceValue = null;
                            }
                        }
                    }
                    else if (changes != null)
                    {
                        changes.Add(new PropertyChange(parentProp.propertyPath, parentProp.type));
                    }
                }
            }
            child.ApplyModifiedProperties();
        }

        public static bool Equals(SerializedProperty child, SerializedProperty parent, ReferenceMap refReg)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            if (refReg == null)
            {
                throw new ArgumentNullException("refReg");
            }

            if (child.propertyType != parent.propertyType || child.type != parent.type)
            {
                return false;
            }

            switch (child.propertyType)
            {
            case SerializedPropertyType.AnimationCurve: return child.animationCurveValue == parent.animationCurveValue;
            case SerializedPropertyType.Character:
            case SerializedPropertyType.Integer:
            case SerializedPropertyType.LayerMask: 
            case SerializedPropertyType.ArraySize: return child.intValue == parent.intValue;
            case SerializedPropertyType.Boolean: return child.boolValue == parent.boolValue;
            case SerializedPropertyType.Bounds: return child.boundsValue == parent.boundsValue;
            case SerializedPropertyType.Color: return child.colorValue == parent.colorValue;
            case SerializedPropertyType.Enum: return child.enumValueIndex == parent.enumValueIndex;
            case SerializedPropertyType.ExposedReference: return child.exposedReferenceValue == parent.exposedReferenceValue;
            case SerializedPropertyType.Float: return child.floatValue == parent.floatValue;
            case SerializedPropertyType.ObjectReference:
                if (child.objectReferenceValue == parent.objectReferenceValue)
                {
                    return true;
                }
                if (child.objectReferenceValue == null || parent.objectReferenceValue == null)
                {
                    return false;
                }
                var localRef = refReg.GetObjectByParentId(SerializedObjectUtils.GetFileId(parent.objectReferenceValue));
                return localRef == child.objectReferenceValue;

            case SerializedPropertyType.Quaternion: return child.quaternionValue == parent.quaternionValue;
            case SerializedPropertyType.Rect: return child.rectValue == parent.rectValue;
            case SerializedPropertyType.String: return child.stringValue == parent.stringValue;
            case SerializedPropertyType.Vector2: return child.vector2Value == parent.vector2Value;
            case SerializedPropertyType.Vector3: return child.vector3Value == parent.vector3Value;
            case SerializedPropertyType.Vector4: return child.vector4Value == parent.vector4Value;
            }

            return true;
        }

        public static void InitReferences(ReferenceCollection refs)
        {
            if (refs == null)
            {
                throw new ArgumentNullException("refs");
            }

            int i = 0;
            var parentComps = refs.parentObject.GetComponents<Component>();
            foreach (var comp in refs.GetComponents<Component>())
            {
                if (comp is IPrefabComponent)
                {
                    continue;
                }

                var fileId = SerializedObjectUtils.GetFileId(comp);
                var parentFileId = SerializedObjectUtils.GetFileId(parentComps[i++]);
                refs.Add(fileId, parentFileId);
            }
        }

    }
}
