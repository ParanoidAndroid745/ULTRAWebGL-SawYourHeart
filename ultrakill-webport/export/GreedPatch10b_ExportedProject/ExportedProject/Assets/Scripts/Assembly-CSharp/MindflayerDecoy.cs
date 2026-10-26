using UnityEngine;

public class MindflayerDecoy : MonoBehaviour
{
	private SkinnedMeshRenderer smr;

	private Material[] mats;

	private Color clr;

	public bool enraged;

	public Material enrageMaterial;

	public float fadeOverride;

	private void Start()
	{
		smr = GetComponentInChildren<SkinnedMeshRenderer>();
		if (enraged)
		{
			smr.material = enrageMaterial;
		}
		mats = smr.materials;
		clr = mats[0].color;
		if (fadeOverride != 0f)
		{
			clr.a = fadeOverride;
			Material[] array = mats;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = clr;
			}
		}
	}

	private void Update()
	{
		if (!(clr.a > 0f))
		{
			return;
		}
		clr.a = Mathf.MoveTowards(clr.a, 0f, Time.deltaTime);
		if (clr.a <= 0f)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Material[] array = mats;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = clr;
		}
	}
}
