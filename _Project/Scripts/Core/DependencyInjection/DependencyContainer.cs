using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Implémentation du conteneur d'injection de dépendances pour Unity.
/// Gère l'enregistrement, la résolution et l'injection automatique des services.
/// </summary>
public sealed class DependencyContainer : IDependencyContainer
{
    #region Champs privés

    /// <summary>
    /// Dictionnaire contenant tous les services enregistrés.
    /// </summary>
    private readonly Dictionary<Type, ServiceDescriptor> _services = new Dictionary<Type, ServiceDescriptor>();

    /// <summary>
    /// Cache pour les constructeurs résolus afin d'améliorer les performances.
    /// </summary>
    private readonly Dictionary<Type, ConstructorInfo> _constructorCache = new Dictionary<Type, ConstructorInfo>();

    /// <summary>
    /// Pile de résolution pour détecter les dépendances circulaires.
    /// </summary>
    private readonly HashSet<Type> _resolutionStack = new HashSet<Type>();

    #endregion

    #region Propriétés publiques

    /// <summary>
    /// Nombre de services enregistrés dans le conteneur.
    /// </summary>
    public int ServiceCount => _services.Count;

    #endregion

    #region Enregistrement de Services

    /// <inheritdoc />
    public IDependencyContainer RegisterTransient<TInterface, TImplementation>() 
        where TImplementation : class, TInterface
    {
        return RegisterService<TInterface, TImplementation>(ServiceLifetime.Transient);
    }

    /// <inheritdoc />
    public IDependencyContainer RegisterSingleton<TInterface, TImplementation>() 
        where TImplementation : class, TInterface
    {
        return RegisterService<TInterface, TImplementation>(ServiceLifetime.Singleton);
    }

    /// <inheritdoc />
    public IDependencyContainer RegisterTransient<T>() where T : class
    {
        return RegisterService<T, T>(ServiceLifetime.Transient);
    }

    /// <inheritdoc />
    public IDependencyContainer RegisterSingleton<T>() where T : class
    {
        return RegisterService<T, T>(ServiceLifetime.Singleton);
    }

    /// <inheritdoc />
    public IDependencyContainer RegisterSingleton<T>(T instance) where T : class
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance), "L'instance ne peut pas être null");

        var serviceType = typeof(T);
        var descriptor = new ServiceDescriptor(serviceType, instance);
        
        _services[serviceType] = descriptor;
        
        Debug.Log($"Service singleton enregistré avec instance: {serviceType.Name}");
        return this;
    }

    /// <inheritdoc />
    public IDependencyContainer RegisterTransient<TInterface>(TInterface implementation) where TInterface : class
    {
        if (implementation == null)
            throw new ArgumentNullException(nameof(implementation), "L'implémentation ne peut pas être null");

        var serviceType = typeof(TInterface);
        var implementationType = implementation.GetType();
        
        ValidateServiceRegistration(serviceType, implementationType);
        
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        _services[serviceType] = descriptor;
        
        Debug.Log($"Service transient enregistré: {serviceType.Name} -> {implementationType.Name}");
        return this;
    }

    #endregion

    #region Résolution de Services

    /// <inheritdoc />
    public T Resolve<T>() where T : class
    {
        return (T)Resolve(typeof(T));
    }

    /// <inheritdoc />
    public object Resolve(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (!_services.TryGetValue(type, out var serviceDescriptor))
        {
            throw new DependencyInjectionException($"Service non enregistré: {type.Name}");
        }

        try
        {
            return CreateInstance(serviceDescriptor);
        }
        catch (Exception ex) when (!(ex is DependencyInjectionException))
        {
            throw new DependencyInjectionException($"Erreur lors de la résolution du service {type.Name}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public T TryResolve<T>() where T : class
    {
        return (T)TryResolve(typeof(T));
    }

    /// <inheritdoc />
    public object TryResolve(Type type)
    {
        try
        {
            return Resolve(type);
        }
        catch (DependencyInjectionException)
        {
            return null;
        }
    }

    #endregion

    #region Injection dans MonoBehaviour

    /// <inheritdoc />
    public void InjectIntoMonoBehaviour(MonoBehaviour monoBehaviour)
    {
        if (monoBehaviour == null)
            throw new ArgumentNullException(nameof(monoBehaviour));

        try
        {
            InjectPropertiesAndFields(monoBehaviour);
        }
        catch (Exception ex)
        {
            throw new DependencyInjectionException(
                $"Erreur lors de l'injection dans {monoBehaviour.GetType().Name}: {ex.Message}", ex);
        }
    }

    #endregion

    #region Utilitaires

    /// <inheritdoc />
    public bool IsRegistered(Type type)
    {
        return _services.ContainsKey(type);
    }

    /// <inheritdoc />
    public bool IsRegistered<T>()
    {
        return IsRegistered(typeof(T));
    }

    #endregion

    #region Méthodes privées - Enregistrement

    /// <summary>
    /// Enregistre un service avec le type d'interface et d'implémentation spécifiés.
    /// </summary>
    private IDependencyContainer RegisterService<TInterface, TImplementation>(ServiceLifetime lifetime)
        where TImplementation : class, TInterface
    {
        var serviceType = typeof(TInterface);
        var implementationType = typeof(TImplementation);
        
        ValidateServiceRegistration(serviceType, implementationType);
        
        var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);
        _services[serviceType] = descriptor;
        
        Debug.Log($"Service {lifetime.ToString().ToLower()} enregistré: {serviceType.Name} -> {implementationType.Name}");
        return this;
    }

    /// <summary>
    /// Valide qu'un enregistrement de service est correct.
    /// </summary>
    private static void ValidateServiceRegistration(Type serviceType, Type implementationType)
    {
        if (serviceType == null)
            throw new ArgumentNullException(nameof(serviceType));
        
        if (implementationType == null)
            throw new ArgumentNullException(nameof(implementationType));

        if (!serviceType.IsAssignableFrom(implementationType))
        {
            throw new DependencyInjectionException(
                $"Le type {implementationType.Name} n'implémente pas {serviceType.Name}");
        }

        if (implementationType.IsAbstract)
        {
            throw new DependencyInjectionException(
                $"Le type d'implémentation {implementationType.Name} ne peut pas être abstrait");
        }
        
        if (implementationType.IsInterface)
        {
            throw new DependencyInjectionException(
                $"Le type d'implémentation {implementationType.Name} ne peut pas être une interface");
        }
    }

    #endregion

    #region Méthodes privées - Création d'instances

    /// <summary>
    /// Crée une instance selon le descripteur de service.
    /// </summary>
    private object CreateInstance(ServiceDescriptor serviceDescriptor)
    {
        if (serviceDescriptor == null)
            throw new ArgumentNullException(nameof(serviceDescriptor));

        // Vérification des dépendances circulaires
        if (_resolutionStack.Contains(serviceDescriptor.ServiceType))
        {
            var dependencyChain = string.Join(" -> ", _resolutionStack.Select(t => t.Name));
            throw new DependencyInjectionException(
                $"Dépendance circulaire détectée: {dependencyChain} -> {serviceDescriptor.ServiceType.Name}");
        }

        try
        {
            _resolutionStack.Add(serviceDescriptor.ServiceType);

            return serviceDescriptor.Lifetime switch
            {
                ServiceLifetime.Singleton => GetOrCreateSingleton(serviceDescriptor),
                ServiceLifetime.Transient => CreateNewInstance(serviceDescriptor),
                _ => throw new DependencyInjectionException($"Durée de vie non supportée: {serviceDescriptor.Lifetime}")
            };
        }
        finally
        {
            _resolutionStack.Remove(serviceDescriptor.ServiceType);
        }
    }

    /// <summary>
    /// Récupère ou crée une instance singleton.
    /// </summary>
    private object GetOrCreateSingleton(ServiceDescriptor serviceDescriptor)
    {
        if (serviceDescriptor.SingletonInstance != null)
            return serviceDescriptor.SingletonInstance;
        
        serviceDescriptor.SingletonInstance = CreateNewInstance(serviceDescriptor);
        return serviceDescriptor.SingletonInstance;
    }

    /// <summary>
    /// Crée une nouvelle instance du service.
    /// </summary>
    private object CreateNewInstance(ServiceDescriptor serviceDescriptor)
    {
        if (serviceDescriptor.IsInstance && serviceDescriptor.SingletonInstance != null)
        {
            return serviceDescriptor.SingletonInstance;
        }

        return CreateInstanceWithDependencyInjection(serviceDescriptor);
    }

    /// <summary>
    /// Crée une instance en injectant automatiquement les dépendances.
    /// </summary>
    private object CreateInstanceWithDependencyInjection(ServiceDescriptor serviceDescriptor)
    {
        var constructor = GetBestConstructor(serviceDescriptor.ImplementationType);
        
        if (constructor == null)
        {
            throw new DependencyInjectionException(
                $"Aucun constructeur approprié trouvé pour {serviceDescriptor.ImplementationType.Name}");
        }
        
        var parameters = constructor.GetParameters();
        var arguments = ResolveConstructorParameters(parameters, serviceDescriptor.ImplementationType);
        
        object instance;
        try
        {
            instance = Activator.CreateInstance(serviceDescriptor.ImplementationType, arguments);
        }
        catch (Exception ex)
        {
            throw new DependencyInjectionException(
                $"Erreur lors de la création de l'instance pour {serviceDescriptor.ImplementationType.Name}: {ex.Message}", ex);
        }
        
        InjectPropertiesAndFields(instance);
        
        return instance;
    }

    /// <summary>
    /// Résout les paramètres d'un constructeur.
    /// </summary>
    private object[] ResolveConstructorParameters(ParameterInfo[] parameters, Type targetType)
    {
        var arguments = new object[parameters.Length];
        
        for (int i = 0; i < arguments.Length; i++)
        {
            var parameterType = parameters[i].ParameterType;
            var parameterName = parameters[i].Name;
            
            arguments[i] = TryResolve(parameterType);

            if (arguments[i] == null)
            {
                throw new DependencyInjectionException(
                    $"Impossible de résoudre le paramètre '{parameterName}' de type {parameterType.Name} " +
                    $"pour le constructeur de {targetType.Name}. " +
                    $"Assurez-vous que le service est enregistré dans le conteneur.");
            }
        }
        
        return arguments;
    }

    #endregion

    #region Méthodes privées - Injection de propriétés et champs

    /// <summary>
    /// Injecte les dépendances dans les propriétés et champs marqués.
    /// </summary>
    private void InjectPropertiesAndFields(object instance)
    {
        if (instance == null) return;

        var type = instance.GetType();

        InjectProperties(type, instance);
        InjectFields(type, instance);
    }

    /// <summary>
    /// Injecte les dépendances dans les propriétés marquées avec [InjectProperty].
    /// </summary>
    private void InjectProperties(Type type, object instance)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.GetCustomAttribute<InjectPropertyAttribute>() != null);

        foreach (var property in properties)
        {
            try
            {
                var dependency = TryResolve(property.PropertyType);
                if (dependency != null)
                {
                    property.SetValue(instance, dependency);
                }
                else
                {
                    Debug.LogWarning($"Impossible d'injecter la propriété {property.Name} de type {property.PropertyType.Name} " +
                                   $"dans {type.Name}: service non enregistré");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erreur lors de l'injection de la propriété {property.Name} dans {type.Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Injecte les dépendances dans les champs marqués avec [InjectProperty].
    /// </summary>
    private void InjectFields(Type type, object instance)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<InjectPropertyAttribute>() != null);

        foreach (var field in fields)
        {
            try
            {
                var dependency = TryResolve(field.FieldType);
                if (dependency != null)
                {
                    field.SetValue(instance, dependency);
                }
                else
                {
                    Debug.LogWarning($"Impossible d'injecter le champ {field.Name} de type {field.FieldType.Name} " +
                                   $"dans {type.Name}: service non enregistré");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erreur lors de l'injection du champ {field.Name} dans {type.Name}: {ex.Message}");
            }
        }
    }

    #endregion

    #region Méthodes privées - Utilitaires

    /// <summary>
    /// Détermine le meilleur constructeur pour l'injection de dépendances.
    /// </summary>
    private ConstructorInfo GetBestConstructor(Type implementationType)
    {
        // Utiliser le cache si disponible
        if (_constructorCache.TryGetValue(implementationType, out var cachedConstructor))
            return cachedConstructor;

        var constructors = implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        
        if (constructors.Length == 0)
        {
            throw new DependencyInjectionException($"Aucun constructeur public trouvé pour {implementationType.Name}");
        }

        // Priorité au constructeur marqué avec [Inject]
        var injectConstructor = constructors.FirstOrDefault(c => c.GetCustomAttribute<InjectAttribute>() != null);
        if (injectConstructor != null)
        {
            _constructorCache[implementationType] = injectConstructor;
            return injectConstructor;
        }

        // Sinon, choisir le constructeur avec le plus de paramètres (plus de dépendances)
        var bestConstructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
        _constructorCache[implementationType] = bestConstructor;
        
        return bestConstructor;
    }

    #endregion

    #region Méthodes de diagnostic

    /// <summary>
    /// Retourne une représentation textuelle des services enregistrés.
    /// </summary>
    public override string ToString()
    {
        var services = _services.Values.Select(s => s.ToString());
        return $"DependencyContainer ({ServiceCount} services):\n" + string.Join("\n", services);
    }

    /// <summary>
    /// Obtient des informations de diagnostic sur le conteneur.
    /// </summary>
    public string GetDiagnosticInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"=== Diagnostic du Conteneur de Dépendances ===");
        info.AppendLine($"Nombre de services enregistrés: {ServiceCount}");
        info.AppendLine($"Taille du cache de constructeurs: {_constructorCache.Count}");
        info.AppendLine();
        
        if (_services.Count > 0)
        {
            info.AppendLine("Services enregistrés:");
            foreach (var kvp in _services.OrderBy(s => s.Key.Name))
            {
                var service = kvp.Value;
                info.AppendLine($"  • {service}");
            }
        }
        
        return info.ToString();
    }

    #endregion
}
