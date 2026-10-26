using UnityEngine;
using UnityEngine.SceneManagement;

namespace ULTRAKILL.Cheats
{
	public class SpawnSpawnerArm : ICheat
	{
		private bool active;

		private DebugArm arm;

		public string LongName => "Spawner Arm";

		public string Identifier => "ultrakill.spawner-arm";

		public string ButtonEnabledOverride => "REMOVE";

		public string ButtonDisabledOverride => "EQUIP";

		public string Icon => "spawner-arm";

		public bool IsActive => active;

		public bool DefaultState { get; }

		public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

		public void Enable()
		{
			if (!MonoSingleton<CheatsManager>.Instance.GetCheatState("ultrakill.sandbox.crash-mode"))
			{
				active = true;
				GameObject gameObject = Object.Instantiate(MonoSingleton<CheatsController>.Instance.spawnerArm, MonoSingleton<GunControl>.Instance.transform);
				MonoSingleton<GunControl>.Instance.slot5.Add(gameObject);
				MonoSingleton<GunControl>.Instance.UpdateWeaponList();
				arm = gameObject.GetComponent<DebugArm>();
				arm.cameraCtrl = MonoSingleton<CameraController>.Instance;
			}
		}

		public void Disable()
		{
			active = false;
			if (!SceneManager.GetActiveScene().isLoaded)
			{
				return;
			}
			if ((bool)arm)
			{
				if ((bool)MonoSingleton<GunControl>.Instance && MonoSingleton<GunControl>.Instance.slot5.Contains(arm.gameObject))
				{
					MonoSingleton<GunControl>.Instance.slot5.Remove(arm.gameObject);
				}
				Object.Destroy(arm.gameObject);
				arm = null;
			}
			if ((bool)MonoSingleton<GunControl>.Instance)
			{
				MonoSingleton<GunControl>.Instance.UpdateWeaponList();
			}
		}

		public void Update()
		{
		}
	}
}
