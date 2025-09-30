using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Initialization Order")] 
    [SerializeField] private bool initializeOnStart = true;

    private async void Start()
    {
        if (initializeOnStart) {
            await InitializeGame();
        }
    }

    private async UniTask InitializeGame()
    {
        Debug.Log("[Client] === Game Bootstrap Starting ===");

        Application.targetFrameRate = 60;
        
        await InitializeAddressables();

        InitializeSystems();

        InitializeUI();

        Debug.Log("[Client] === Game Bootstrap Complete ===");
    }

    private async UniTask InitializeAddressables()
    {
        Debug.Log("[Client] Initializing Addressables...");

        // Ждем загрузки BulletHitEffectsRepository
        await BulletHitEffectsRepository.Instance.Initialize();

        Debug.Log("[Client] ✅ Addressables initialized");
    }

    private void InitializeSystems()
    {
        Debug.Log("[Client] Initializing systems...");

        // Инициализация всех систем
        var objectPool = ObjectPool.Instance;
        var enemyManager = EnemyManager.Instance;

        Debug.Log("[Client] ✅ Systems initialized");
    }

    private async UniTask InitializeUI()
    {
        Debug.Log("[Client] Initializing UI...");

        // Инициализация UI
        var uiInitializer = FindObjectOfType<UIInitializer>();
        if (uiInitializer != null) {
            await uiInitializer.Initialize();
            Debug.Log("[Client] ✅ UI initialized");
        }
    }
}