namespace ULTRAKILL.Cheats
{
	public class KillAllEnemies : ICheat
	{
		public string LongName => "Kill All Enemies";

		public string Identifier => "ultrakill.kill-all-enemies";

		public string ButtonEnabledOverride => null;

		public string ButtonDisabledOverride => "Kill All";

		public string Icon => "death";

		public bool IsActive => false;

		public bool DefaultState => false;

		public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

		public void Enable()
		{
			foreach (EnemyIdentifier enemy in MonoSingleton<EnemyTracker>.Instance.enemies)
			{
				enemy.InstaKill();
			}
		}

		public void Disable()
		{
		}

		public void Update()
		{
		}
	}
}
