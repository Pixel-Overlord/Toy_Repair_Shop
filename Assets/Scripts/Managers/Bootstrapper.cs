using ToyRepairShop.Data;
using UnityEngine;

namespace ToyRepairShop.Managers
{
    /// <summary>
    /// Entry point of the game, present only in the Bootstrap scene. By the
    /// time its Start runs, every persistent manager in the scene has
    /// already initialized (Unity runs all Awake calls before any Start),
    /// so this class only has to hand off to MainMenu.
    /// </summary>
    public sealed class Bootstrapper : MonoBehaviour
    {
        private void Start()
        {
            SceneLoader.Instance.LoadSceneAsync(SceneNames.MainMenu);
        }
    }
}
