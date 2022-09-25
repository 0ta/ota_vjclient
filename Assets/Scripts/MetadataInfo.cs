using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ota.ndi
{
    public class MetadataInfo
    {
        public string arcameraPosition { get; set; }
        public string arcameraRotation { get; set; }
        public string projectionMatrix { get; set; }

        public MetadataInfo(Vector3 arcameraPosition, Quaternion arcameraRotation, Matrix4x4 projectionMatrix)
        {
            this.arcameraPosition = arcameraPosition.ToString("F2");
            this.arcameraRotation = arcameraRotation.ToString("F5");
            this.projectionMatrix = projectionMatrix.ToString("F5");
        }

        public Vector3 getArcameraPosition()
        {
            if (arcameraPosition == null) throw new Exception("AR Camera position is null.");
            return createVector3(this.arcameraPosition);
        }

        public Quaternion getArcameraRotation()
        {
            if (arcameraPosition == null) throw new Exception("AR Camera rotaion is null.");
            return createRotation(this.arcameraRotation);
        }

        //
        //注意！！！！！！
        //このMethodはBug含む
        //後で修正必要！！！！！
        //
        public Matrix4x4 getProjectionMatrix()
        {
            if (arcameraPosition == null) throw new Exception("Projection Matrix is null.");
            return createMatrix4x4(this.projectionMatrix);
        }

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
            for (int i = 0; i < 15; i++)
            {
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
    }
}