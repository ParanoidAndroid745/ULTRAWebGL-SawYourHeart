using UnityEngine;
using UnityEngine.Events;

public class GabrielOutro : MonoBehaviour
{
	private Transform target;

	private Animator anim;

	public Vector3 middlePosition;

	public Gabriel gabe;

	public UnityEvent onEnableWings;

	public UnityEvent onRageStart;

	public UnityEvent onDisappear;

	private bool tracking;

	private void Start()
	{
		if (!target)
		{
			target = MonoSingleton<NewMovement>.Instance.transform;
		}
		anim = GetComponent<Animator>();
		Invoke("ToEndAnimation", 0.75f);
	}

	private void OnDisable()
	{
		CancelInvoke();
	}

	private void Update()
	{
		if (tracking)
		{
			Quaternion quaternion = Quaternion.LookRotation(new Vector3(target.position.x, base.transform.position.y, target.position.z) - base.transform.position, Vector3.up);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * Mathf.Clamp(Quaternion.Angle(base.transform.rotation, quaternion) * 2f, 1f, 90f));
		}
	}

	public void SetSource(Transform tf)
	{
		if (!target)
		{
			target = MonoSingleton<NewMovement>.Instance.transform;
		}
		base.transform.position = tf.position;
		base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
	}

	public void ToEndAnimation()
	{
		int num = Mathf.RoundToInt(Vector3.Distance(base.transform.position, middlePosition) / 2.5f);
		for (int i = 0; i < num; i++)
		{
			MindflayerDecoy component = CreateDecoy(Vector3.Lerp(base.transform.position, middlePosition, (float)i / (float)num)).GetComponent<MindflayerDecoy>();
			if ((bool)component)
			{
				component.fadeOverride = (float)i / (float)num + 0.1f;
			}
		}
		base.transform.position = middlePosition;
		Object.Instantiate(gabe.teleportSound, base.transform.position, Quaternion.identity);
		base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
		anim.Play("Outro");
	}

	private GameObject CreateDecoy(Vector3 position)
	{
		GameObject obj = Object.Instantiate(gabe.decoy, position, base.transform.GetChild(0).rotation);
		Animator componentInChildren = obj.GetComponentInChildren<Animator>();
		AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
		componentInChildren.Play(currentAnimatorStateInfo.shortNameHash, 0, currentAnimatorStateInfo.normalizedTime);
		componentInChildren.speed = 0f;
		return obj;
	}

	public void EnableWings()
	{
		onEnableWings?.Invoke();
		tracking = true;
	}

	public void RageStart()
	{
		onRageStart?.Invoke();
	}

	public void Disappear()
	{
		onDisappear?.Invoke();
	}
}
