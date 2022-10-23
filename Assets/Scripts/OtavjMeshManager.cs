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

        MeshFilter m_meshFilter;
        Action<MeshFilter> m_BreakupMeshAction;
        Action<MeshFilter> m_UpdateMeshAction;
        Action<MeshFilter> m_RemoveMeshAction;
        //readonly Dictionary<TrackableId, MeshFilter> m_MeshMap = new Dictionary<TrackableId, MeshFilter>();
        readonly Dictionary<string, MeshFilter> m_MeshMap = new Dictionary<string, MeshFilter>();

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
            Debug.Log("onMeshsChangeds");
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
            Debug.Log("BreakupMesh");
            var vertices = meshFilter.mesh.vertices;
            var normals = meshFilter.mesh.normals;
            var indices = meshFilter.mesh.triangles;
            Debug.Log(meshFilter.name);
            Debug.Log("length vertices:" + meshFilter.mesh.vertices.Length);
            Debug.Log("length traiangles:" + meshFilter.mesh.triangles.Length);
            Debug.Log("vertices:" + vertices.ToString());




            var parent = meshFilter.transform.parent;
            var bgmeshfilter = Instantiate(m_BackgroundMeshPrefab, parent);
            bgmeshfilter.mesh = meshFilter.mesh;

            bgmeshfilter.mesh.SetIndices(bgmeshfilter.mesh.GetIndices(0), MeshTopology.Lines, 0);

            var meshId = ExtractTrackableId(meshFilter.name);
            m_MeshMap[meshId] = bgmeshfilter;
            Debug.Log(meshFilter.name);

            //XRMeshSubsystem meshSubsystem = m_MeshManager.subsystem as XRMeshSubsystem;
            //if (meshSubsystem == null)
            //{
            //    return;
            //}

            //var meshId = ExtractTrackableId(meshFilter.name);
            //var faceClassifications = meshSubsystem.GetFaceClassifications(meshId, Allocator.Persistent);

            //if (!faceClassifications.IsCreated)
            //{
            //    return;
            //}

            //using (faceClassifications)
            //{
            //    if (faceClassifications.Length <= 0)
            //    {
            //        return;
            //    }

            //    var parent = meshFilter.transform.parent;

            //    MeshFilter[] meshFilters = new MeshFilter[k_NumClassifications];

            //    meshFilters[(int)ARMeshClassification.None] = (m_NoneMeshPrefab == null) ? null : Instantiate(m_NoneMeshPrefab, parent);
            //    meshFilters[(int)ARMeshClassification.Wall] = (m_WallMeshPrefab == null) ? null : Instantiate(m_WallMeshPrefab, parent);
            //    meshFilters[(int)ARMeshClassification.Floor] = (m_FloorMeshPrefab == null) ? null : Instantiate(m_FloorMeshPrefab, parent);
            //    meshFilters[(int)ARMeshClassification.Ceiling] = (m_CeilingMeshPrefab == null) ? null : Instantiate(m_CeilingMeshPrefab, parent);
            //    meshFilters[(int)ARMeshClassification.Table] = (m_TableMeshPrefab == null) ? null : Instantiate(m_TableMeshPrefab, parent);
            //    meshFilters[(int)ARMeshClassification.Seat] = (m_SeatMeshPrefab == null) ? null : Instantiate(m_SeatMeshPrefab, parent);
            //    meshFilters[(int)ARMeshClassification.Window] = (m_WindowMeshPrefab == null) ? null : Instantiate(m_WindowMeshPrefab, parent);
            //    meshFilters[(int)ARMeshClassification.Door] = (m_DoorMeshPrefab == null) ? null : Instantiate(m_DoorMeshPrefab, parent);

            //    m_MeshFrackingMap[meshId] = meshFilters;

            //    var baseMesh = meshFilter.sharedMesh;
            //    for (int i = 0; i < k_NumClassifications; ++i)
            //    {
            //        var classifiedMeshFilter = meshFilters[i];
            //        if (classifiedMeshFilter != null)
            //        {
            //            var classifiedMesh = classifiedMeshFilter.mesh;
            //            ExtractClassifiedMesh(baseMesh, faceClassifications, (ARMeshClassification)i, classifiedMesh);
            //            meshFilters[i].mesh = classifiedMesh;
            //        }
            //    }
            //}
        }

        void UpdateMesh(MeshFilter meshFilter)
        {
            Debug.Log("Update!!!!!");
            var vertices = meshFilter.mesh.vertices;
            var normals = meshFilter.mesh.normals;
            var indices = meshFilter.mesh.triangles;
            Debug.Log(meshFilter.name);
            Debug.Log("length vertices:" + meshFilter.mesh.vertices.Length);
            Debug.Log("length traiangles:" + meshFilter.mesh.triangles.Length);
            Debug.Log("vertices:" + vertices.ToString());


            var meshId = ExtractTrackableId(meshFilter.name);
            var bgmeshfilter = m_MeshMap[meshId];
            bgmeshfilter.mesh.Clear();
            bgmeshfilter.mesh = meshFilter.mesh;

            bgmeshfilter.mesh.SetIndices(bgmeshfilter.mesh.GetIndices(0), MeshTopology.Lines, 0);
        }

        void RemoveMesh(MeshFilter meshFilter)
        {
            Debug.Log("Delete!!!!!!!");
            Debug.Log(meshFilter.name);


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

