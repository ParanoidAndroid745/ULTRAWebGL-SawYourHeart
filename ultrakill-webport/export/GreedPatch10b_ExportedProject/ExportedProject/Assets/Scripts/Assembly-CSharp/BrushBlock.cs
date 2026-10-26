using UnityEngine;

public class BrushBlock : MonoBehaviour
{
	public Vector3 DataSize;

	public BlockType Type;

	public BoxCollider OverrideCollider;

	public SavedBlock Save()
	{
		Transform transform = base.transform;
		return new SavedBlock
		{
			ObjectIdentifier = GetComponent<SandboxProp>().sourceObject.identifier,
			Position = transform.position,
			Rotation = transform.rotation,
			BlockSize = DataSize,
			BlockType = Type,
			Kinematic = GetComponent<Rigidbody>().isKinematic
		};
	}
}
