using System;

/// <summary>
/// Indique qu'une propriété ou un champ doit être injecté avec une dépendance.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class InjectPropertyAttribute : Attribute
{
}