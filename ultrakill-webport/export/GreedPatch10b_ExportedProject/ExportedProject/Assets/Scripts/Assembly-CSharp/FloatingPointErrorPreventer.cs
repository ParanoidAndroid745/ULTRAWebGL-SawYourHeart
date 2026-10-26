using UnityEngine;

public class FloatingPointErrorPreventer : MonoBehaviour
{
	private bool deactivated;

	private Vector3 startPos;

	private void Start()
	{
		startPos = base.transform.position;
		SlowCheck();
	}

	private void SlowCheck()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Vector3 position = base.transform.position;
			if (base.transform.position.y > 0f)
			{
				position.y = startPos.y;
			}
			if (Vector3.Distance(position, startPos) > 5000f)
			{
				deactivated = true;
				CharacterJoint[] componentsInChildren = GetComponentsInChildren<CharacterJoint>();
				Rigidbody[] componentsInChildren2 = GetComponentsInChildren<Rigidbody>();
				CharacterJoint[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					Object.Destroy(array[i]);
				}
				Rigidbody[] array2 = componentsInChildren2;
				foreach (Rigidbody rigidbody in array2)
				{
					if (rigidbody != null)
					{
						rigidbody.useGravity = false;
						rigidbody.isKinematic = true;
					}
				}
				base.gameObject.SetActive(value: false);
				base.transform.position = new Vector3(-100f, -100f, -100f);
				base.transform.localScale = Vector3.zero;
				Object.Destroy(this);
			}
		}
		if (!deactivated)
		{
			Invoke("SlowCheck", 3f);
		}
	}
}
