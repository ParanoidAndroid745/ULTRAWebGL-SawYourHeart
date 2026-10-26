public interface IArmModule
{
	void Update(float delta);

	void PrimaryUse();

	void SecondaryUse();

	void OnDeactivate();

	void OnActivate();
}
