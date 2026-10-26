namespace ULTRAKILL.Cheats
{
	public class TeleportMenu : ICheat
	{
		private bool active;

		public string LongName => "Teleport Menu";

		public string Identifier => "ultrakill.teleport-menu";

		public string ButtonEnabledOverride => "CLOSE";

		public string ButtonDisabledOverride => "OPEN";

		public string Icon => "teleport";

		public bool IsActive => active;

		public bool DefaultState { get; }

		public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

		public void Enable()
		{
			if (MonoSingleton<CheatsManager>.Instance.MenuOpen)
			{
				MonoSingleton<CheatsManager>.Instance.HideMenu();
				MonoSingleton<OptionsManager>.Instance.UnPause();
			}
			MonoSingleton<CheatsController>.Instance.ShowTeleportPanel();
		}

		public void Disable()
		{
		}

		public void Update()
		{
		}
	}
}
