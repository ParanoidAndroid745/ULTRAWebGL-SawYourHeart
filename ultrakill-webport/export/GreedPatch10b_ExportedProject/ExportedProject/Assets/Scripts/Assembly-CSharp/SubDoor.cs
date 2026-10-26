using UnityEngine;

public class SubDoor : MonoBehaviour
{
	public SubDoorType type;

	public Vector3 openPos;

	public Vector3 origPos;

	public Vector3 targetPos;

	public float speed = 1f;

	public bool valuesSet;

	private AudioSource aud;

	private float origPitch;

	public Door dr;

	private Animator anim;

	public AudioClip[] sounds;

	public AudioClip stopSound;

	public UltrakillEvent[] animationEvents;

	private void Awake()
	{
		SetValues();
	}

	private void Update()
	{
		if (type == SubDoorType.Animation)
		{
			if (!anim)
			{
				return;
			}
			float normalizedTime = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
			if (normalizedTime > 1f)
			{
				anim.Play(0, -1, 1f);
				anim.SetFloat("Speed", 0f);
				if ((bool)stopSound)
				{
					aud.loop = false;
					aud.clip = stopSound;
					aud.Play();
				}
				else
				{
					aud.Stop();
				}
			}
			else if (normalizedTime < 0f)
			{
				anim.Play(0, -1, 0f);
				anim.SetFloat("Speed", 0f);
				if ((bool)stopSound)
				{
					aud.loop = false;
					aud.clip = stopSound;
					aud.Play();
				}
				else
				{
					aud.Stop();
				}
			}
		}
		else
		{
			if (!(base.transform.localPosition != targetPos))
			{
				return;
			}
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, targetPos, Time.deltaTime * speed);
			if (base.transform.localPosition == targetPos)
			{
				if (targetPos == origPos)
				{
					dr?.BigDoorClosed();
				}
				else
				{
					dr?.onFullyOpened?.Invoke();
				}
				if ((bool)aud)
				{
					aud.Stop();
				}
			}
		}
	}

	public void Open()
	{
		if (type == SubDoorType.Animation)
		{
			if ((bool)aud && anim.GetFloat("Speed") != speed)
			{
				aud.pitch = origPitch + Random.Range(-0.1f, 0.1f);
				aud.Play();
			}
			anim.SetFloat("Speed", speed);
		}
		else
		{
			targetPos = origPos + openPos;
			if ((bool)aud && base.transform.localPosition != targetPos)
			{
				aud.pitch = origPitch + Random.Range(-0.1f, 0.1f);
				aud.Play();
			}
		}
	}

	public void Close()
	{
		if (type == SubDoorType.Animation)
		{
			if ((bool)aud && anim.GetFloat("Speed") != 0f - speed)
			{
				aud.pitch = origPitch + Random.Range(-0.1f, 0.1f);
				aud.Play();
			}
			anim.SetFloat("Speed", 0f - speed);
		}
		else
		{
			targetPos = origPos;
			if ((bool)aud && base.transform.localPosition != targetPos)
			{
				aud.pitch = origPitch + Random.Range(-0.1f, 0.1f);
				aud.Play();
			}
		}
	}

	public void SetValues()
	{
		if (!valuesSet)
		{
			valuesSet = true;
			origPos = base.transform.localPosition;
			targetPos = origPos;
			aud = GetComponent<AudioSource>();
			if ((bool)aud)
			{
				origPitch = aud.pitch;
			}
			if (type == SubDoorType.Animation)
			{
				anim = GetComponent<Animator>();
			}
		}
	}

	public void AnimationEvent(int i)
	{
		animationEvents[i].Invoke();
	}

	public void PlaySound(int targetSound)
	{
		if (!(aud.clip == sounds[targetSound]) || !aud.isPlaying)
		{
			aud.clip = sounds[targetSound];
			aud.loop = true;
			aud.Play();
		}
	}
}
