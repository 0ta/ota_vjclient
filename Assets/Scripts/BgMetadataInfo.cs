using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ota.ndi {
    public class BgMetadataInfo
    {
        //public List<(float, float, float)[]> verticesList { get; set; }
        public List<float[]> verticesList { get; set; }
        public List<int[]> trianglesList { get; set; }
        public List<string> meshKeyList { get; set; }
        public List<string> deletedMeshKeyList { get; set; }

        public BgMetadataInfo() { }

        public BgMetadataInfo(Dictionary<string, (Vector3[], int[])> meshInfoMap, List<string> deletedMesh)
        {
            verticesList = new List<float[]>();
            trianglesList = new List<int[]>();
            meshKeyList = new List<string>();
            deletedMeshKeyList = deletedMesh;
            var it = meshInfoMap.GetEnumerator();
            while (it.MoveNext())
            {
                KeyValuePair<string, (Vector3[], int[])> meshInfoPair = it.Current;
                meshKeyList.Add(meshInfoPair.Key);
                var vericeVec3 = meshInfoPair.Value.Item1;
                verticesList.Add(getVerticeListFloatArray(vericeVec3));
                trianglesList.Add(meshInfoPair.Value.Item2);
            }
        }

        private float[] getVerticeListFloatArray(Vector3[] vec3array)
        {
            float[] ret = new float[vec3array.Length * 3];
            var it = vec3array.GetEnumerator();
            int i = 0;
            while (it.MoveNext())
            {
                Vector3 vec3 = (Vector3)it.Current;
                ret[i] = vec3.x;
                ret[i + 1] = vec3.y;
                ret[i + 2] = vec3.z;
                i++;
            }
            return ret;
        }

        private (float, float, float)[] getVerticeListFloatTuple(Vector3[] vec3array) {
            (float, float, float)[] ret = new (float, float, float)[vec3array.Length];
            var it = vec3array.GetEnumerator();
            int i = 0;
            while (it.MoveNext()) {
                Vector3 vec3 = (Vector3)it.Current;
                ret[i] = (vec3.x, vec3.y, vec3.z);
                i++;
            }
            return ret;
        }

    }
}

