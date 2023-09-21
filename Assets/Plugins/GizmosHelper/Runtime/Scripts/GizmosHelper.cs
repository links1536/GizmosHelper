using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Links
{
	public static class GizmosHelper
	{
		//毎回線を作ってると負荷かかるので、各描画命令の初回呼び出しでMeshをキャッシュする
		static Mesh m_LineMesh;
		static Mesh m_ArrowHeadMesh;
		static Mesh m_WireDiscMesh;
		static Mesh m_WireHalfDiscMesh;

		static Mesh CreateWireMesh(Vector3[] vertices, int[] indices, MeshTopology topology)
		{
			Color32[] colors = new Color32[vertices.Length];
			for (int i = 0; i < colors.Length; i++) {
				colors[i] = Color.white;
			}

			Vector3[] normals = new Vector3[vertices.Length];
			for (int i = 0; i < colors.Length; i++) {
				normals[i] = Vector3.forward;
			}

			Mesh mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontUnloadUnusedAsset;
			mesh.subMeshCount = 1;
			mesh.SetVertices(vertices);
			mesh.SetColors(colors);
			mesh.SetNormals(normals);
			mesh.SetIndices(indices, 0, indices.Length, topology, 0, true, 0);
			mesh.RecalculateBounds();
			mesh.Optimize();
			mesh.UploadMeshData(true);

			return mesh;
		}

		#region DrawLine

		public static void DrawLine(Vector3 from, Vector3 to)
		{
			if(m_LineMesh == null) {
				Vector3[] vertices = new Vector3[]
				{
					new Vector3(0, 0, +0.5f),
					new Vector3(0, 0, -0.5f),
				};
				int[] indices = new int[] { 0,1 };
				m_LineMesh = CreateWireMesh(vertices, indices, MeshTopology.Lines);
			}
			Vector3 vector = to - from;
			Gizmos.DrawWireMesh(m_LineMesh, Vector3.Lerp(from, to, 0.5f), Quaternion.LookRotation(vector), Vector3.one * vector.magnitude);
		}

		public static void DrawLines(Vector3[] lineSegments)
		{
			if(lineSegments.Length % 2 != 0) {
				Debug.LogError("lineSegments count is not even number");
				return;
			}

			for (int i = 0; i < lineSegments.Length; i += 2)
				DrawLine(lineSegments[i + 0], lineSegments[i + 1]);
		}

		public static void DrawLines(Vector3[] points, int[] indicies)
		{
			if (indicies.Length % 2 != 0) {
				Debug.LogError("indicies count is not even number");
				return;
			}

			for (int i = 0; i < indicies.Length; i += 2)
				DrawLine(points[indicies[i + 0]], points[indicies[i + 1]]);
		}

		#endregion

		#region DrawArrow

		static void DrawArrowHeadInternal(Vector3 position, Quaternion rotation, float size)
		{
			if (m_ArrowHeadMesh == null) {
				const int divCount = 6;
				Vector3[] vertices = new Vector3[divCount + 1];
				for (int i = 0; i < divCount; i++) {
					float time = Mathf.InverseLerp(0, divCount, i);
					vertices[i] = -(Quaternion.Euler(0, 0, 360.0f * time) * (Quaternion.Euler(15, 0, 0) * Vector3.forward));
				}
				vertices[vertices.Length - 1] = Vector3.zero;

				int[] indices = new int[divCount * 2 + divCount * 2];
				int index = 0;
				for (int i = 0; i < divCount; i++) {
					indices[index++] = vertices.Length - 1;
					indices[index++] = i;
				}
				for (int i = 0; i < divCount; i++) {
					indices[index++] = (i + 0) % divCount;
					indices[index++] = (i + 1) % divCount;
				}

				m_ArrowHeadMesh = CreateWireMesh(vertices, indices, MeshTopology.Lines);
			}
			Gizmos.DrawWireMesh(m_ArrowHeadMesh, position, rotation, Vector3.one * size);
		}

		static void DrawArrowInternal(Vector3 position, Quaternion rotation, float length, float size)
		{
			Vector3 half = Vector3.forward * length * 0.5f;
			Vector3 forward = position + rotation * half;
			Vector3 back = position + rotation * -half;

			DrawArrowHeadInternal(forward, rotation, size);
			DrawLine(forward, back);
		}

		public static void DrawArrow(Vector3 position, Vector3 directon, float length, float size)
			=> DrawArrowInternal(position, Quaternion.LookRotation(directon), length, size);

		public static void DrawArrow(Vector3 position, Quaternion rotation, float length, float size)
			=> DrawArrowInternal(position, rotation, length, size);

		#endregion

		#region DrawWireSphere

		static void DrawWireDiscInternal(Vector3 position, Quaternion rotation, float radius)
		{
			if (m_WireDiscMesh == null) {
				const int divCount = 360;
				Vector3[] vertices = new Vector3[divCount];
				for (int i = 0; i < divCount; i++) {
					float time = Mathf.InverseLerp(0, divCount, i);
					float angle = Mathf.Lerp(0, 360, time) * Mathf.Deg2Rad;
					vertices[i] = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);
				}

				int[] indices = new int[vertices.Length + 1];
				for (int i = 0; i < indices.Length; i++) {
					indices[i] = i % vertices.Length;
				}

				m_WireDiscMesh = CreateWireMesh(vertices, indices, MeshTopology.LineStrip);
			}
			Gizmos.DrawWireMesh(m_WireDiscMesh, position, rotation, Vector3.one * radius);
		}

		public static void DrawWireSphere(Vector3 position, float radius)
		{
			DrawWireDiscInternal(position, Quaternion.Euler(90,  0,  0), radius);
			DrawWireDiscInternal(position, Quaternion.Euler( 0, 90,  0), radius);
			DrawWireDiscInternal(position, Quaternion.Euler( 0,  0,  0), radius);

			//3つだけだと角度次第では実際のサイズより小さく見えてしまうので、補うためにカメラの方を向く円を描く
			var camera = Camera.current;
			var vector = position - camera.transform.position;
			var lookat = Quaternion.LookRotation(-vector, camera.transform.up);
			DrawWireDiscInternal(position, lookat, radius);
		}

		#endregion

		#region DrawWriteCapsule
		static void DrawWireHalfDiscInternal(Vector3 position, Quaternion rotation, float radius)
		{
			if (m_WireHalfDiscMesh == null) {
				const int divCount = 180;
				Vector3[] vertices = new Vector3[divCount];
				for (int i = 0; i < divCount; i++) {
					float time = Mathf.InverseLerp(0, divCount - 1, i);
					float angle = Mathf.Lerp(-90, +90, time) * Mathf.Deg2Rad;
					vertices[i] = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);
				}

				int[] indices = new int[vertices.Length];
				for (int i = 0; i < indices.Length; i++) {
					indices[i] = i;
				}

				m_WireHalfDiscMesh = CreateWireMesh(vertices, indices, MeshTopology.LineStrip);
			}
			Gizmos.DrawWireMesh(m_WireHalfDiscMesh, position, rotation, Vector3.one * radius);
		}

		static void DrawWireCapsuleInternal(Vector3 position, Quaternion rotation, float radius, float height)
		{
			float halfHeight = height / 2;
			DrawWireHalfDiscInternal(position + rotation * Vector3.up * halfHeight, rotation, radius);
			DrawWireHalfDiscInternal(position + rotation * Vector3.up * halfHeight, rotation * Quaternion.Euler(0, 90, 0), radius);
			DrawWireHalfDiscInternal(position - rotation * Vector3.up * halfHeight, rotation * Quaternion.Euler(180, 0, 0), radius);
			DrawWireHalfDiscInternal(position - rotation * Vector3.up * halfHeight, rotation * Quaternion.Euler(180, 90, 0), radius);

			DrawLine(position + rotation * new Vector3(0, +halfHeight, +radius), position + rotation * new Vector3(0, -halfHeight, +radius));
			DrawLine(position + rotation * new Vector3(+radius, +halfHeight, 0), position + rotation * new Vector3(+radius, -halfHeight, 0));
			DrawLine(position + rotation * new Vector3(0, +halfHeight, -radius), position + rotation * new Vector3(0, -halfHeight, -radius));
			DrawLine(position + rotation * new Vector3(-radius, +halfHeight, 0), position + rotation * new Vector3(-radius, -halfHeight, 0));

			DrawWireDiscInternal(position + rotation * Vector3.up * halfHeight, rotation * Quaternion.Euler(90, 0, 0), radius);
			DrawWireDiscInternal(position - rotation * Vector3.up * halfHeight, rotation * Quaternion.Euler(90, 0, 0), radius);
		}

		public static void DrawWireCapsule(Vector3 position, Quaternion rotation, float radius, float height)
			=> DrawWireCapsuleInternal(position, rotation, radius, height);

		#endregion
	}
}
