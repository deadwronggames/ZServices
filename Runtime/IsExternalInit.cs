namespace System.Runtime.CompilerServices;

/// <summary>
/// Workaround so that we can use the records keyword. Other assemblies must reference this Assembly.
/// Provides the IsExternalInit type to the compiler. 
/// </summary>
public static class IsExternalInit { }