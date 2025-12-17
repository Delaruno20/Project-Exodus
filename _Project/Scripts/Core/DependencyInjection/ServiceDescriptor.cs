using System;

/// <summary>
/// Décrit un service enregistré dans le conteneur de dépendances.
/// Contient toutes les informations nécessaires pour créer et gérer une instance du service.
/// </summary>
public sealed class ServiceDescriptor
{
    /// <summary>
    /// Le type du service (généralement une interface).
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Le type de l'implémentation concrète.
    /// </summary>
    public Type ImplementationType { get; }

    /// <summary>
    /// La durée de vie du service.
    /// </summary>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    /// L'instance singleton, si applicable.
    /// </summary>
    public object SingletonInstance { get; set; }

    /// <summary>
    /// Indique si ce descripteur représente une instance pré-créée.
    /// </summary>
    public bool IsInstance { get; }

    /// <summary>
    /// Initialise un nouveau descripteur de service.
    /// </summary>
    /// <param name="serviceType">Le type du service.</param>
    /// <param name="implementationType">Le type de l'implémentation.</param>
    /// <param name="lifetime">La durée de vie du service.</param>
    public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        Lifetime = lifetime;
        IsInstance = false;
    }

    /// <summary>
    /// Initialise un nouveau descripteur de service avec une instance pré-créée.
    /// </summary>
    /// <param name="serviceType">Le type du service.</param>
    /// <param name="singletonInstance">L'instance singleton.</param>
    public ServiceDescriptor(Type serviceType, object singletonInstance)
    {
        ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        SingletonInstance = singletonInstance ?? throw new ArgumentNullException(nameof(singletonInstance));
        ImplementationType = singletonInstance.GetType();
        Lifetime = ServiceLifetime.Singleton;
        IsInstance = true;
    }

    public override string ToString()
    {
        return $"Service: {ServiceType.Name} -> {ImplementationType.Name} ({Lifetime})";
    }
}