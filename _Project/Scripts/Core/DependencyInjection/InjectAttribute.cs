using System;
using UnityEngine;

/// <summary>
/// Indique qu'un constructeur doit être utilisé pour l'injection de dépendances.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public sealed class InjectAttribute : Attribute
{
}