using System.Collections.Generic;
using NewBlood.Rendering;
using UnityEngine;

public static class ColliderUtility
{
	private static readonly List<Vector3> s_Vertices = new List<Vector3>();

	private static readonly List<int> s_Triangles = new List<int>();

	private static Triangle<Vector3> GetWorldTriangle(Transform collider, int index)
	{
		Vector3 position = s_Vertices[s_Triangles[3 * index]];
		Vector3 position2 = s_Vertices[s_Triangles[3 * index + 1]];
		Vector3 position3 = s_Vertices[s_Triangles[3 * index + 2]];
		return new Triangle<Vector3>(collider.TransformPoint(position), collider.TransformPoint(position2), collider.TransformPoint(position3));
	}

	private static Plane GetWorldTrianglePlane(Transform collider, int index)
	{
		return GetWorldTrianglePlane(GetWorldTriangle(collider, index));
	}

	private static Plane GetWorldTrianglePlane(Triangle<Vector3> source)
	{
		return new Plane(source.Index0, source.Index1, source.Index2);
	}

	private static bool InTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
	{
		Vector3 vector = b - a;
		Vector3 vector2 = c - a;
		float num = (vector2.x * (a.y - p.y) + vector2.y * (p.x - a.x)) / (vector.x * vector2.y - vector.y * vector2.x);
		float num2 = (p.y - a.y - num * vector.y) / vector2.y;
		if ((double)num >= 0.0 && (double)num2 >= 0.0)
		{
			return (double)(num + num2) <= 1.0;
		}
		return false;
	}

	public static Vector3 FindClosestPoint(Collider collider, Vector3 position)
	{
		return FindClosestPoint(collider, position, ignoreVerticalTriangles: false);
	}

	public static Vector3 FindClosestPoint(Collider collider, Vector3 position, bool ignoreVerticalTriangles)
	{
		if (collider is MeshCollider meshCollider && !meshCollider.convex)
		{
			Mesh sharedMesh = meshCollider.sharedMesh;
			sharedMesh.GetVertices(s_Vertices);
			Vector3 result = Vector3.zero;
			float num = float.PositiveInfinity;
			for (int i = 0; i < sharedMesh.subMeshCount; i++)
			{
				sharedMesh.GetTriangles(s_Triangles, i);
				int j = 0;
				for (int num2 = s_Triangles.Count / 3; j < num2; j++)
				{
					Triangle<Vector3> worldTriangle = GetWorldTriangle(meshCollider.transform, j);
					Plane worldTrianglePlane = GetWorldTrianglePlane(worldTriangle);
					Vector3 vector = worldTrianglePlane.ClosestPointOnPlane(position);
					float num3 = Mathf.Abs(worldTrianglePlane.GetDistanceToPoint(position));
					if ((!ignoreVerticalTriangles || (!(worldTrianglePlane.normal == Vector3.up) && !(worldTrianglePlane.normal == Vector3.down))) && InTriangle(worldTriangle.Index0, worldTriangle.Index1, worldTriangle.Index2, vector) && ((i == 0 && j == 0) || num3 < num))
					{
						result = vector;
						num = num3;
					}
				}
			}
			return result;
		}
		return collider.ClosestPoint(position);
	}
}
