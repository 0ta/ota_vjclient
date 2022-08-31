using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ota.ndi
{
    public class TmpCubeMove : MonoBehaviour
    {
        bool isUp = true;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (isUp && transform.position.y < 3f)
            {
                transform.Translate(0f, 0.1f, 0f);
            } else if (transform.position.y >= 3f)
            {
                isUp = false;
                transform.Translate(0f, -0.1f, 0f);
            } else if (!isUp && transform.position.y >= -1f)
            {
                transform.Translate(0f, -0.1f, 0f);
            } else
            {
                isUp = true;
                transform.Translate(0f, 0.1f, 0f);
            }

        }
    }
}

