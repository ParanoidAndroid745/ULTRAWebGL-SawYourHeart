using UnityEngine;

public class ChangeMaterials : MonoBehaviour
{
	public Material[] materials;

	private SkinnedMeshRenderer smr;

	private MeshRenderer mr;

	public void Activate()
	{
		smr = GetComponentInParent<SkinnedMeshRenderer>();
		if ((bool)smr)
		{
			smr.materials = materials;
		}
		mr = GetComponentInParent<MeshRenderer>();
		if ((bool)mr)
		{
			mr.materials = materials;
		}
		Object.Destroy(this);
	}
}
