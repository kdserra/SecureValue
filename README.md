<p align="center">
  <img src="https://raw.githubusercontent.com/kdserra/SecureValue/master/icon.png" width="128" alt="SecureValue icon" />
</p>

<h1 align="center">🛡️ SecureValue</h1>

<p align="center">
  Memory-encrypted values for .NET
</p>

<p align="center">
  <a href="https://github.com/kdserra/SecureValue/actions/workflows/release.yml"><img src="https://github.com/kdserra/SecureValue/actions/workflows/release.yml/badge.svg" alt="Build status" /></a>
  <a href="https://www.nuget.org/packages/SecureValue/"><img src="https://img.shields.io/nuget/v/SecureValue.svg" alt="NuGet version" /></a>
  <a href="https://github.com/kdserra/SecureValue/blob/master/LICENSE.md"><img src="https://img.shields.io/badge/License-MIT-green.svg" alt="MIT license" /></a>
  <a href="#compatibility"><img src="https://img.shields.io/badge/.NET-net5.0--10.0_%7C_netstandard2.1-512BD4" alt=".NET targets" /></a>
  <a href="#installation"><img src="https://img.shields.io/badge/Unity-2021.3%2B-222222?logo=unity" alt="Unity 2021.3+" /></a>
</p>

<p align="center">
  <a href="#features">Features</a> •
  <a href="#installation">Installation</a> •
  <a href="#getting-started">Getting Started</a> •
  <a href="#supported-types">Supported Types</a> •
  <a href="#compatibility">Compatibility</a> •
  <a href="#best-practices">Best Practices</a> •
  <a href="#samples">Samples</a> •
  <a href="#benchmark-results">Benchmarks</a> •
  <a href="#security-recommendations">Security Recommendations</a> •
  <a href="#license">License</a>
</p>

<a id="what-is-securevalue"></a>

## 🧭 What is SecureValue?

SecureValue is a C# library that keeps important game values encrypted in process memory while
they are stored. It is designed for values commonly targeted by memory editors: currency, health,
ammunition, scores, cooldowns, unlocks, and progression.

Use `Secure*` wrappers like ordinary C# values: assignments encrypt, and reads return the
underlying value.

```csharp
using SecureValue;

SecureInt health = 100;     // Encrypt `int` to `SecureInt`
int currentHealth = health; // Decrypt `SecureInt` to `int`
```

<a id="features"></a>

## ✨ Features

- **Encrypted in memory.**

  Values are stored in memory in an encrypted state, protecting them from being targeted by memory
  scanners.

- **Saved to disk as encrypted data.**

  Scene data never holds a plain initial value that could be intercepted while it is being loaded on
  startup.

- **4-stage Feistel cipher, over XOR.**

  Four rounds of Feistel encryption guarantees sufficient avalanche mixing, while being fast. Lower
  number of rounds is faster, but produces scannable ciphertext patterns. XOR ciphers are faster,
  but easy to locate via directional scanning, basic for attackers to implement, and are easy to
  attack blindly with bit flipping.

- **A fresh salt for every write.**

  The same plaintext gets a different ciphertext each time and in each field, so there is no single
  reusable ciphertext pattern for a value such as `100`.

- **Avalanche mixing.**

  When a plaintext value increases, its ciphertext may increase or decrease unpredictably. Memory
  editors cannot follow the value's direction with an “increased/decreased” scan, or find it with
  exact value scans.

- **Tamper detection.**

  A keyed integrity tag is checked on read. A tampered value throws `TamperedException` and raises `TamperingNotifier.TamperingDetected`.

- **Prevent tampered consumption.**

  Throwing `TamperedException` on read blocks the app from consuming corrupted state before handling
  the tampering event (e.g., spending tampered currency).

- **Backup copy restores tampered values.**

  Each value is stored as two independent copies with separate ciphertexts, salts, and integrity tags.
  If one copy is tampered with, the uncorrupted copy restores the damaged copy on the next read, or
  write.

- **CSPRNG-seeded PRNG.**

  Uses the operating system's slower cryptographic random generator **once** at startup to
  attain a secure RNG seed, then a faster seeded generator for per-write salts.

- **A broad range of supported types.**

  SecureValue provides wrappers for core C# types, .NET numerics, and Unity types.

  See [Supported types](#supported-types) for the complete list.

- **Unity serialization beyond Unity's built-in types.**

  SecureValue's Unity editor integration supports its full Unity-compatible wrapper set, including
  .NET numerics that Unity does not normally serialize.

- **SecureString without GC via Span API.**

  You can read via `CopyTo(Span<char>)`, or `StackDecrypt`, and write via assignment from
  `ReadOnlySpan<char>`.  With no intermediate `string`, and zero heap allocations upto 16
  characters.

  See [Zero heap-allocation `SecureString`](#string-span) section to learn more.

- **Simple to use.**

  `SecureInt` can be assigned from an `int`, used with familiar operators, and read back as an `int`.

- **Fast, and secure.**

  SecureValue encrypts vulnerable data with a stronger 4-stage feistel cipher, proper avalanche
  mixing, salting, with a separate encrypted value backup, providing tamper detection and recovery,
  while only operating in nanoseconds.

- **Zero heap-allocation primitive operations.**

  Fixed-size wrappers allocate no managed memory on reads or writes. `SecureString` and
  `SecureBigInteger` allocate for their variable-sized data.

- **Extensive test suite.**

  Validated across [XUnit](https://github.com/kdserra/SecureValue/tree/master/SecureValue.Tests)
  and [Unity Test Runner](https://github.com/kdserra/SecureValue/tree/master/SecureValue.Unity/Tests)
  covering thread safety, cipher avalanche properties, tamper detection, edge cases, and millions
  of round-trip iterations.

- **Highly Compatible.**

  Supports everything from .NET Standard 2.1, to .NET 5-10, with dedicated Unity integration, each
  shipping with dedicated binaries to ensure maximum performance for your .NET version.

- **No dependencies.**

<a id="installation"></a>

## 📦 Installation

### .NET with NuGet

```sh
dotnet add package SecureValue
```

You can also search for **SecureValue** in NuGet Package Manager, or visit the
[NuGet package page](https://www.nuget.org/packages/SecureValue/).

### .NET Binaries

Precompiled binaries are available from [GitHub Releases](https://github.com/kdserra/SecureValue/releases).

### Unity with UPM

Requires Unity **2021.3 or later**.

1. Open **Window → Package Manager**.
2. Select **+ → Add package from git URL**.
3. Enter `https://github.com/kdserra/SecureValue.git?path=SecureValue.Unity`.

### Unity with a `.unitypackage`

1. Download the `.unitypackage` from the [GitHub Releases page](https://github.com/kdserra/SecureValue/releases), 
2. Import it with **Assets → Import Package → Custom Package**.

<a id="getting-started"></a>

## 🚀 Getting started

### 🧊.NET

```csharp
using System;
using System.Numerics;
using SecureValue;
using SecureValue.Numerics;

SecureInt score = 0;
score += 1000;
int currentScore = score;

SecureVector3 velocity = new Vector3(1, 2, 3);
SecureDateTime deadline = DateTime.UtcNow.AddDays(7);

TamperingNotifier.TamperingDetected += () => Console.WriteLine("Tampering detected.");
```

Each wrapper converts implicitly to and from its underlying type. The wrapper stays encrypted
while stored; a plain local such as `currentScore` is available to your code while you use it.

<a id="unity"></a>

## 🎮 Unity

Unity types are fully supported, and SecureValue wrappers are serialized as encrypted data, not
the plain-text value.

```csharp
using SecureValue;
using SecureValue.Unity;
using UnityEngine;

public sealed class PlayerStats : MonoBehaviour
{
    [SerializeField] private SecureInt _health = 100;
    [SerializeField] private SecureFloat _speed = 6.0f;
    [SerializeField] private SecureVector3 _spawnPoint = new Vector3(0f, 1f, 0f);
}
```

### 🔨 Manual integrations

SecureValue also works in third-party engines without an official integration. Assign values to
wrappers as you would in any C# project:

```csharp
using SecureValue;

public sealed class PlayerWallet : EngineComponent
{
    [EngineInject] private int _startingCoins;
    private SecureInt _coins;

    public void OnStartup()
    {
        _coins = _startingCoins;
    }
}
```

The main downside to this is that the starting value is unencrypted until it's assigned to the
encrypted `_coins` field, creating a temporary attack vector targeting the initial
unencrypted value prior to it being loaded into the secure field.

This is why using a dedicated engine integration is preferable, as it ensures that the
secure value is always stored in an encrypted state, even on startup.

### ⚒ Build a custom engine integration

You can also build an editor integration for your engine. The integration point is small
on purpose: you never touch the cryptographic guts.

Integration code only ever handles two things: the encrypted words as a `uint[]` blob,
and the native underlying value (`int`, `float`, …).

These files show the pattern as implemented for Unity:

- [`ISecureSerialization.cs`](https://github.com/kdserra/SecureValue/blob/master/SecureValue/Utility/ISecureSerialization.cs) —
  the three-method bridge every wrapper implements: `IsUnset`, `SaveToSerialized` (live
  value → encrypted `uint[]` words), `LoadFromSerialized` (words → verified, re-encrypted
  live value).
- [`SecureIntDrawer.cs`](https://github.com/kdserra/SecureValue/blob/master/SecureValue.Unity/Runtime/EditorDrawers/SecureIntDrawer.cs) —
  the editor side: it reads the live wrapper as a plain `int` for display and writes edits
  back through implicit conversion, so designers only ever see a normal integer field.

**Note:** `uint[]` is only used for saving/loading encrypted data from the editor integration,
the actual secure values do not rely on arrays *(except for reference types like `string` and
`BigInteger`)*.

<a id="supported-types"></a>

## 🧩 Supported Types

### Core (`SecureValue`)

| Secure type | Wraps |
|---|---|
| `SecureBool` | `bool` |
| `SecureByte` | `byte` |
| `SecureSByte` | `sbyte` |
| `SecureShort` | `short` |
| `SecureUShort` | `ushort` |
| `SecureInt` | `int` |
| `SecureUInt` | `uint` |
| `SecureLong` | `long` |
| `SecureULong` | `ulong` |
| `SecureFloat` | `float` |
| `SecureDouble` | `double` |
| `SecureDecimal` | `decimal` |
| `SecureChar` | `char` |
| `SecureRune` | `System.Text.Rune` ¹ |
| `SecureString` | `string` |
| `SecureGuid` | `System.Guid` |
| `SecureDateTime` | `System.DateTime` |
| `SecureDateTimeOffset` | `System.DateTimeOffset` |
| `SecureDateOnly` | `System.DateOnly` ² |
| `SecureTimeOnly` | `System.TimeOnly` ² |
| `SecureTimeSpan` | `System.TimeSpan` |

### .NET numerics (`SecureValue.Numerics`)

| Secure type | Wraps |
|---|---|
| `SecureBigInteger` | `System.Numerics.BigInteger` |
| `SecureComplex` | `System.Numerics.Complex` |
| `SecureMatrix3x2` | `System.Numerics.Matrix3x2` |
| `SecureMatrix4x4` | `System.Numerics.Matrix4x4` |
| `SecurePlane` | `System.Numerics.Plane` |
| `SecureQuaternion` | `System.Numerics.Quaternion` |
| `SecureVector2` | `System.Numerics.Vector2` |
| `SecureVector3` | `System.Numerics.Vector3` |
| `SecureVector4` | `System.Numerics.Vector4` |

### Unity (`SecureValue.Unity`)

| Secure type | Wraps |
|---|---|
| `SecureVector2` | `UnityEngine.Vector2` |
| `SecureVector2Int` | `UnityEngine.Vector2Int` |
| `SecureVector3` | `UnityEngine.Vector3` |
| `SecureVector3Int` | `UnityEngine.Vector3Int` |
| `SecureVector4` | `UnityEngine.Vector4` |
| `SecureRect` | `UnityEngine.Rect` |
| `SecureRectInt` | `UnityEngine.RectInt` |
| `SecureBounds` | `UnityEngine.Bounds` |
| `SecureBoundsInt` | `UnityEngine.BoundsInt` |
| `SecureColor` | `UnityEngine.Color` |
| `SecureColor32` | `UnityEngine.Color32` |
| `SecureQuaternion` | `UnityEngine.Quaternion` |
| `SecureMatrix4x4` | `UnityEngine.Matrix4x4` |
| `SecurePlane` | `UnityEngine.Plane` |
| `SecureRay` | `UnityEngine.Ray` |
| `SecureLayerMask` | `UnityEngine.LayerMask` |

¹ `SecureRune` requires .NET 5 or later. ² `SecureDateOnly` and `SecureTimeOnly` require .NET 6
or later. These wrappers are excluded from the .NET Standard 2.1 and Unity builds.

Use regular collections to group values, such as `List<SecureInt>`.

**Namespace note:** `SecureValue.Numerics` and `SecureValue.Unity` intentionally have matching
names for types such as `SecureVector3`, but each wraps its own framework's type. If a file uses
both families, qualify the wrapper or use aliases:

```csharp
using NumericsSV = SecureValue.Numerics;
using UnitySV = SecureValue.Unity;

NumericsSV.SecureVector3 simulationPosition;
UnitySV.SecureVector3 worldPosition;
```

<a id="compatibility"></a>

## 🔗 Compatibility

| Platform | Support |
|---|---|
| .NET 5.0–10.0 | Full support; the package supplies target-specific builds and all types available on each target. |
| .NET Standard 2.1 | Core and .NET numerics wrappers; excludes `SecureRune`, `SecureDateOnly`, and `SecureTimeOnly`. |
| Unity 2021.3+ | UPM package with core, numerics, Unity value wrappers, serialization, and inspector drawers. `Rune`, `DateOnly`, and `TimeOnly` are unavailable. |

<a id="best-practices"></a>

## ✅ Best practices

### 🎯 Choose what to protect

**Don't encrypt everything.** Focus on values memory editors are likely to target: coins, gems,
health, ammo, speed, aim angles, cooldowns, and progression. Wrapping UI layout data, asset handles,
or a loading-screen timer adds work without meaningful protection.

### ⚡ Keep hot-path work small

**Keep calculations in plain locals.** Decrypt once, do the work, and write the result back:

```csharp
using System;

// Good: one read, local calculation, one write.
int hp = _health;
hp = Math.Max(hp - damage, 0);
_health = hp;

// Wasteful: each operator reads and writes the secured field again.
_health -= damage;
if (_health < 0) _health = 0;
```

### ⚠ Always Initialize Before Reading

Always assign an initial value when declaring a SecureValue.

Attempting to read the decrypted value of an uninitialized instance will throw an
`UninitializedException`, as there was no valid data to decrypt.

```csharp
// BAD: Default instance is uninitialized; calling StackDecrypt or .Value throws UninitializedException!
SecureString token; 

// GOOD: Always assign an initial value upon declaration or setup
SecureString token = "init_token_val";
```

If you need to guard against unassigned fields before accessing them, check the `IsUnset` property:

```csharp
if (token.IsUnset)
{
    // Handle uninitialized state safely
}
```

**Note:** Explicitly checking `IsUnset` is unnecessary in the vast majority of workflows if you
properly initialize your fields, but it is available for edge cases where default struct
initialization or unassigned field states cannot be avoided.

### ⚖ Use Strings and BigInteger Sparingly

`SecureString` and `SecureBigInteger` are the only wrappers that can allocate:
every other type is zero-allocation.

They should generally be avoided in favor of the other types offered by
SecureValue, as they are faster, and non-allocating, but these are provided as
there are genuine scenarios where they might be necessary, such as a save-file
encryption key.

<a id="string-span"></a>

### 🚀 Zero heap-allocation `SecureString` - Span API (Advanced)

Bypass heap allocations by operating directly on stack memory.

Most developers should stick to standard `SecureString` usage on cold paths whenever possible
as they're simpler to work with.

However, if you must use `SecureString` in a hot path—such as updating UI every frame — the
Span API keeps your game loop allocation-free.

📋 **Guidelines:**

- `SecureString` containing <=16 characters produce zero heap allocations, strings >16 characters
  allocate the heap backing array.

- **Initializing with temporary strings causes heap allocations.** Creating a new string to populate
  a SecureString allocates memory on the heap before SecureString even receives it. To stay
  allocation-free, assign from string literals, cached string references, or a `ReadOnlySpan<char>`.

- **Downstream APIs must accept `ReadOnlySpan<char>`.**  This is the stack-materialized format that
  cleans-up fast, and causes zero heap allocations.

- **Do not convert the span, ex; `.ToString()`/`.ToArray()`.** A heap object will materialize,
  causing heap allocations, and GC spikes.  Be careful of third party APIs who convert
  `ReadOnlySpan<char>` to heap objects.

- **Never `stackalloc` In a Loop:** `stackalloc` memory is retained until the outer method returns,
  causing stack overflows.  Hoist your manual buffer outside the loop using `CopyTo`, or let
  `StackDecrypt` manage the scope for you.

- **Avoid Repeated Decryption & Nested Callbacks:** `StackDecrypt` incurs a decryption computation
  every time it is called. If you need to read a SecureString multiple times in a single loop, or
  operate on multiple SecureStrings simultaneously, use a single manual `CopyTo` buffer outside the
  loop to avoid redundant decryption costs, and nested callback clutter.

**1. Manual Zero-Alloc Reads (`CopyTo` / `TryCopyTo`)**

Decrypt directly into a stack-allocated buffer instead of materializing a string object.
Always zero the buffer in a `finally` block so plaintext does not linger on the stack:

```csharp
SecureString questReward = "crown_01";
Span<char> itemName = stackalloc char[questReward.Length];
try
{
    questReward.CopyTo(itemName);
    GivePlayerItem(itemName);
}
finally
{
    CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(itemName));
}

void GivePlayerItem(ReadOnlySpan<char> itemName)
{
    if (itemName.SequenceEqual("crown_01")) GrantCrown();
}
```

**2. Zero-Alloc Writes (span assignment)**

Encrypt directly from characters without creating an intermediate string instance:

```csharp
// Inbound: player name arrives as raw chars in a packet buffer.
void OnPlayerJoined(ReadOnlySpan<char> nameChars)
{
    SecureString playerName = nameChars;
    CreatePlayerPrefab(playerName);
}
```

**3. Zero-Copy Passing (`in` / `ref readonly`)**

To prevent copying the 112-byte struct across function calls or local assignments,
use `in` or `ref readonly` to pass and reference instances via an 8-byte stack reference:

```csharp
SecureString equippedWeaponId = "wpn_excalibur_01";

void ExecuteAttack()
{
    // Passes an 8-byte reference into the method instead of copying 112 bytes
    enemy.Health -= GetItemDamage(in equippedWeaponId);
}

int GetItemDamage(in SecureString itemId)
{
    if (itemId.SequenceEqual("wpn_excalibur_01"))
        return 50;
    return 0;
}
```

**4. Managed Stack Buffers (`StackDecrypt` / `TryStackDecrypt`)**

Manual stack decryption requires writing repetitive boilerplate to allocate memory, wrap calls in
try/finally blocks, and clear plaintext data.

`StackDecrypt` handles the entire buffer lifecycle automatically: it creates a stack buffer,
decrypts into it, passes the span to your callback, and guarantees memory zeroing before
returning.

Using static lambdas ensures the delegate is cached, avoiding repeated heap allocations.

```csharp
weaponId.StackDecrypt(static id =>
{
    if (id.SequenceEqual("wpn_excalibur_01")) ExecuteAttack();
});
```

Since static lambdas cannot capture local scope, stateful overloads let you pass local variables
directly into the signature.

The state is passed as a value parameter: `expected`, so the callback doesn't need to capture a
closure, preserving zero heap-allocation execution.

```csharp
int damage = weaponId.StackDecrypt("wpn_excalibur_01", static (id, expected) =>
    id.SequenceEqual(expected) ? 50 : 10);
```

To pass multiple local variables, bundle them into a `ValueTuple`.

```csharp
var state = (expected: "wpn_excalibur_01", bonus: 10);

int damage = weaponId.StackDecrypt(state, static (id, s) =>
    id.SequenceEqual(s.expected) ? 50 + s.bonus : 10);
```

### 🛟 TryDecrypt: Non-Throwing Reads

Throwing on tamper stays the default because a forged value must not flow into game logic.
`TryDecrypt` exists for callers who prefer to branch on failure themselves.

Every wrapper has an optional `bool TryDecrypt(out T value)` method: it returns `true` with the decrypted
value on success and `false` with `value` set to the type default when the read fails — an uninitialized
value, or a value whose both copies are tampered. A singly-tampered value returns `true` with the recovered 
value (the event still fires).

**Code example:**

```csharp
if (bossHp.TryDecrypt(out int hp))
{
    // Use hp.
}
else
{
    // Never-assigned or doubly-tampered: reset, re-fetch, or abort.
}
```

There is also a performance reason to prefer this over **manually wrapping** `.Decrypted` in a try/catch block: 
`TryDecrypt` verifies, and returns `false` **directly**, so the tampered path doesn't pay the cost of exception unwinding.

<a id="samples"></a>

## 🧪 Samples

- [`SecureValue.Sample`](https://github.com/kdserra/SecureValue/tree/master/SecureValue.Sample) —
  console sample covering wrappers, conversions, operators, and tamper detection. Run it with
  `dotnet run --project SecureValue.Sample`.
- [`SecureValue.Benchmarks`](https://github.com/kdserra/SecureValue/tree/master/SecureValue.Benchmarks) —
  BenchmarkDotNet suites for primitive baselines, wrapper read/write, arithmetic, comparisons,
  and string allocations. Run with
  `dotnet run -c Release --project SecureValue.Benchmarks`.
- [Unity benchmark harness](https://github.com/kdserra/SecureValue/tree/master/SecureValue.Unity/Runtime/Samples/Benchmark)
  — compares plain primitives and secured wrappers in the Unity Editor or player.
- [Unity demo](https://github.com/kdserra/SecureValue/tree/master/SecureValue.Unity/Runtime/Samples/SecureValueDemo.cs)
  — demonstrates the core, numerics, Unity wrapper families, and provides menu-item utilities for manual testing.
  
The pre-compiled sample binaries are available in the [GitHub Releases](https://github.com/kdserra/SecureValue/releases).

<a id="benchmark-results"></a>

## 📊 Benchmark Results

The results below are examples from one machine and configuration, not universal guarantees.
Unity performance varies by platform, build type, scripting backend, and IL2CPP settings. Measure
in the release build and hardware you plan to ship. In these runs, primitive operations had no
measured managed allocations and completed in nanoseconds to tens of nanoseconds; string costs
and allocations grew with the value length.

**Test PC:** Intel i7-9700K (3.6 GHz base, 4.7 GHz boost), RTX 4070 Super, 32 GB DDR4 at 2133 MHz.

Benchmark sources:

- [Unity benchmark code](https://github.com/kdserra/SecureValue/tree/master/SecureValue.Unity/Runtime/Samples/Benchmark)
- [.NET BenchmarkDotNet code](https://github.com/kdserra/SecureValue/tree/master/SecureValue.Benchmarks)

Per-release .NET results (`Benchmark-IntFloatString.md`, `Benchmark-All.md`) are attached to each [GitHub Release](https://github.com/kdserra/SecureValue/releases).

### Unity Editor

```text
[BenchmarkRunner] === C# Primitive ===
[Warmup] Warmup: 1,000 ops in 0.04 ms | 42.7 ns/op | ~0.0 B/op
[C# Primitive] WriteInt: 1,000,000 ops in 2.88 ms | 2.9 ns/op | ~0.0 B/op
[C# Primitive] ReadInt: 1,000,000 ops in 2.56 ms | 2.6 ns/op | ~0.0 B/op
[C# Primitive] WriteFloat: 1,000,000 ops in 5.56 ms | 5.6 ns/op | ~0.0 B/op
[C# Primitive] ReadFloat: 1,000,000 ops in 3.63 ms | 3.6 ns/op | ~0.0 B/op
[C# Primitive] WriteString: 100,000 ops in 1.20 ms | 12.0 ns/op | ~0.0 B/op
[C# Primitive] ReadString: 100,000 ops in 0.39 ms | 3.9 ns/op | ~0.0 B/op

[BenchmarkRunner] === SecureValue ===
[Warmup] Warmup: 1,000 ops in 0.06 ms | 63.8 ns/op | ~0.0 B/op
[SecureValue] WriteInt: 1,000,000 ops in 60.59 ms | 60.6 ns/op | ~0.0 B/op
[SecureValue] ReadInt: 1,000,000 ops in 24.32 ms | 24.3 ns/op | ~0.0 B/op
[SecureValue] WriteFloat: 1,000,000 ops in 66.37 ms | 66.4 ns/op | ~0.0 B/op
[SecureValue] ReadFloat: 1,000,000 ops in 38.91 ms | 38.9 ns/op | ~0.0 B/op
[SecureValue] WriteString: 100,000 ops in 64.23 ms | 642.3 ns/op | ~251.5 B/op
[SecureValue] ReadString: 100,000 ops in 39.60 ms | 396.0 ns/op | ~103.8 B/op

[BenchmarkRunner] All benchmarks complete.
```

### Unity Windows Player — IL2CPP

```text
[BenchmarkRunner] === C# Primitive ===
[Warmup] Warmup: 1,000 ops in 0.00 ms | 4.0 ns/op | ~0.0 B/op
[C# Primitive] WriteInt: 1,000,000 ops in 3.84 ms | 3.8 ns/op | ~0.0 B/op
[C# Primitive] ReadInt: 1,000,000 ops in 3.87 ms | 3.9 ns/op | ~0.0 B/op
[C# Primitive] WriteFloat: 1,000,000 ops in 2.99 ms | 3.0 ns/op | ~0.0 B/op
[C# Primitive] ReadFloat: 1,000,000 ops in 3.71 ms | 3.7 ns/op | ~0.0 B/op
[C# Primitive] WriteString: 100,000 ops in 0.82 ms | 8.2 ns/op | ~0.0 B/op
[C# Primitive] ReadString: 100,000 ops in 0.40 ms | 4.0 ns/op | ~0.0 B/op

[BenchmarkRunner] === SecureValue ===
[Warmup] Warmup: 1,000 ops in 0.07 ms | 68.6 ns/op | ~0.0 B/op
[SecureValue] WriteInt: 1,000,000 ops in 67.22 ms | 67.2 ns/op | ~0.0 B/op
[SecureValue] ReadInt: 1,000,000 ops in 23.39 ms | 23.4 ns/op | ~0.0 B/op
[SecureValue] WriteFloat: 1,000,000 ops in 66.00 ms | 66.0 ns/op | ~0.0 B/op
[SecureValue] ReadFloat: 1,000,000 ops in 23.72 ms | 23.7 ns/op | ~0.0 B/op
[SecureValue] WriteString: 100,000 ops in 57.61 ms | 576.1 ns/op | ~2.9 B/op
[SecureValue] ReadString: 100,000 ops in 33.21 ms | 332.1 ns/op | ~5.4 B/op

[BenchmarkRunner] All benchmarks complete.
```

### .NET 10 — BenchmarkDotNet (`IntFloatStringBenchmarks`)

```text
| Method                  | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------------------------ |----------:|---------:|---------:|-------:|----------:|
| Int_Write               |  26.37 ns | 0.077 ns | 0.068 ns |      - |         - |
| Int_Read                |  13.86 ns | 0.063 ns | 0.059 ns |      - |         - |
| Int_Arithmetic          |  98.56 ns | 0.941 ns | 0.880 ns |      - |         - |
| Int_Compare             |  14.63 ns | 0.053 ns | 0.044 ns |      - |         - |
| Float_Write             |  25.29 ns | 0.080 ns | 0.071 ns |      - |         - |
| Float_Read              |  14.01 ns | 0.007 ns | 0.006 ns |      - |         - |
| Float_Arithmetic        |  99.20 ns | 0.631 ns | 0.590 ns |      - |         - |
| Float_Compare           |  14.62 ns | 0.014 ns | 0.011 ns |      - |         - |
| String_Write_Short      |  69.63 ns | 0.162 ns | 0.135 ns |      - |         - |
| String_Write_Long       | 354.29 ns | 1.559 ns | 1.458 ns | 0.0429 |     272 B |
| String_Read_Short       |  84.32 ns | 0.145 ns | 0.113 ns | 0.0063 |      40 B |
| String_Read_Long        | 233.37 ns | 2.026 ns | 1.796 ns | 0.0215 |     136 B |
| String_Equality_Short   | 254.70 ns | 1.165 ns | 1.032 ns | 0.0124 |      80 B |
| String_Read_Short_Span  |  66.37 ns | 0.216 ns | 0.191 ns |      - |         - |
| String_Read_Long_Span   | 206.85 ns | 0.526 ns | 0.466 ns |      - |         - |
| String_Write_Span_Short |  83.74 ns | 0.237 ns | 0.198 ns |      - |         - |
| String_Write_Span_Long  | 382.18 ns | 0.753 ns | 0.705 ns | 0.0429 |     272 B |
```

<a id="security-recommendations"></a>

## 🔐 Security Recommendations

No client-side protection is perfect. SecureValue is one layer in a defense-in-depth approach, and
it works best alongside protections for other attack paths.

Server authority is the preferred way to protect important game state: let a trusted server
validate outcomes such as purchases, currency, and progression. Some threats remain client-side,
though. For example, server authority alone does not prevent an aimbot; protecting aim-related
values may make one harder to build or help detect its use.

### Unity projects

- **IL2CPP scripting backend** — Ahead-of-time compiled native code is harder to inspect and
  modify than managed assemblies: [Unity IL2CPP Documentation](https://docs.unity3d.com/6000.1/Documentation/Manual/scripting-backends-il2cpp.html).
- **Source obfuscation** — [Obfuz](https://github.com/focus-creative-games/obfuz/blob/main/README-EN.md)
  is a free, open-source option offers symbol renaming, string encryption, and control-flow obfuscation for Unity projects.
- **Metadata encryption** — [Mfuscator](https://mfuscator.com/) encrypts IL2CPP metadata to
  prevent automatic dumping of your code which bad actors rely on to build cheats for your game.

### .NET projects

- **NativeAOT** — Ahead-of-time compiled native code is harder to inspect and
  modify than managed assemblies: [Microsoft's NativeAOT guidance](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).
- **Source obfuscation** — [Obfuscar](https://github.com/obfuscar/obfuscar)
  is a free, open-source option offers symbol renaming, string encryption, and control-flow obfuscation for .NET projects.
- **VMProtect** — [VMProtect](https://vmprotect.com/) provides code virtualization and mutation to increase resistance to decompilation and static analysis.

<a id="license"></a>

## 📄 License

SecureValue is released under the [MIT License](https://github.com/kdserra/SecureValue/blob/master/LICENSE.md).
