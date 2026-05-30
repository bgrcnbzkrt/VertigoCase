using UnityEngine;

namespace Vertigo.Core
{
    public class FpsManager : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            // vSync overrides targetFrameRate on mobile, turn it off first
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
        }
    }
}
