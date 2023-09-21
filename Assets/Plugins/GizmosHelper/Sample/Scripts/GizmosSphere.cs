using System.Collections;
using System.Collections.Generic;
//using Links.Attributes;
using UnityEngine;

namespace Links
{
    public class GizmosSphere : MonoBehaviour
    {
        /*[Slider(0, 5)]*/ public float Radius = 1;

		private void OnDrawGizmos()
		{
			GizmosHelper.DrawWireSphere(transform.position, Radius);
		}
	}
}
