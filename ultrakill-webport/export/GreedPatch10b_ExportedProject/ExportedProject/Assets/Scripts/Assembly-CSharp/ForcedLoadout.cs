using System;
using UnityEngine;

[Serializable]
public class ForcedLoadout
{
	public VariantSetting revolver;

	public VariantSetting altRevolver;

	public VariantSetting shotgun;

	public VariantSetting nailgun;

	public VariantSetting altNailgun;

	public VariantSetting railcannon;

	[Space]
	public ArmVariantSetting arm;
}
