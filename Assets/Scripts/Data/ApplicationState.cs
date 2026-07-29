namespace ToyRepairShop.Data
{
    /// <summary>
    /// High-level state of the running application, owned by GameManager.
    /// </summary>
    public enum ApplicationState
    {
        Initializing,
        MainMenu,
        InGame,
        Paused
    }
}
