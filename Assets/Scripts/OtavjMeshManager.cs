using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Object = UnityEngine.Object;

namespace ota.ndi
{
    public class OtavjMeshManager : MonoBehaviour
    {
        public ARMeshManager m_MeshManager;
        public MeshFilter m_BackgroundMeshPrefab;
        [HideInInspector]
        public readonly Dictionary<string, MeshFilter> m_MeshMap = new Dictionary<string, MeshFilter>();

        MeshFilter m_meshFilter;
        Action<MeshFilter> m_BreakupMeshAction;
        Action<MeshFilter> m_UpdateMeshAction;
        Action<MeshFilter> m_RemoveMeshAction;
        //readonly Dictionary<TrackableId, MeshFilter> m_MeshMap = new Dictionary<TrackableId, MeshFilter>();

        void Awake()
        {
            m_meshFilter = GetComponent<MeshFilter>();
            m_BreakupMeshAction = new Action<MeshFilter>(BreakupMesh);
            m_UpdateMeshAction = new Action<MeshFilter>(UpdateMesh);
            m_RemoveMeshAction = new Action<MeshFilter>(RemoveMesh);
        }

        void OnEnable()
        {
            m_MeshManager.meshesChanged += OnMeshesChanged;
        }

        void OnDisable()
        {
            m_MeshManager.meshesChanged -= OnMeshesChanged;
        }

        void OnMeshesChanged(ARMeshesChangedEventArgs args)
        {
            if (args.added != null)
            {
                args.added.ForEach(m_BreakupMeshAction);
            }

            if (args.updated != null)
            {
                args.updated.ForEach(m_UpdateMeshAction);
            }

            if (args.removed != null)
            {
                args.removed.ForEach(m_RemoveMeshAction);
            }
        }

        void BreakupMesh(MeshFilter meshFilter)
        {
            var vertices = meshFilter.mesh.vertices;
            var indices = meshFilter.mesh.triangles;

            var parent = meshFilter.transform.parent;
            var bgmeshfilter = Instantiate(m_BackgroundMeshPrefab, parent);
            bgmeshfilter.mesh = meshFilter.mesh;

            bgmeshfilter.mesh.SetIndices(bgmeshfilter.mesh.GetIndices(0), MeshTopology.Lines, 0);

            var meshId = ExtractTrackableId(meshFilter.name);
            m_MeshMap[meshId] = bgmeshfilter;

        }

        void UpdateMesh(MeshFilter meshFilter)
        {
            var vertices = meshFilter.mesh.vertices;
            var indices = meshFilter.mesh.triangles;

            var meshId = ExtractTrackableId(meshFilter.name);
            var bgmeshfilter = m_MeshMap[meshId];
            bgmeshfilter.mesh.Clear();
            bgmeshfilter.mesh = meshFilter.mesh;

            bgmeshfilter.mesh.SetIndices(bgmeshfilter.mesh.GetIndices(0), MeshTopology.Lines, 0);
        }

        void RemoveMesh(MeshFilter meshFilter)
        {
            var meshId = ExtractTrackableId(meshFilter.name);
            var bgmeshfilter = m_MeshMap[meshId];
            Object.Destroy(bgmeshfilter);
            m_MeshMap.Remove(meshId);
        }

        string ExtractTrackableId(string meshFilterName)
        {
            string[] nameSplit = meshFilterName.Split(' ');
            //return new TrackableId(nameSplit[1]);
            return nameSplit[1];
        }
    }
}

