using UnityEngine;

public class AnimatedTexture : MonoBehaviour
{
	[SerializeField]
	private float delay;

	[SerializeField]
	private Texture2D[] framePool;

	private TimeSince counter;

	private int selector;

	private MaterialPropertyBlock block;

	private Renderer renderer;

	private static readonly int MainTexID = Shader.PropertyToID("_MainTex");

	private void Awake()
	{
		block = new MaterialPropertyBlock();
		renderer = GetComponent<Renderer>();
		renderer.GetPropertyBlock(block, 0);
		counter = 0f;
	}

	private void Update()
	{
		if ((float)counter > delay)
		{
			if (selector >= framePool.Length)
			{
				selector = 0;
			}
			block.SetTexture(MainTexID, framePool[selector]);
			renderer.SetPropertyBlock(block, 0);
			selector++;
			counter = 0f;
		}
	}
}
