using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catan
{
    public class BoatLogic : MonoBehaviour
    {
        float speed = 0.25f;


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Vector3 movement = new Vector3(0, -1, 0);
            transform.Translate(movement * speed * Time.deltaTime);

            Vector3 rotation = new Vector3(0, 0, 0.001f);
            transform.Rotate(rotation);
        }
    }
}
