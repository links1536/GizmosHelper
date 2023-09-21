using System.Collections;
using System.Collections.Generic;
//using Links.Attributes;
using UnityEngine;

namespace Links
{
    public class GizmosCapsule : MonoBehaviour
    {
        /*[Slider(0, 5)]*/ public float Length = 3;
        /*[Slider(0, 5)]*/ public float Size = 1;

		private void OnDrawGizmos()
		{
			//GizmosHelper.DrawArrow(transform.position, transform.rotation, Length, Size);
			//GizmosHelper.DrawWireSphere(transform.position, transform.lossyScale.magnitude);
			GizmosHelper.DrawWireCapsule(transform.position, transform.rotation, Size, Length);
		}
	}
}
