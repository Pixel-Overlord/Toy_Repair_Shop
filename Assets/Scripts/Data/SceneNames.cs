namespace ToyRepairShop.Data
{
    /// <summary>
    /// Centralized scene name constants so scene loading never relies on
    /// magic strings scattered across the codebase. Values must match the
    /// scene file names under Assets/Scenes.
    /// </summary>
    public static class SceneNames
    {
        public const string Bootstrap = "Bootstrap";
        public const string Loading = "Loading";
        public const string MainMenu = "MainMenu";
        public const string Workshop = "Workshop";
    }
}
