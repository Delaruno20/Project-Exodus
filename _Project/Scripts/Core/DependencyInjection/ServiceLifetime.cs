using System;

/// <summary>
/// Définit la durée de vie d'un service dans le conteneur de dépendances.
/// </summary>
public enum ServiceLifetime
{
    /// <summary>
    /// Une nouvelle instance est créée à chaque résolution.
    /// </summary>
    Transient,
    
    /// <summary>
    /// Une seule instance est créée et réutilisée pour toutes les résolutions.
    /// </summary>
    Singleton
}