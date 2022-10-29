using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgMetadataManager
{
    //public Dictionary<string, MeshFilter> SentMeshMap { get; set; }
    public Dictionary<string, (Vector3[], int[])> SentMeshMap { get; set; }

    public void InitializeManager(Dictionary<string, MeshFilter> CurrentMeshMap)
    {
        SentMeshMap = new Dictionary<string, (Vector3[], int[])>();
        var currentIt = CurrentMeshMap.GetEnumerator();
        while (currentIt.MoveNext())
        {
            KeyValuePair<string, MeshFilter> currentPair = currentIt.Current;
            SentMeshMap.Add(currentPair.Key, (currentPair.Value.mesh.vertices, currentPair.Value.mesh.GetIndices(0)));
        }
    }

    public void Clear()
    {
        SentMeshMap = null;
    }

    public Dictionary<string, (Vector3[], int[])> getDeltaVerticesInfo(Dictionary<string, MeshFilter> CurrentMeshMap)
    {
        Dictionary<string, (Vector3[], int[])> ret = new Dictionary<string, (Vector3[], int[])>();

        var currentIt = CurrentMeshMap.GetEnumerator();
        while (currentIt.MoveNext())
        {
            KeyValuePair<string, MeshFilter> currentPair = currentIt.Current;
            bool hasAlreadySent = false;
            var sentIt = SentMeshMap.GetEnumerator();
            while (sentIt.MoveNext())
            {
                KeyValuePair<string, (Vector3[], int[])> sentPair = sentIt.Current;
                if (currentPair.Key.Equals(sentPair.Key))
                {
                    hasAlreadySent = true;
                    if (currentPair.Value.mesh.vertices.Length != sentPair.Value.Item1.Length)
                    {
                        // 今回のフレームでUpdateされたMesh
                        var mesh = currentPair.Value.mesh;
                        ret.Add(currentPair.Key, (mesh.vertices, mesh.GetIndices(0)));

                        //SentMeshMap[currentPair.Key] = (currentPair.Value.mesh.vertices, currentPair.Value.mesh.GetIndices(0));
                    } else
                    {
                        // 今回のフレームで変更がないMeshは送信しない
                    }
                } 
            }
            if (!hasAlreadySent)
            {
                // 今回のフレームで新しく生成されたMesh
                var mesh = currentPair.Value.mesh;
                ret.Add(currentPair.Key, (mesh.vertices, mesh.GetIndices(0)));

                //SentMeshMap.Add(currentPair.Key, (mesh.vertices, mesh.GetIndices(0)));
            } else
            {
                //Debug.Log("何もしない:" + currentPair.Key);
            }
        }
        var tmpIt = ret.GetEnumerator();
        while (tmpIt.MoveNext())
        {
            KeyValuePair<string, (Vector3[], int[])> tmpPair = tmpIt.Current;
            if (SentMeshMap.ContainsKey(tmpPair.Key))
            {
                SentMeshMap[tmpPair.Key] = tmpPair.Value;
            } else
            {
                SentMeshMap.Add(tmpPair.Key, tmpPair.Value);
            }
        }
        return ret;
    }

    public List<string> getDeletedMeshList(Dictionary<string, MeshFilter> CurrentMeshMap)
    {
        List<string> ret = new List<string>();
        var sentIt = SentMeshMap.GetEnumerator();
        while (sentIt.MoveNext())
        {
            KeyValuePair<string, (Vector3[], int[])> sentPair = sentIt.Current;
            bool isDeleted = true;
            var currentIt = CurrentMeshMap.GetEnumerator();
            while (currentIt.MoveNext())
            {
                KeyValuePair<string, MeshFilter> currentPair = currentIt.Current;
                if (currentPair.Key.Equals(sentPair.Key))
                {
                    isDeleted = false;
                }
            }
            if (isDeleted)
            {
                // 今回のフレームで削除されたMesh
                ret.Add(sentPair.Key);
                //SentMeshMap.Remove(sentPair.Key);
            }
        }
        var tmpIt = ret.GetEnumerator();
        while (tmpIt.MoveNext())
        {
            var deleteId = tmpIt.Current;
            SentMeshMap.Remove(deleteId);
        }
        return ret;
    }

}
