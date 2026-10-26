[ConfigureSingleton(SingletonFlags.DestroyDuplicates)]
public class ChallengeDoneByDefault : MonoSingleton<ChallengeDoneByDefault>
{
	private bool prepared;

	public void Prepare()
	{
		if (!prepared)
		{
			MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
			prepared = true;
		}
	}
}
