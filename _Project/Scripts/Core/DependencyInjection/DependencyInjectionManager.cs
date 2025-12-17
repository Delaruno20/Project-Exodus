using System;
using UnityEngine;

/// <summary>
/// Gestionnaire principal de l'injection de dépendances pour Unity.
/// Ce composant doit être placé sur un GameObject dans la scène et s'initialise automatiquement.
/// </summary>
[DefaultExecutionOrder(-1000)]
public sealed class DependencyInjectionManager : MonoBehaviour
{
    #region Champs sérialisés

    [Header("Configuration Générale")]
    [Tooltip("Si activé, ce GameObject ne sera pas détruit lors du chargement d'une nouvelle scène")]
    [SerializeField] private bool dontDestroyOnLoad = true;
    
    [Tooltip("Si activé, injecte automatiquement les dépendances dans tous les MonoBehaviour de la scène")]
    [SerializeField] private bool autoInjectMonoBehaviours = true;
    
    [Tooltip("Si activé, affiche des logs détaillés pendant l'initialisation")]
    [SerializeField] private bool verboseLogging = true;

    [Header("Dépendances Unity")]
    [Tooltip("Animator à utiliser pour les services d'animation")]
    [SerializeField] private Animator animator;

    #endregion

    #region Propriétés publiques

    /// <summary>
    /// Accès au conteneur de dépendances.
    /// </summary>
    public IDependencyContainer Container => _container;

    /// <summary>
    /// Instance singleton du gestionnaire.
    /// </summary>
    public static DependencyInjectionManager Instance { get; private set; }

    /// <summary>
    /// Indique si le gestionnaire est initialisé.
    /// </summary>
    public bool IsInitialized { get; private set; }

    #endregion

    #region Champs privés

    private DependencyContainer _container;

    #endregion

    #region Cycle de vie Unity

    private void Awake()
    {
        try
        {
            InitializeSingleton();
            InitializeContainer();
            ConfigureServices();
            PerformAutoInjection();
            
            IsInitialized = true;
            
            if (verboseLogging)
            {
                Debug.Log($"[DependencyInjectionManager] Initialisation réussie avec {_container.ServiceCount} services");
                Debug.Log(_container.GetDiagnosticInfo());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DependencyInjectionManager] Erreur critique lors de l'initialisation: {ex.Message}");
            Debug.LogException(ex);
            enabled = false; // Désactiver le composant en cas d'erreur
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            IsInitialized = false;
        }
    }

    #endregion

    #region Méthodes d'initialisation

    /// <summary>
    /// Initialise le pattern singleton.
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[DependencyInjectionManager] Instance déjà existante sur {Instance.gameObject.name}. " +
                           $"Destruction du duplicata sur {gameObject.name}.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
            
            if (verboseLogging)
                Debug.Log($"[DependencyInjectionManager] GameObject configuré pour persister entre les scènes");
        }
    }

    /// <summary>
    /// Initialise le conteneur de dépendances.
    /// </summary>
    private void InitializeContainer()
    {
        _container = new DependencyContainer();
        
        if (verboseLogging)
            Debug.Log("[DependencyInjectionManager] Conteneur de dépendances créé");
    }

    /// <summary>
    /// Configure tous les services de base de l'application.
    /// Vous pouvez personnaliser cette méthode selon vos besoins.
    /// </summary>
    private void ConfigureServices()
    {
        try
        {
            // Services de base (à personnaliser selon votre projet)
            RegisterCoreServices();
            RegisterUnityServices();
            RegisterApplicationServices();
            
            if (verboseLogging)
                Debug.Log($"[DependencyInjectionManager] Configuration des services terminée");
        }
        catch (Exception ex)
        {
            throw new DependencyInjectionException("Erreur lors de la configuration des services", ex);
        }
    }

    /// <summary>
    /// Enregistre les services de base du système.
    /// </summary>
    private void RegisterCoreServices()
    {
        // Exemple de services de base - à adapter selon votre architecture
        
        // Service de logging (exemple)
        // _container.RegisterSingleton<ILogger, UnityLogger>();
        
        // Service de gestion d'événements (exemple)
        // _container.RegisterSingleton<IEventBus, EventBus>();
        
        // Service de configuration (exemple)
        // _container.RegisterSingleton<IConfigurationService, ConfigurationService>();
    }

    /// <summary>
    /// Enregistre les services liés aux composants Unity.
    /// </summary>
    private void RegisterUnityServices()
    {
        // Enregistrer l'Animator si fourni
        if (animator != null)
        {
            // Exemple d'enregistrement d'un service d'animation
            _container.RegisterSingleton<IAnimatorService>(new AnimatorService(animator));
        }

        // Autres services Unity (Input, Audio, etc.)
        _container.RegisterSingleton<IInputManager, InputManager>();
        // _container.RegisterSingleton<IAudioManager, AudioManager>();
    }

    /// <summary>
    /// Enregistre les services spécifiques à l'application.
    /// </summary>
    private void RegisterApplicationServices()
    {
        // Services de logique métier
        _container.RegisterTransient<UpdateMovementAnimationActivity>();
        // _container.RegisterTransient<IPlayerService, PlayerService>();
        // _container.RegisterTransient<IGameStateService, GameStateService>();

        // Services de données
        // _container.RegisterSingleton<IDataService, DataService>();
        // _container.RegisterTransient<ISaveLoadService, SaveLoadService>();

        // Activities et commandes
        // _container.RegisterTransient<UpdateMovementAnimationActivity>();
    }

    /// <summary>
    /// Effectue l'injection automatique si activée.
    /// </summary>
    private void PerformAutoInjection()
    {
        if (autoInjectMonoBehaviours)
        {
            InjectAllMonoBehaviours();
        }
    }

    #endregion

    #region Méthodes publiques

    /// <summary>
    /// Injecte les dépendances dans tous les MonoBehaviour présents dans la scène.
    /// </summary>
    public void InjectAllMonoBehaviours()
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("[DependencyInjectionManager] Tentative d'injection avant initialisation complète");
            return;
        }

        var monoBehaviours = FindObjectsOfType<MonoBehaviour>(includeInactive: true);
        int injectedCount = 0;
        int errorCount = 0;

        foreach (var mb in monoBehaviours)
        {
            // Éviter l'auto-injection et les objets détruits
            if (mb == this || mb == null) continue;

            try
            {
                _container.InjectIntoMonoBehaviour(mb);
                injectedCount++;
                
                if (verboseLogging)
                    Debug.Log($"[DependencyInjectionManager] Injection réussie dans {mb.GetType().Name} sur {mb.gameObject.name}");
            }
            catch (Exception ex)
            {
                errorCount++;
                Debug.LogError($"[DependencyInjectionManager] Erreur lors de l'injection dans {mb.GetType().Name} " +
                             $"sur {mb.gameObject.name}: {ex.Message}", mb.gameObject);
                
                if (verboseLogging)
                    Debug.LogException(ex, mb.gameObject);
            }
        }

        var totalProcessed = injectedCount + errorCount;
        Debug.Log($"[DependencyInjectionManager] Injection terminée: {injectedCount} réussies, " +
                 $"{errorCount} erreurs sur {totalProcessed} MonoBehaviours traités");
    }

    /// <summary>
    /// Injecte les dépendances dans un MonoBehaviour spécifique.
    /// </summary>
    /// <param name="monoBehaviour">Le MonoBehaviour cible.</param>
    public void InjectMonoBehaviour(MonoBehaviour monoBehaviour)
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("[DependencyInjectionManager] Tentative d'injection avant initialisation complète");
            return;
        }

        if (monoBehaviour == null)
        {
            Debug.LogError("[DependencyInjectionManager] Tentative d'injection dans un MonoBehaviour null");
            return;
        }

        try
        {
            _container.InjectIntoMonoBehaviour(monoBehaviour);
            
            if (verboseLogging)
                Debug.Log($"[DependencyInjectionManager] Injection manuelle réussie dans {monoBehaviour.GetType().Name}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DependencyInjectionManager] Erreur lors de l'injection manuelle dans " +
                          $"{monoBehaviour.GetType().Name}: {ex.Message}", monoBehaviour.gameObject);
            throw;
        }
    }

    /// <summary>
    /// Enregistre un service supplémentaire au runtime.
    /// Utile pour les services créés dynamiquement.
    /// </summary>
    /// <param name="serviceRegistration">Action d'enregistrement à effectuer.</param>
    public void RegisterService(Action<IDependencyContainer> serviceRegistration)
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("[DependencyInjectionManager] Tentative d'enregistrement avant initialisation complète");
            return;
        }

        if (serviceRegistration == null)
            throw new ArgumentNullException(nameof(serviceRegistration));

        try
        {
            serviceRegistration(_container);
            
            if (verboseLogging)
                Debug.Log("[DependencyInjectionManager] Service enregistré dynamiquement");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DependencyInjectionManager] Erreur lors de l'enregistrement dynamique: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Méthodes de diagnostic

    /// <summary>
    /// Affiche les informations de diagnostic du conteneur dans la console.
    /// </summary>
    [ContextMenu("Afficher Diagnostic")]
    public void LogDiagnosticInfo()
    {
        if (_container == null)
        {
            Debug.Log("[DependencyInjectionManager] Conteneur non initialisé");
            return;
        }

        Debug.Log(_container.GetDiagnosticInfo());
    }

    /// <summary>
    /// Valide la configuration actuelle du conteneur.
    /// </summary>
    [ContextMenu("Valider Configuration")]
    public void ValidateConfiguration()
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("[DependencyInjectionManager] Impossible de valider: gestionnaire non initialisé");
            return;
        }

        Debug.Log($"[DependencyInjectionManager] Validation terminée: {_container.ServiceCount} services enregistrés");
        
        // Ici vous pourriez ajouter des validations spécifiques à votre application
        // Par exemple, vérifier que tous les services critiques sont enregistrés
    }

    /// <summary>
    /// Force une réinjection de tous les MonoBehaviour de la scène.
    /// Utile pour le debugging ou après des changements de configuration.
    /// </summary>
    [ContextMenu("Forcer Réinjection")]
    public void ForceReinjection()
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("[DependencyInjectionManager] Impossible de réinjecter: gestionnaire non initialisé");
            return;
        }

        Debug.Log("[DependencyInjectionManager] Démarrage de la réinjection forcée...");
        InjectAllMonoBehaviours();
    }

    #endregion

    #region Méthodes d'accès statiques

    /// <summary>
    /// Accès statique au conteneur de dépendances.
    /// Retourne null si le gestionnaire n'est pas initialisé.
    /// </summary>
    public static IDependencyContainer GetContainer()
    {
        return Instance?.Container;
    }

    /// <summary>
    /// Résout un service via l'accès statique.
    /// </summary>
    /// <typeparam name="T">Le type du service à résoudre.</typeparam>
    /// <returns>L'instance du service ou null si non disponible.</returns>
    public static T ResolveService<T>() where T : class
    {
        return GetContainer()?.Resolve<T>();
    }

    /// <summary>
    /// Tente de résoudre un service via l'accès statique sans lever d'exception.
    /// </summary>
    /// <typeparam name="T">Le type du service à résoudre.</typeparam>
    /// <returns>L'instance du service ou null si non disponible.</returns>
    public static T TryResolveService<T>() where T : class
    {
        return GetContainer()?.TryResolve<T>();
    }

    #endregion
}