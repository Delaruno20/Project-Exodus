using System;

/// <summary>
/// Exception levée lors d'erreurs liées à l'injection de dépendances.
/// </summary>
public sealed class DependencyInjectionException : Exception
{
    /// <summary>
    /// Initialise une nouvelle instance de <see cref="DependencyInjectionException"/>.
    /// </summary>
    /// <param name="message">Le message d'erreur.</param>
    public DependencyInjectionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="DependencyInjectionException"/>.
    /// </summary>
    /// <param name="message">Le message d'erreur.</param>
    /// <param name="innerException">L'exception interne.</param>
    public DependencyInjectionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}