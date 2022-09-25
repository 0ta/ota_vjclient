using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("*" + _camera.transform.position);
        //Debug.Log(createVector3(_camera.transform.position.ToString("F2")));

        //Debug.Log("**" + _camera.transform.rotation);
        //Debug.Log(createRotation(_camera.transform.rotation.ToString("F5")));
        var tmpmat = _camera.projectionMatrix;
        Debug.Log("**" + tmpmat.ToString("F5"));
        Debug.Log(createMatrix4x4(tmpmat.ToString("F5")));
    }

    //void showMat(Matrix4x4 mat)
    //{

    //}

    Vector3 createVector3(string str)
    {
        var farray = convertStr2FloatArray(str);
        return new Vector3(farray[0], farray[1], farray[2]);
    }

    Quaternion createRotation(string str)
    {
        var farray = convertStr2FloatArray(str);
        return new Quaternion(farray[0], farray[1], farray[2], farray[3]);
    }

    Matrix4x4 createMatrix4x4(string str)
    {
        var farray = convertStr2FloatArray(str);
        var mat = Matrix4x4.identity;
        for (int i = 0; i < 15; i++) {
            mat[i] = farray[i];
        }
        return mat;
    }

    float[] convertStr2FloatArray(string str)
    {
        var matchs = Regex.Matches(str, "-?[0-9]+\\.[0-9]+");
        var ret = new float[matchs.Count + 1];
        for (int i = 0; i < matchs.Count; i++)
        {
            ret[i] = float.Parse(matchs[i].Value);
        }
        return ret;
    }

    float[] oldcreateVector3(string str)
    {
        var matchs = Regex.Matches(str, "-?[0-9]+\\.[0-9][0-9]");
        Debug.Log("Count:" + matchs.Count);
        for (int i = 0; i < matchs.Count; i++)
        {
            Debug.Log(matchs[i]);
        }
        return new float[] { float.Parse(matchs[0].Value), float.Parse(matchs[1].Value), float.Parse(matchs[2].Value) };
    }

}
