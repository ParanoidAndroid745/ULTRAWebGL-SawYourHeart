using UnityEngine;

public class Follow : MonoBehaviour
{
	public float speed;

	public Transform target;

	public bool mimicPosition = true;

	public bool followX = true;

	public bool followY = true;

	public bool followZ = true;

	public bool mimicRotation;

	public bool rotX = true;

	public bool rotY = true;

	public bool rotZ = true;

	private bool followingPlayer;

	private void Awake()
	{
		if (!(target != null))
		{
			target = MonoSingleton<NewMovement>.Instance.transform;
			followingPlayer = true;
		}
	}

	private void Update()
	{
		if (mimicRotation)
		{
			Vector3 eulerAngles = base.transform.eulerAngles;
			if (followingPlayer)
			{
				if (rotX)
				{
					eulerAngles.x = 0f - MonoSingleton<CameraController>.Instance.rotationX;
				}
			}
			else if (rotX)
			{
				eulerAngles.x = target.eulerAngles.x;
			}
			if (rotY)
			{
				eulerAngles.y = target.eulerAngles.y;
			}
			if (rotZ)
			{
				eulerAngles.z = target.eulerAngles.z;
			}
			base.transform.eulerAngles = eulerAngles;
		}
		if (mimicPosition)
		{
			Vector3 position = new Vector3(followX ? target.position.x : base.transform.position.x, followY ? target.position.y : base.transform.position.y, followZ ? target.position.z : base.transform.position.z);
			if (speed == 0f)
			{
				base.transform.position = position;
				return;
			}
			float maxDistanceDelta = speed * Time.deltaTime;
			base.transform.position = Vector3.MoveTowards(base.transform.position, position, maxDistanceDelta);
		}
	}
}
