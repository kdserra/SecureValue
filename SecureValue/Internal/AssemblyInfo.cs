#nullable enable
using System.Runtime.CompilerServices;

// Internal test surface. This file (not the csproj) owns InternalsVisibleTo because the
// Unity package compiles these sources directly and csproj metadata does not transfer:
// the staging script copies this file verbatim into Runtime/Generated/Core, so the Unity
// test assembly gets the same visibility the .NET test assembly has.
[assembly: InternalsVisibleTo("SecureValue.Tests")]
[assembly: InternalsVisibleTo("SecureValue.Unity.Tests")]
[assembly: InternalsVisibleTo("SecureValue.Benchmarks")]
