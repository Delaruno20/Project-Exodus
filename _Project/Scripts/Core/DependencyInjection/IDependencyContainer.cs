
using System;
using UnityEngine;

/// <summary>
/// Interface définissant les opérations d'un conteneur d'injection de dépendances.
/// </summary>
public interface IDependencyContainer
{
    #region Enregistrement de Services
    
    /// <summary>
    /// Enregistre un service transient avec une interface et une implémentation.
    /// </summary>
    /// <typeparam name="TInterface">Le type de l'interface.</typeparam>
    /// <typeparam name="TImplementation">Le type de l'implémentation.</typeparam>
    /// <returns>Le conteneur pour permettre le chaînage des appels.</returns>
    IDependencyContainer RegisterTransient<TInterface, TImplementation>() 
        where TImplementation : class, TInterface;

    /// <summary>
    /// Enregistre un service singleton avec une interface et une implémentation.
    /// </summary>
    /// <typeparam name="TInterface">Le type de l'interface.</typeparam>
    /// <typeparam name="TImplementation">Le type de l'implémentation.</typeparam>
    /// <returns>Le conteneur pour permettre le chaînage des appels.</returns>
    IDependencyContainer RegisterSingleton<TInterface, TImplementation>() 
        where TImplementation : class, TInterface;

    /// <summary>
    /// Enregistre un service transient sans interface distincte.
    /// </summary>
    /// <typeparam name="T">Le type du service.</typeparam>
    /// <returns>Le conteneur pour permettre le chaînage des appels.</returns>
    IDependencyContainer RegisterTransient<T>() where T : class;

    /// <summary>
    /// Enregistre un service singleton sans interface distincte.
    /// </summary>
    /// <typeparam name="T">Le type du service.</typeparam>
    /// <returns>Le conteneur pour permettre le chaînage des appels.</returns>
    IDependencyContainer RegisterSingleton<T>() where T : class;

    /// <summary>
    /// Enregistre une instance singleton pré-créée.
    /// </summary>
    /// <typeparam name="T">Le type du service.</typeparam>
    /// <param name="instance">L'instance à enregistrer.</param>
    /// <returns>Le conteneur pour permettre le chaînage des appels.</returns>
    IDependencyContainer RegisterSingleton<T>(T instance) where T : class;

    /// <summary>
    /// Enregistre un service transient avec une implémentation pré-créée.
    /// </summary>
    /// <typeparam name="TInterface">Le type de l'interface.</typeparam>
    /// <param name="implementation">L'implémentation à enregistrer.</param>
    /// <returns>Le conteneur pour permettre le chaînage des appels.</returns>
    IDependencyContainer RegisterTransient<TInterface>(TInterface implementation) where TInterface : class;

    #endregion

    #region Résolution de Services

    /// <summary>
    /// Résout un service du type spécifié.
    /// </summary>
    /// <typeparam name="T">Le type du service à résoudre.</typeparam>
    /// <returns>L'instance du service ou null si non trouvé.</returns>
    T Resolve<T>() where T : class;

    /// <summary>
    /// Résout un service du type spécifié.
    /// </summary>
    /// <param name="type">Le type du service à résoudre.</param>
    /// <returns>L'instance du service ou null si non trouvé.</returns>
    object Resolve(Type type);

    /// <summary>
    /// Tente de résoudre un service du type spécifié sans lever d'exception.
    /// </summary>
    /// <typeparam name="T">Le type du service à résoudre.</typeparam>
    /// <returns>L'instance du service ou null si non trouvé.</returns>
    T TryResolve<T>() where T : class;

    /// <summary>
    /// Tente de résoudre un service du type spécifié sans lever d'exception.
    /// </summary>
    /// <param name="type">Le type du service à résoudre.</param>
    /// <returns>L'instance du service ou null si non trouvé.</returns>
    object TryResolve(Type type);

    #endregion

    #region Injection dans MonoBehaviour

    /// <summary>
    /// Injecte les dépendances dans un MonoBehaviour existant.
    /// </summary>
    /// <param name="monoBehaviour">Le MonoBehaviour cible.</param>
    void InjectIntoMonoBehaviour(MonoBehaviour monoBehaviour);

    #endregion

    #region Utilitaires

    /// <summary>
    /// Vérifie si un service est enregistré pour le type spécifié.
    /// </summary>
    /// <param name="type">Le type à vérifier.</param>
    /// <returns>True si le service est enregistré, sinon false.</returns>
    bool IsRegistered(Type type);

    /// <summary>
    /// Vérifie si un service est enregistré pour le type spécifié.
    /// </summary>
    /// <typeparam name="T">Le type à vérifier.</typeparam>
    /// <returns>True si le service est enregistré, sinon false.</returns>
    bool IsRegistered<T>();

    #endregion
}