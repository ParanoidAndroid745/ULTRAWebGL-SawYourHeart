[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class LoadingScreen : MonoSingleton<LoadingScreen>
{
	protected override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(value: false);
	}
}
