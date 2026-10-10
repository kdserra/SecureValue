using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SecureValue;
using SecureValue.Numerics;
using UnityEngine;

/// <summary>
/// Demonstrates every Secure* type that Unity can serialize.
/// All fields below are [SerializeField]: editable in the Inspector in the
/// editor (via custom PropertyDrawers that decrypt for display and
/// re-encrypt on edit), stored in scenes/prefabs as encrypted words, and
/// tamper-checked on every read at runtime. Fresh (never-set) fields read as
/// the type default; only tampered memory throws.
/// Note: SecureDateOnly / SecureTimeOnly / SecureRune are compiled
/// out of Unity (#if NET) and are therefore not demonstrated here.
/// The SecureValue.Numerics types are part of the core package and are
/// always demonstrated (no opt-in needed).
/// The Unity-specific (SecureValue.Unity) and Numerics (SecureValue.Numerics)
/// families share simple type names (e.g. SecureVector2 exists in both), so every
/// field below from those two families is FULLY QUALIFIED — never rely on usings here.
/// The "Fill Test Values" context menu on this component assigns a known constant
/// to every field (verifying each one decrypts back immediately). The constants
/// persist in the Inspector, so saving/reloading the scene gives a quick visual
/// confirmation that serialization round-trips every supported type. The separate
/// "Validate Test Values" menu re-checks the same constants without assigning:
/// standard workflow is Fill, Validate, save + close Unity, reopen, Validate again
/// (still green means persistence round-tripped every supported type).
/// The "Save to JSON" menu additionally records every field's decrypted value to
/// Assets/saved-values.json; "Validate JSON" compares the live fields against that
/// record, so deviation across editor sessions is caught with a per-field message.
/// The "Fill with Random Values" menu assigns fresh random values instead of the
/// constants (proving persistence with non-constant data via the JSON menus);
/// run Fill again afterwards to restore the constants.
/// Test constants deliberately avoid 0 and type min/max values: a decrypt failure
/// that resolves to 0 would be invisible against a 0 constant. Dedicated extreme
/// (lowest/highest/zero) coverage lives in the test suites instead.
/// </summary>
public class SecureValueDemo : MonoBehaviour
{
	[Header("Player State")]
	[SerializeField]
	private SecureInt playerHealth;

	[SerializeField]
	private SecureUInt totalScore;

	[SerializeField]
	private SecureFloat movementSpeed;

	[SerializeField]
	private SecureString playerName;

	[SerializeField]
	private SecureChar playerRankInitial;

	[SerializeField]
	private SecureGuid playerAccountId;

	[Header("Economy")]
	[SerializeField]
	private SecureDecimal walletBalance;

	[SerializeField]
	private SecureLong lifetimeExperiencePoints;

	[SerializeField]
	private SecureULong globalLeaderboardScore;

	[SerializeField]
	private SecureUShort enemyKillCount;

	[SerializeField]
	private SecureShort ammoInMagazine;

	[SerializeField]
	private SecureByte currentPlayerLevel;

	[Header("Game Options")]
	[SerializeField]
	private SecureBool isGodModeEnabled;

	[SerializeField]
	private SecureSByte difficultyLevel;

	[SerializeField]
	private SecureDouble criticalHitDamageMultiplier;

	[Header("World & Scheduling")]
	[SerializeField]
	private SecureDateTimeOffset eventActivationDate;

	[SerializeField]
	private SecureDateTime lastLoginTimeUtc;

	[SerializeField]
	private SecureTimeSpan activeBoostDuration;

	[Header("Unity")]
	[SerializeField]
	private SecureValue.Unity.SecureVector2 uiPosition;

	[SerializeField]
	private SecureValue.Unity.SecureVector2Int tileCoordinate;

	[SerializeField]
	private SecureValue.Unity.SecureVector3 respawnPoint;

	[SerializeField]
	private SecureValue.Unity.SecureVector3Int gridCell;

	[SerializeField]
	private SecureValue.Unity.SecureVector4 spellEffectParameters;

	[SerializeField]
	private SecureValue.Unity.SecureRect minimapViewport;

	[SerializeField]
	private SecureValue.Unity.SecureRectInt inventorySlotArea;

	[SerializeField]
	private SecureValue.Unity.SecureBounds arenaBounds;

	[SerializeField]
	private SecureValue.Unity.SecureBoundsInt buildZone;

	[SerializeField]
	private SecureValue.Unity.SecureColor teamColor;

	[SerializeField]
	private SecureValue.Unity.SecureColor32 minimapPixel;

	[SerializeField]
	private SecureValue.Unity.SecureQuaternion doorRotation;

	[SerializeField]
	private SecureValue.Unity.SecureMatrix4x4 portalTransform;

	[SerializeField]
	private SecureValue.Unity.SecurePlane waterPlaneHeight;

	[SerializeField]
	private SecureValue.Unity.SecureRay aimRay;

	[SerializeField]
	private SecureValue.Unity.SecureLayerMask enemyCollisionLayers;

	[Header("Numerics")]
	[SerializeField]
	private SecureValue.Numerics.SecureVector2 joystickInput;

	[SerializeField]
	private SecureValue.Numerics.SecureVector3 spawnPointPosition;

	[SerializeField]
	private SecureValue.Numerics.SecureVector4 shaderEffectParameters;

	[SerializeField]
	private SecureValue.Numerics.SecureQuaternion cameraRotation;

	[SerializeField]
	private SecureValue.Numerics.SecurePlane groundCollisionPlane;

	[SerializeField]
	private SecureValue.Numerics.SecureMatrix3x2 uiTransformMatrix;

	[SerializeField]
	private SecureValue.Numerics.SecureMatrix4x4 characterRigTransformMatrix;

	[SerializeField]
	private SecureValue.Numerics.SecureComplex signalProcessingValue;

	[SerializeField]
	private SecureValue.Numerics.SecureBigInteger puzzleSolutionNumber;

	/// <summary>
	/// Unity component reset hook: restores every secured field to the default value
	/// of its underlying primitive or Unity/BCL value type.
	/// </summary>
	private void Reset()
	{
#if UNITY_EDITOR
		UnityEditor.Undo.RecordObject(this, "Reset SecureValue Demo");
#endif
		playerHealth = default(int);
		totalScore = default(uint);
		movementSpeed = default(float);
		playerName = default(string);
		playerRankInitial = default(char);
		playerAccountId = default(Guid);
		walletBalance = default(decimal);
		lifetimeExperiencePoints = default(long);
		globalLeaderboardScore = default(ulong);
		enemyKillCount = default(ushort);
		ammoInMagazine = default(short);
		currentPlayerLevel = default(byte);
		isGodModeEnabled = default(bool);
		difficultyLevel = default(sbyte);
		criticalHitDamageMultiplier = default(double);
		eventActivationDate = default(DateTimeOffset);
		lastLoginTimeUtc = default(DateTime);
		activeBoostDuration = default(TimeSpan);
		uiPosition = default(Vector2);
		tileCoordinate = default(Vector2Int);
		respawnPoint = default(Vector3);
		gridCell = default(Vector3Int);
		spellEffectParameters = default(Vector4);
		minimapViewport = default(Rect);
		inventorySlotArea = default(RectInt);
		arenaBounds = default(Bounds);
		buildZone = default(BoundsInt);
		teamColor = default(Color);
		minimapPixel = default(Color32);
		doorRotation = default(Quaternion);
		portalTransform = default(Matrix4x4);
		waterPlaneHeight = default(Plane);
		aimRay = default(Ray);
		enemyCollisionLayers = default(LayerMask);
		joystickInput = default(System.Numerics.Vector2);
		spawnPointPosition = default(System.Numerics.Vector3);
		shaderEffectParameters = default(System.Numerics.Vector4);
		cameraRotation = default(System.Numerics.Quaternion);
		groundCollisionPlane = default(System.Numerics.Plane);
		uiTransformMatrix = default(System.Numerics.Matrix3x2);
		characterRigTransformMatrix = default(System.Numerics.Matrix4x4);
		signalProcessingValue = default(System.Numerics.Complex);
		puzzleSolutionNumber = default(System.Numerics.BigInteger);
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif
	}

	private void Start()
	{
		// Reads decrypt + tamper-check on every access.
		// Logged in field-declaration (Unity serialization) order.
		Debug.Log($"playerHealth: {playerHealth}");
		Debug.Log($"totalScore: {totalScore}");
		Debug.Log($"movementSpeed: {movementSpeed}");
		Debug.Log($"playerName: {playerName}");
		Debug.Log($"playerRankInitial: {playerRankInitial}");
		Debug.Log($"playerAccountId: {playerAccountId}");
		Debug.Log($"walletBalance: {walletBalance}");
		Debug.Log($"lifetimeExperiencePoints: {lifetimeExperiencePoints}");
		Debug.Log($"globalLeaderboardScore: {globalLeaderboardScore}");
		Debug.Log($"enemyKillCount: {enemyKillCount}");
		Debug.Log($"ammoInMagazine: {ammoInMagazine}");
		Debug.Log($"currentPlayerLevel: {currentPlayerLevel}");
		Debug.Log($"isGodModeEnabled: {isGodModeEnabled}");
		Debug.Log($"difficultyLevel: {difficultyLevel}");
		Debug.Log($"criticalHitDamageMultiplier: {criticalHitDamageMultiplier}");
		Debug.Log($"eventActivationDate: {eventActivationDate}");
		Debug.Log($"lastLoginTimeUtc: {lastLoginTimeUtc}");
		Debug.Log($"activeBoostDuration: {activeBoostDuration}");
		Debug.Log($"uiPosition: {uiPosition}");
		Debug.Log($"tileCoordinate: {tileCoordinate}");
		Debug.Log($"respawnPoint: {respawnPoint}");
		Debug.Log($"gridCell: {gridCell}");
		Debug.Log($"spellEffectParameters: {spellEffectParameters}");
		Debug.Log($"minimapViewport: {minimapViewport}");
		Debug.Log($"inventorySlotArea: {inventorySlotArea}");
		Debug.Log($"arenaBounds: {arenaBounds}");
		Debug.Log($"buildZone: {buildZone}");
		Debug.Log($"teamColor: {teamColor}");
		Debug.Log($"minimapPixel: {minimapPixel}");
		Debug.Log($"doorRotation: {doorRotation}");
		Debug.Log($"portalTransform: {portalTransform}");
		Debug.Log($"waterPlaneHeight: {waterPlaneHeight}");
		Debug.Log($"aimRay: {aimRay}");
		Debug.Log($"enemyCollisionLayers: {enemyCollisionLayers}");
		Debug.Log($"joystickInput: {joystickInput}");
		Debug.Log($"spawnPointPosition: {spawnPointPosition}");
		Debug.Log($"shaderEffectParameters: {shaderEffectParameters}");
		Debug.Log($"cameraRotation: {cameraRotation}");
		Debug.Log($"groundCollisionPlane: {groundCollisionPlane}");
		Debug.Log($"uiTransformMatrix: {uiTransformMatrix}");
		Debug.Log($"characterRigTransformMatrix: {characterRigTransformMatrix}");
		Debug.Log($"signalProcessingValue: {signalProcessingValue}");
		Debug.Log($"puzzleSolutionNumber: {puzzleSolutionNumber}");
	}

	/// <summary>
	/// Inspector test hook (component context menu): assigns a known constant to every
	/// serialized field, decrypts it straight back, and reports any value that does not
	/// match. The constants stay in the fields, so the Inspector shows them (drawers
	/// decrypt the serialized form) and saving/reloading the scene visibly confirms
	/// every supported type survives serialization without checking values en masse.
	/// </summary>
	[ContextMenu("Fill Test Values")]
	private void FillTestValues()
	{
		int checks = 0;
		int failures = 0;
#if UNITY_EDITOR
		// Direct field assignments do not mark the scene dirty, so without this
		// the fill would silently vanish on close/restart (Validate passes
		// in-session either way, which hides the loss until the next session).
		UnityEditor.Undo.RecordObject(this, "Fill Test Values");
#endif

		playerHealth = 100;
		Check("playerHealth", (int)playerHealth, 100, ref checks, ref failures);
		totalScore = 250000u;
		Check("totalScore", (uint)totalScore, 250000u, ref checks, ref failures);
		movementSpeed = 6.5f;
		Check("movementSpeed", (float)movementSpeed, 6.5f, ref checks, ref failures);
		playerName = "SecureValue";
		Check("playerName", (string)playerName, "SecureValue", ref checks, ref failures);
		playerRankInitial = 'S';
		Check("playerRankInitial", (char)playerRankInitial, 'S', ref checks, ref failures);
		playerAccountId = new Guid("11111111-2222-3333-4444-555555555555");
		Check(
			"playerAccountId",
			(Guid)playerAccountId,
			new Guid("11111111-2222-3333-4444-555555555555"),
			ref checks,
			ref failures
		);
		walletBalance = 1234.56m;
		Check("walletBalance", (decimal)walletBalance, 1234.56m, ref checks, ref failures);
		lifetimeExperiencePoints = 123456789012345L;
		Check(
			"lifetimeExperiencePoints",
			(long)lifetimeExperiencePoints,
			123456789012345L,
			ref checks,
			ref failures
		);
		globalLeaderboardScore = 9876543210UL;
		Check(
			"globalLeaderboardScore",
			(ulong)globalLeaderboardScore,
			9876543210UL,
			ref checks,
			ref failures
		);
		enemyKillCount = 1234;
		Check("enemyKillCount", (ushort)enemyKillCount, (ushort)1234, ref checks, ref failures);
		ammoInMagazine = 30;
		Check("ammoInMagazine", (short)ammoInMagazine, (short)30, ref checks, ref failures);
		currentPlayerLevel = 7;
		Check("currentPlayerLevel", (byte)currentPlayerLevel, (byte)7, ref checks, ref failures);
		isGodModeEnabled = true;
		Check("isGodModeEnabled", (bool)isGodModeEnabled, true, ref checks, ref failures);
		difficultyLevel = 2;
		Check("difficultyLevel", (sbyte)difficultyLevel, (sbyte)2, ref checks, ref failures);
		criticalHitDamageMultiplier = 1.5;
		Check(
			"criticalHitDamageMultiplier",
			(double)criticalHitDamageMultiplier,
			1.5,
			ref checks,
			ref failures
		);
		eventActivationDate = new DateTimeOffset(2026, 12, 31, 23, 59, 0, TimeSpan.FromHours(2));
		Check(
			"eventActivationDate",
			(DateTimeOffset)eventActivationDate,
			new DateTimeOffset(2026, 12, 31, 23, 59, 0, TimeSpan.FromHours(2)),
			ref checks,
			ref failures
		);
		lastLoginTimeUtc = new DateTime(2026, 9, 24, 12, 30, 0, DateTimeKind.Utc);
		Check(
			"lastLoginTimeUtc",
			(DateTime)lastLoginTimeUtc,
			new DateTime(2026, 9, 24, 12, 30, 0, DateTimeKind.Utc),
			ref checks,
			ref failures
		);
		activeBoostDuration = new TimeSpan(1, 37, 42);
		Check(
			"activeBoostDuration",
			(TimeSpan)activeBoostDuration,
			new TimeSpan(1, 37, 42),
			ref checks,
			ref failures
		);

		uiPosition = new Vector2(12.5f, -3.25f);
		Check(
			"uiPosition",
			(Vector2)uiPosition,
			new Vector2(12.5f, -3.25f),
			ref checks,
			ref failures
		);
		tileCoordinate = new Vector2Int(7, -3);
		Check(
			"tileCoordinate",
			(Vector2Int)tileCoordinate,
			new Vector2Int(7, -3),
			ref checks,
			ref failures
		);
		respawnPoint = new Vector3(1f, 2f, 3f);
		Check(
			"respawnPoint",
			(Vector3)respawnPoint,
			new Vector3(1f, 2f, 3f),
			ref checks,
			ref failures
		);
		gridCell = new Vector3Int(1, -2, 3);
		Check("gridCell", (Vector3Int)gridCell, new Vector3Int(1, -2, 3), ref checks, ref failures);
		spellEffectParameters = new Vector4(1f, 2f, 3f, 4f);
		Check(
			"spellEffectParameters",
			(Vector4)spellEffectParameters,
			new Vector4(1f, 2f, 3f, 4f),
			ref checks,
			ref failures
		);
		minimapViewport = new Rect(10f, 20f, 100f, 50f);
		Check(
			"minimapViewport",
			(Rect)minimapViewport,
			new Rect(10f, 20f, 100f, 50f),
			ref checks,
			ref failures
		);
		inventorySlotArea = new RectInt(1, 2, 10, 20);
		Check(
			"inventorySlotArea",
			(RectInt)inventorySlotArea,
			new RectInt(1, 2, 10, 20),
			ref checks,
			ref failures
		);
		arenaBounds = new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f));
		Check(
			"arenaBounds",
			(Bounds)arenaBounds,
			new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f)),
			ref checks,
			ref failures
		);
		buildZone = new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6));
		Check(
			"buildZone",
			(BoundsInt)buildZone,
			new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6)),
			ref checks,
			ref failures
		);
		teamColor = new Color(0.15f, 0.5f, 0.85f, 0.75f);
		Check(
			"teamColor",
			(Color)teamColor,
			new Color(0.15f, 0.5f, 0.85f, 0.75f),
			ref checks,
			ref failures
		);
		minimapPixel = new Color32(237, 129, 64, 210);
		Check(
			"minimapPixel",
			(Color32)minimapPixel,
			new Color32(237, 129, 64, 210),
			ref checks,
			ref failures
		);
		doorRotation = Quaternion.Euler(10f, 20f, 30f);
		Check(
			"doorRotation",
			(Quaternion)doorRotation,
			Quaternion.Euler(10f, 20f, 30f),
			ref checks,
			ref failures
		);
		portalTransform = Matrix4x4.TRS(
			new Vector3(1f, 2f, 3f),
			Quaternion.Euler(15f, 30f, 45f),
			new Vector3(2.5f, 0.5f, 1.75f)
		);
		Check(
			"portalTransform",
			(Matrix4x4)portalTransform,
			Matrix4x4.TRS(
				new Vector3(1f, 2f, 3f),
				Quaternion.Euler(15f, 30f, 45f),
				new Vector3(2.5f, 0.5f, 1.75f)
			),
			ref checks,
			ref failures
		);
		waterPlaneHeight = new Plane(new Vector3(0.25f, 0.9f, 0.35f), 5.25f);
		Check(
			"waterPlaneHeight",
			(Plane)waterPlaneHeight,
			new Plane(new Vector3(0.25f, 0.9f, 0.35f), 5.25f),
			ref checks,
			ref failures
		);
		aimRay = new Ray(new Vector3(1f, 2f, 3f), new Vector3(0.36f, 0.48f, 0.8f));
		Check(
			"aimRay",
			(Ray)aimRay,
			new Ray(new Vector3(1f, 2f, 3f), new Vector3(0.36f, 0.48f, 0.8f)),
			ref checks,
			ref failures
		);
		enemyCollisionLayers = new LayerMask() { value = 5 };
		Check(
			"enemyCollisionLayers",
			((LayerMask)enemyCollisionLayers).value,
			5,
			ref checks,
			ref failures
		);

		joystickInput = new System.Numerics.Vector2(1.5f, -2.5f);
		Check(
			"joystickInput",
			(System.Numerics.Vector2)joystickInput,
			new System.Numerics.Vector2(1.5f, -2.5f),
			ref checks,
			ref failures
		);
		spawnPointPosition = new System.Numerics.Vector3(1f, 2f, 3f);
		Check(
			"spawnPointPosition",
			(System.Numerics.Vector3)spawnPointPosition,
			new System.Numerics.Vector3(1f, 2f, 3f),
			ref checks,
			ref failures
		);
		shaderEffectParameters = new System.Numerics.Vector4(1f, 2f, 3f, 4f);
		Check(
			"shaderEffectParameters",
			(System.Numerics.Vector4)shaderEffectParameters,
			new System.Numerics.Vector4(1f, 2f, 3f, 4f),
			ref checks,
			ref failures
		);
		cameraRotation = new System.Numerics.Quaternion(0.27f, 0.36f, 0.48f, 0.76f);
		Check(
			"cameraRotation",
			(System.Numerics.Quaternion)cameraRotation,
			new System.Numerics.Quaternion(0.27f, 0.36f, 0.48f, 0.76f),
			ref checks,
			ref failures
		);
		groundCollisionPlane = new System.Numerics.Plane(0.25f, 0.9f, 0.35f, 5.25f);
		Check(
			"groundCollisionPlane",
			(System.Numerics.Plane)groundCollisionPlane,
			new System.Numerics.Plane(0.25f, 0.9f, 0.35f, 5.25f),
			ref checks,
			ref failures
		);
		uiTransformMatrix = new System.Numerics.Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
		Check(
			"uiTransformMatrix",
			(System.Numerics.Matrix3x2)uiTransformMatrix,
			new System.Numerics.Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f),
			ref checks,
			ref failures
		);
		characterRigTransformMatrix = new System.Numerics.Matrix4x4(
			1f,
			2f,
			3f,
			4f,
			5f,
			6f,
			7f,
			8f,
			9f,
			10f,
			11f,
			12f,
			13f,
			14f,
			15f,
			16f
		);
		Check(
			"characterRigTransformMatrix",
			(System.Numerics.Matrix4x4)characterRigTransformMatrix,
			new System.Numerics.Matrix4x4(
				1f,
				2f,
				3f,
				4f,
				5f,
				6f,
				7f,
				8f,
				9f,
				10f,
				11f,
				12f,
				13f,
				14f,
				15f,
				16f
			),
			ref checks,
			ref failures
		);
		signalProcessingValue = new System.Numerics.Complex(1.5, -2.5);
		Check(
			"signalProcessingValue",
			(System.Numerics.Complex)signalProcessingValue,
			new System.Numerics.Complex(1.5, -2.5),
			ref checks,
			ref failures
		);
		puzzleSolutionNumber = System.Numerics.BigInteger.Parse("123456789012345678901234567890");
		Check(
			"puzzleSolutionNumber",
			(System.Numerics.BigInteger)puzzleSolutionNumber,
			System.Numerics.BigInteger.Parse("123456789012345678901234567890"),
			ref checks,
			ref failures
		);
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif

		Debug.Log(
			$"[SecureValueDemo] Round-trip check complete: {checks - failures}/{checks} values match their expected constants."
		);
	}

	/// <summary>
	/// Inspector test hook (component context menu): assigns a fresh random value to
	/// every serialized field and verifies each one decrypts back immediately. These
	/// are NOT the Fill Test Values constants, so Validate Test Values will report
	/// mismatches afterwards until Fill is run again — use Save to JSON before
	/// restarting and Validate JSON after to prove the random values persisted.
	/// </summary>
	[ContextMenu("Fill with Random Values")]
	private void FillWithRandomValues()
	{
		int checks = 0;
		int failures = 0;
#if UNITY_EDITOR
		UnityEditor.Undo.RecordObject(this, "Fill with Random Values");
#endif

		int health =
			UnityEngine.Random.Range(1, 1000000) * (UnityEngine.Random.value < 0.5f ? -1 : 1);
		playerHealth = health;
		Check("playerHealth", (int)playerHealth, health, ref checks, ref failures);
		uint score = (uint)UnityEngine.Random.Range(1, int.MaxValue);
		totalScore = score;
		Check("totalScore", (uint)totalScore, score, ref checks, ref failures);
		float speed = UnityEngine.Random.Range(-100f, 100f);
		movementSpeed = speed;
		Check("movementSpeed", (float)movementSpeed, speed, ref checks, ref failures);
		string playerNameValue = RandomString(UnityEngine.Random.Range(1, 25));
		playerName = playerNameValue;
		Check("playerName", (string)playerName, playerNameValue, ref checks, ref failures);
		char rank = (char)UnityEngine.Random.Range(33, 127);
		playerRankInitial = rank;
		Check("playerRankInitial", (char)playerRankInitial, rank, ref checks, ref failures);
		Guid accountId = Guid.NewGuid();
		playerAccountId = accountId;
		Check("playerAccountId", (Guid)playerAccountId, accountId, ref checks, ref failures);
		decimal balance = (decimal)UnityEngine.Random.Range(-999999f, 999999f);
		if (balance == 0m)
		{
			balance = 123.45m;
		}
		walletBalance = balance;
		Check("walletBalance", (decimal)walletBalance, balance, ref checks, ref failures);
		long experience =
			(long)UnityEngine.Random.Range(1, int.MaxValue) * UnityEngine.Random.Range(1, 100000);
		lifetimeExperiencePoints = experience;
		Check(
			"lifetimeExperiencePoints",
			(long)lifetimeExperiencePoints,
			experience,
			ref checks,
			ref failures
		);
		ulong leaderboard =
			(ulong)UnityEngine.Random.Range(1, int.MaxValue)
			* (ulong)UnityEngine.Random.Range(1, 100000);
		globalLeaderboardScore = leaderboard;
		Check(
			"globalLeaderboardScore",
			(ulong)globalLeaderboardScore,
			leaderboard,
			ref checks,
			ref failures
		);
		ushort kills = (ushort)UnityEngine.Random.Range(1, ushort.MaxValue);
		enemyKillCount = kills;
		Check("enemyKillCount", (ushort)enemyKillCount, kills, ref checks, ref failures);
		short ammo = (short)(
			UnityEngine.Random.Range(1, short.MaxValue) * (UnityEngine.Random.value < 0.5f ? -1 : 1)
		);
		ammoInMagazine = ammo;
		Check("ammoInMagazine", (short)ammoInMagazine, ammo, ref checks, ref failures);
		byte level = (byte)UnityEngine.Random.Range(1, 256);
		currentPlayerLevel = level;
		Check("currentPlayerLevel", (byte)currentPlayerLevel, level, ref checks, ref failures);
		bool godMode = UnityEngine.Random.value < 0.5f;
		isGodModeEnabled = godMode;
		Check("isGodModeEnabled", (bool)isGodModeEnabled, godMode, ref checks, ref failures);
		sbyte difficulty = (sbyte)(
			UnityEngine.Random.Range(1, 128) * (UnityEngine.Random.value < 0.5f ? -1 : 1)
		);
		difficultyLevel = difficulty;
		Check("difficultyLevel", (sbyte)difficultyLevel, difficulty, ref checks, ref failures);
		double multiplier = (double)UnityEngine.Random.Range(-100000f, 100000f);
		if (multiplier == 0.0)
		{
			multiplier = 1.25;
		}
		criticalHitDamageMultiplier = multiplier;
		Check(
			"criticalHitDamageMultiplier",
			(double)criticalHitDamageMultiplier,
			multiplier,
			ref checks,
			ref failures
		);
		DateTimeOffset activation = new DateTimeOffset(
			new DateTime(2000, 1, 1)
				.AddDays(UnityEngine.Random.Range(0, 11000))
				.AddHours(UnityEngine.Random.Range(0, 24)),
			TimeSpan.FromHours(UnityEngine.Random.Range(-12, 15))
		);
		eventActivationDate = activation;
		Check(
			"eventActivationDate",
			(DateTimeOffset)eventActivationDate,
			activation,
			ref checks,
			ref failures
		);
		DateTime login = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
			.AddDays(UnityEngine.Random.Range(0, 11000))
			.AddSeconds(UnityEngine.Random.Range(0, 86400));
		lastLoginTimeUtc = login;
		Check("lastLoginTimeUtc", (DateTime)lastLoginTimeUtc, login, ref checks, ref failures);
		TimeSpan boost = TimeSpan
			.FromDays(UnityEngine.Random.Range(0, 30))
			.Add(TimeSpan.FromTicks(UnityEngine.Random.Range(1, int.MaxValue)));
		activeBoostDuration = boost;
		Check(
			"activeBoostDuration",
			(TimeSpan)activeBoostDuration,
			boost,
			ref checks,
			ref failures
		);

		Vector2 uiPos = new Vector2(
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(-100f, 100f)
		);
		uiPosition = uiPos;
		Check("uiPosition", (Vector2)uiPosition, uiPos, ref checks, ref failures);
		Vector2Int tile = new Vector2Int(
			UnityEngine.Random.Range(-50, 51),
			UnityEngine.Random.Range(-50, 51)
		);
		tileCoordinate = tile;
		Check("tileCoordinate", (Vector2Int)tileCoordinate, tile, ref checks, ref failures);
		Vector3 respawn = RandomVector3(-100f, 100f);
		respawnPoint = respawn;
		Check("respawnPoint", (Vector3)respawnPoint, respawn, ref checks, ref failures);
		Vector3Int cell = new Vector3Int(
			UnityEngine.Random.Range(-50, 51),
			UnityEngine.Random.Range(-50, 51),
			UnityEngine.Random.Range(-50, 51)
		);
		gridCell = cell;
		Check("gridCell", (Vector3Int)gridCell, cell, ref checks, ref failures);
		Vector4 spell = new Vector4(
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(-100f, 100f)
		);
		spellEffectParameters = spell;
		Check(
			"spellEffectParameters",
			(Vector4)spellEffectParameters,
			spell,
			ref checks,
			ref failures
		);
		Rect viewport = new Rect(
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(1f, 200f),
			UnityEngine.Random.Range(1f, 200f)
		);
		minimapViewport = viewport;
		Check("minimapViewport", (Rect)minimapViewport, viewport, ref checks, ref failures);
		RectInt slot = new RectInt(
			UnityEngine.Random.Range(-50, 51),
			UnityEngine.Random.Range(-50, 51),
			UnityEngine.Random.Range(1, 100),
			UnityEngine.Random.Range(1, 100)
		);
		inventorySlotArea = slot;
		Check("inventorySlotArea", (RectInt)inventorySlotArea, slot, ref checks, ref failures);
		Bounds arena = new Bounds(RandomVector3(-10f, 10f), RandomVector3(1f, 50f));
		arenaBounds = arena;
		Check("arenaBounds", (Bounds)arenaBounds, arena, ref checks, ref failures);
		BoundsInt zone = new BoundsInt(
			new Vector3Int(
				UnityEngine.Random.Range(-50, 51),
				UnityEngine.Random.Range(-50, 51),
				UnityEngine.Random.Range(-50, 51)
			),
			new Vector3Int(
				UnityEngine.Random.Range(1, 20),
				UnityEngine.Random.Range(1, 20),
				UnityEngine.Random.Range(1, 20)
			)
		);
		buildZone = zone;
		Check("buildZone", (BoundsInt)buildZone, zone, ref checks, ref failures);
		Color team = new Color(
			UnityEngine.Random.value,
			UnityEngine.Random.value,
			UnityEngine.Random.value,
			UnityEngine.Random.value
		);
		teamColor = team;
		Check("teamColor", (Color)teamColor, team, ref checks, ref failures);
		Color32 pixel = new Color32(
			(byte)UnityEngine.Random.Range(0, 256),
			(byte)UnityEngine.Random.Range(0, 256),
			(byte)UnityEngine.Random.Range(0, 256),
			(byte)UnityEngine.Random.Range(0, 256)
		);
		minimapPixel = pixel;
		Check("minimapPixel", (Color32)minimapPixel, pixel, ref checks, ref failures);
		Quaternion door = Quaternion.Euler(
			UnityEngine.Random.Range(0f, 360f),
			UnityEngine.Random.Range(0f, 360f),
			UnityEngine.Random.Range(0f, 360f)
		);
		doorRotation = door;
		Check("doorRotation", (Quaternion)doorRotation, door, ref checks, ref failures);
		Matrix4x4 portal = Matrix4x4.TRS(
			RandomVector3(-10f, 10f),
			Quaternion.Euler(
				UnityEngine.Random.Range(0f, 360f),
				UnityEngine.Random.Range(0f, 360f),
				UnityEngine.Random.Range(0f, 360f)
			),
			new Vector3(
				UnityEngine.Random.Range(0.1f, 5f),
				UnityEngine.Random.Range(0.1f, 5f),
				UnityEngine.Random.Range(0.1f, 5f)
			)
		);
		portalTransform = portal;
		Check("portalTransform", (Matrix4x4)portalTransform, portal, ref checks, ref failures);
		Plane water = new Plane(RandomVector3(-1f, 1f), UnityEngine.Random.Range(-50f, 50f));
		waterPlaneHeight = water;
		Check("waterPlaneHeight", (Plane)waterPlaneHeight, water, ref checks, ref failures);
		Vector3 aimDirection = RandomVector3(-1f, 1f);
		if (aimDirection.sqrMagnitude < 1e-6f)
		{
			aimDirection = Vector3.up;
		}
		Ray aim = new Ray(RandomVector3(-10f, 10f), aimDirection);
		aimRay = aim;
		Check("aimRay", (Ray)aimRay, aim, ref checks, ref failures);
		LayerMask layers = new LayerMask() { value = UnityEngine.Random.Range(1, 1024) };
		enemyCollisionLayers = layers;
		Check(
			"enemyCollisionLayers",
			((LayerMask)enemyCollisionLayers).value,
			layers.value,
			ref checks,
			ref failures
		);

		System.Numerics.Vector2 joy = new System.Numerics.Vector2(
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(-100f, 100f)
		);
		joystickInput = joy;
		Check(
			"joystickInput",
			(System.Numerics.Vector2)joystickInput,
			joy,
			ref checks,
			ref failures
		);
		System.Numerics.Vector3 spawn = new System.Numerics.Vector3(
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(-100f, 100f)
		);
		spawnPointPosition = spawn;
		Check(
			"spawnPointPosition",
			(System.Numerics.Vector3)spawnPointPosition,
			spawn,
			ref checks,
			ref failures
		);
		System.Numerics.Vector4 shader = new System.Numerics.Vector4(
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(-100f, 100f)
		);
		shaderEffectParameters = shader;
		Check(
			"shaderEffectParameters",
			(System.Numerics.Vector4)shaderEffectParameters,
			shader,
			ref checks,
			ref failures
		);
		System.Numerics.Quaternion camera = new System.Numerics.Quaternion(
			UnityEngine.Random.Range(-10f, 10f),
			UnityEngine.Random.Range(-10f, 10f),
			UnityEngine.Random.Range(-10f, 10f),
			UnityEngine.Random.Range(-10f, 10f)
		);
		cameraRotation = camera;
		Check(
			"cameraRotation",
			(System.Numerics.Quaternion)cameraRotation,
			camera,
			ref checks,
			ref failures
		);
		System.Numerics.Plane ground = new System.Numerics.Plane(
			UnityEngine.Random.Range(-1f, 1f),
			UnityEngine.Random.Range(-1f, 1f),
			UnityEngine.Random.Range(-1f, 1f),
			UnityEngine.Random.Range(-50f, 50f)
		);
		groundCollisionPlane = ground;
		Check(
			"groundCollisionPlane",
			(System.Numerics.Plane)groundCollisionPlane,
			ground,
			ref checks,
			ref failures
		);
		System.Numerics.Matrix3x2 ui = new System.Numerics.Matrix3x2(
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f)
		);
		uiTransformMatrix = ui;
		Check(
			"uiTransformMatrix",
			(System.Numerics.Matrix3x2)uiTransformMatrix,
			ui,
			ref checks,
			ref failures
		);
		System.Numerics.Matrix4x4 rig = new System.Numerics.Matrix4x4(
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f),
			UnityEngine.Random.Range(-50f, 50f)
		);
		characterRigTransformMatrix = rig;
		Check(
			"characterRigTransformMatrix",
			(System.Numerics.Matrix4x4)characterRigTransformMatrix,
			rig,
			ref checks,
			ref failures
		);
		System.Numerics.Complex signal = new System.Numerics.Complex(
			UnityEngine.Random.Range(-100f, 100f),
			UnityEngine.Random.Range(-100f, 100f)
		);
		signalProcessingValue = signal;
		Check(
			"signalProcessingValue",
			(System.Numerics.Complex)signalProcessingValue,
			signal,
			ref checks,
			ref failures
		);
		System.Numerics.BigInteger puzzle = System.Numerics.BigInteger.Parse(
			RandomDigits(UnityEngine.Random.Range(1, 31))
		);
		puzzleSolutionNumber = puzzle;
		Check(
			"puzzleSolutionNumber",
			(System.Numerics.BigInteger)puzzleSolutionNumber,
			puzzle,
			ref checks,
			ref failures
		);
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif

		Debug.Log(
			$"[SecureValueDemo] Random fill complete: {checks - failures}/{checks} values round-tripped immediately. These are NOT the Fill constants — Validate Test Values will mismatch until Fill runs again."
		);
	}

	private static Vector3 RandomVector3(float min, float max)
	{
		return new Vector3(
			UnityEngine.Random.Range(min, max),
			UnityEngine.Random.Range(min, max),
			UnityEngine.Random.Range(min, max)
		);
	}

	private static string RandomString(int length)
	{
		const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		var sb = new StringBuilder(length);
		for (int i = 0; i < length; i++)
		{
			sb.Append(alphabet[UnityEngine.Random.Range(0, alphabet.Length)]);
		}
		return sb.ToString();
	}

	private static string RandomDigits(int length)
	{
		var sb = new StringBuilder(length);
		sb.Append((char)('1' + UnityEngine.Random.Range(0, 9)));
		for (int i = 1; i < length; i++)
		{
			sb.Append((char)('0' + UnityEngine.Random.Range(0, 10)));
		}
		return sb.ToString();
	}

	/// <summary>
	/// Inspector test hook (component context menu): decrypts every serialized field
	/// and reports any value that does not match its Fill Test Values constant —
	/// without assigning anything. Standard workflow: Fill, Validate, save + close
	/// Unity, reopen, Validate again. Still green after the restart means the saved
	/// words round-tripped every supported type with integrity intact.
	/// </summary>
	[ContextMenu("Validate Test Values")]
	private void ValidateTestValues()
	{
		int checks = 0;
		int failures = 0;

		Check("playerHealth", (int)playerHealth, 100, ref checks, ref failures);
		Check("totalScore", (uint)totalScore, 250000u, ref checks, ref failures);
		Check("movementSpeed", (float)movementSpeed, 6.5f, ref checks, ref failures);
		Check("playerName", (string)playerName, "SecureValue", ref checks, ref failures);
		Check("playerRankInitial", (char)playerRankInitial, 'S', ref checks, ref failures);
		Check(
			"playerAccountId",
			(Guid)playerAccountId,
			new Guid("11111111-2222-3333-4444-555555555555"),
			ref checks,
			ref failures
		);
		Check("walletBalance", (decimal)walletBalance, 1234.56m, ref checks, ref failures);
		Check(
			"lifetimeExperiencePoints",
			(long)lifetimeExperiencePoints,
			123456789012345L,
			ref checks,
			ref failures
		);
		Check(
			"globalLeaderboardScore",
			(ulong)globalLeaderboardScore,
			9876543210UL,
			ref checks,
			ref failures
		);
		Check("enemyKillCount", (ushort)enemyKillCount, (ushort)1234, ref checks, ref failures);
		Check("ammoInMagazine", (short)ammoInMagazine, (short)30, ref checks, ref failures);
		Check("currentPlayerLevel", (byte)currentPlayerLevel, (byte)7, ref checks, ref failures);
		Check("isGodModeEnabled", (bool)isGodModeEnabled, true, ref checks, ref failures);
		Check("difficultyLevel", (sbyte)difficultyLevel, (sbyte)2, ref checks, ref failures);
		Check(
			"criticalHitDamageMultiplier",
			(double)criticalHitDamageMultiplier,
			1.5,
			ref checks,
			ref failures
		);
		Check(
			"eventActivationDate",
			(DateTimeOffset)eventActivationDate,
			new DateTimeOffset(2026, 12, 31, 23, 59, 0, TimeSpan.FromHours(2)),
			ref checks,
			ref failures
		);
		Check(
			"lastLoginTimeUtc",
			(DateTime)lastLoginTimeUtc,
			new DateTime(2026, 9, 24, 12, 30, 0, DateTimeKind.Utc),
			ref checks,
			ref failures
		);
		Check(
			"activeBoostDuration",
			(TimeSpan)activeBoostDuration,
			new TimeSpan(1, 37, 42),
			ref checks,
			ref failures
		);

		Check(
			"uiPosition",
			(Vector2)uiPosition,
			new Vector2(12.5f, -3.25f),
			ref checks,
			ref failures
		);
		Check(
			"tileCoordinate",
			(Vector2Int)tileCoordinate,
			new Vector2Int(7, -3),
			ref checks,
			ref failures
		);
		Check(
			"respawnPoint",
			(Vector3)respawnPoint,
			new Vector3(1f, 2f, 3f),
			ref checks,
			ref failures
		);
		Check("gridCell", (Vector3Int)gridCell, new Vector3Int(1, -2, 3), ref checks, ref failures);
		Check(
			"spellEffectParameters",
			(Vector4)spellEffectParameters,
			new Vector4(1f, 2f, 3f, 4f),
			ref checks,
			ref failures
		);
		Check(
			"minimapViewport",
			(Rect)minimapViewport,
			new Rect(10f, 20f, 100f, 50f),
			ref checks,
			ref failures
		);
		Check(
			"inventorySlotArea",
			(RectInt)inventorySlotArea,
			new RectInt(1, 2, 10, 20),
			ref checks,
			ref failures
		);
		Check(
			"arenaBounds",
			(Bounds)arenaBounds,
			new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f)),
			ref checks,
			ref failures
		);
		Check(
			"buildZone",
			(BoundsInt)buildZone,
			new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6)),
			ref checks,
			ref failures
		);
		Check(
			"teamColor",
			(Color)teamColor,
			new Color(0.15f, 0.5f, 0.85f, 0.75f),
			ref checks,
			ref failures
		);
		Check(
			"minimapPixel",
			(Color32)minimapPixel,
			new Color32(237, 129, 64, 210),
			ref checks,
			ref failures
		);
		Check(
			"doorRotation",
			(Quaternion)doorRotation,
			Quaternion.Euler(10f, 20f, 30f),
			ref checks,
			ref failures
		);
		Check(
			"portalTransform",
			(Matrix4x4)portalTransform,
			Matrix4x4.TRS(
				new Vector3(1f, 2f, 3f),
				Quaternion.Euler(15f, 30f, 45f),
				new Vector3(2.5f, 0.5f, 1.75f)
			),
			ref checks,
			ref failures
		);
		Check(
			"waterPlaneHeight",
			(Plane)waterPlaneHeight,
			new Plane(new Vector3(0.25f, 0.9f, 0.35f), 5.25f),
			ref checks,
			ref failures
		);
		Check(
			"aimRay",
			(Ray)aimRay,
			new Ray(new Vector3(1f, 2f, 3f), new Vector3(0.36f, 0.48f, 0.8f)),
			ref checks,
			ref failures
		);
		Check(
			"enemyCollisionLayers",
			((LayerMask)enemyCollisionLayers).value,
			5,
			ref checks,
			ref failures
		);

		Check(
			"joystickInput",
			(System.Numerics.Vector2)joystickInput,
			new System.Numerics.Vector2(1.5f, -2.5f),
			ref checks,
			ref failures
		);
		Check(
			"spawnPointPosition",
			(System.Numerics.Vector3)spawnPointPosition,
			new System.Numerics.Vector3(1f, 2f, 3f),
			ref checks,
			ref failures
		);
		Check(
			"shaderEffectParameters",
			(System.Numerics.Vector4)shaderEffectParameters,
			new System.Numerics.Vector4(1f, 2f, 3f, 4f),
			ref checks,
			ref failures
		);
		Check(
			"cameraRotation",
			(System.Numerics.Quaternion)cameraRotation,
			new System.Numerics.Quaternion(0.27f, 0.36f, 0.48f, 0.76f),
			ref checks,
			ref failures
		);
		Check(
			"groundCollisionPlane",
			(System.Numerics.Plane)groundCollisionPlane,
			new System.Numerics.Plane(0.25f, 0.9f, 0.35f, 5.25f),
			ref checks,
			ref failures
		);
		Check(
			"uiTransformMatrix",
			(System.Numerics.Matrix3x2)uiTransformMatrix,
			new System.Numerics.Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f),
			ref checks,
			ref failures
		);
		Check(
			"characterRigTransformMatrix",
			(System.Numerics.Matrix4x4)characterRigTransformMatrix,
			new System.Numerics.Matrix4x4(
				1f,
				2f,
				3f,
				4f,
				5f,
				6f,
				7f,
				8f,
				9f,
				10f,
				11f,
				12f,
				13f,
				14f,
				15f,
				16f
			),
			ref checks,
			ref failures
		);
		Check(
			"signalProcessingValue",
			(System.Numerics.Complex)signalProcessingValue,
			new System.Numerics.Complex(1.5, -2.5),
			ref checks,
			ref failures
		);
		Check(
			"puzzleSolutionNumber",
			(System.Numerics.BigInteger)puzzleSolutionNumber,
			System.Numerics.BigInteger.Parse("123456789012345678901234567890"),
			ref checks,
			ref failures
		);

		Debug.Log(
			$"[SecureValueDemo] Validation complete: {checks - failures}/{checks} values match their expected constants."
		);
	}

	/// <summary>
	/// Inspector test hook (component context menu): records every field's current
	/// decrypted (plaintext) value to Assets/saved-values.json. Reopen the editor
	/// later and run Validate JSON to prove the values persisted unchanged across
	/// sessions. Cover custom fields by adding one snapshot line per field below.
	/// </summary>
	[ContextMenu("Save to JSON")]
	private void SaveToJson()
	{
		List<(string name, string value)> entries = Snapshot();
		var json = new StringBuilder();
		json.AppendLine("{");
		json.AppendLine("  \"entries\": [");
		for (int i = 0; i < entries.Count; i++)
		{
			json.Append("    { \"name\": \"");
			json.Append(Escape(entries[i].name));
			json.Append("\", \"value\": \"");
			json.Append(Escape(entries[i].value));
			json.Append("\" }");
			json.AppendLine(i + 1 < entries.Count ? "," : string.Empty);
		}
		json.AppendLine("  ]");
		json.AppendLine("}");
		string path = Path.Combine(Application.dataPath, "saved-values.json");
		File.WriteAllText(path, json.ToString());
#if UNITY_EDITOR
		UnityEditor.AssetDatabase.Refresh();
#endif
		Debug.Log(
			$"[SecureValueDemo] Saved {entries.Count} plaintext values to Assets/saved-values.json."
		);
	}

	/// <summary>
	/// Inspector test hook (component context menu): compares every field's live
	/// decrypted value against Assets/saved-values.json and reports deviation per
	/// field. Missing record file means Save to JSON has not been run yet.
	/// </summary>
	[ContextMenu("Validate JSON")]
	private void ValidateJson()
	{
		string path = Path.Combine(Application.dataPath, "saved-values.json");
		if (!File.Exists(path))
		{
			Debug.LogError(
				"[SecureValueDemo] No saved-values.json in Assets yet — run Save to JSON first."
			);
			return;
		}
		var expected = new Dictionary<string, string>();
		foreach ((string name, string value) in Parse(File.ReadAllText(path)))
		{
			expected[name] = value;
		}
		int checks = 0;
		int failures = 0;
		foreach ((string name, string value) in Snapshot())
		{
			checks++;
			if (!expected.TryGetValue(name, out string recorded) || recorded == null)
			{
				failures++;
				Debug.LogError($"[SecureValueDemo] {name} has no recorded value.");
			}
			else if (!string.Equals(recorded, value, StringComparison.Ordinal))
			{
				failures++;
				Debug.LogError(
					$"[SecureValueDemo] {name} deviated: recorded {recorded}, now {value}."
				);
			}
		}
		Debug.Log(
			$"[SecureValueDemo] JSON validation complete: {checks - failures}/{checks} values unchanged since save."
		);
	}

	/// <summary>
	/// Decrypts every field into (name, plaintext) pairs. Plaintext strings are
	/// formatted identically on save and validate, so string comparison is exact.
	/// </summary>
	private List<(string name, string value)> Snapshot()
	{
		var entries = new List<(string name, string value)>();
		entries.Add(("playerHealth", V((int)playerHealth)));
		entries.Add(("totalScore", V((uint)totalScore)));
		entries.Add(("movementSpeed", R((float)movementSpeed)));
		entries.Add(("playerName", V((string)playerName)));
		entries.Add(("playerRankInitial", V((char)playerRankInitial)));
		entries.Add(("playerAccountId", V((Guid)playerAccountId)));
		entries.Add(("walletBalance", V((decimal)walletBalance)));
		entries.Add(("lifetimeExperiencePoints", V((long)lifetimeExperiencePoints)));
		entries.Add(("globalLeaderboardScore", V((ulong)globalLeaderboardScore)));
		entries.Add(("enemyKillCount", V((ushort)enemyKillCount)));
		entries.Add(("ammoInMagazine", V((short)ammoInMagazine)));
		entries.Add(("currentPlayerLevel", V((byte)currentPlayerLevel)));
		entries.Add(("isGodModeEnabled", V((bool)isGodModeEnabled)));
		entries.Add(("difficultyLevel", V((sbyte)difficultyLevel)));
		entries.Add(("criticalHitDamageMultiplier", R((double)criticalHitDamageMultiplier)));
		entries.Add(("eventActivationDate", V((DateTimeOffset)eventActivationDate)));
		entries.Add(("lastLoginTimeUtc", V((DateTime)lastLoginTimeUtc)));
		entries.Add(("activeBoostDuration", V((TimeSpan)activeBoostDuration)));

		Vector2 uiPos = uiPosition;
		entries.Add(("uiPosition", R(uiPos.x) + ";" + R(uiPos.y)));
		Vector2Int tile = tileCoordinate;
		entries.Add(("tileCoordinate", V(tile.x) + ";" + V(tile.y)));
		Vector3 respawn = respawnPoint;
		entries.Add(("respawnPoint", R(respawn.x) + ";" + R(respawn.y) + ";" + R(respawn.z)));
		Vector3Int grid = gridCell;
		entries.Add(("gridCell", V(grid.x) + ";" + V(grid.y) + ";" + V(grid.z)));
		Vector4 spell = spellEffectParameters;
		entries.Add(
			(
				"spellEffectParameters",
				R(spell.x) + ";" + R(spell.y) + ";" + R(spell.z) + ";" + R(spell.w)
			)
		);
		Rect viewport = minimapViewport;
		entries.Add(
			(
				"minimapViewport",
				R(viewport.x)
					+ ";"
					+ R(viewport.y)
					+ ";"
					+ R(viewport.width)
					+ ";"
					+ R(viewport.height)
			)
		);
		RectInt slot = inventorySlotArea;
		entries.Add(
			(
				"inventorySlotArea",
				V(slot.x) + ";" + V(slot.y) + ";" + V(slot.width) + ";" + V(slot.height)
			)
		);
		Bounds arena = arenaBounds;
		entries.Add(
			(
				"arenaBounds",
				R(arena.center.x)
					+ ";"
					+ R(arena.center.y)
					+ ";"
					+ R(arena.center.z)
					+ ";"
					+ R(arena.size.x)
					+ ";"
					+ R(arena.size.y)
					+ ";"
					+ R(arena.size.z)
			)
		);
		BoundsInt zone = buildZone;
		entries.Add(
			(
				"buildZone",
				V(zone.position.x)
					+ ";"
					+ V(zone.position.y)
					+ ";"
					+ V(zone.position.z)
					+ ";"
					+ V(zone.size.x)
					+ ";"
					+ V(zone.size.y)
					+ ";"
					+ V(zone.size.z)
			)
		);
		Color team = teamColor;
		entries.Add(("teamColor", R(team.r) + ";" + R(team.g) + ";" + R(team.b) + ";" + R(team.a)));
		Color32 pixel = minimapPixel;
		entries.Add(
			("minimapPixel", V(pixel.r) + ";" + V(pixel.g) + ";" + V(pixel.b) + ";" + V(pixel.a))
		);
		Quaternion door = doorRotation;
		entries.Add(
			("doorRotation", R(door.x) + ";" + R(door.y) + ";" + R(door.z) + ";" + R(door.w))
		);
		Matrix4x4 portal = portalTransform;
		entries.Add(
			(
				"portalTransform",
				R(portal.m00)
					+ ";"
					+ R(portal.m01)
					+ ";"
					+ R(portal.m02)
					+ ";"
					+ R(portal.m03)
					+ ";"
					+ R(portal.m10)
					+ ";"
					+ R(portal.m11)
					+ ";"
					+ R(portal.m12)
					+ ";"
					+ R(portal.m13)
					+ ";"
					+ R(portal.m20)
					+ ";"
					+ R(portal.m21)
					+ ";"
					+ R(portal.m22)
					+ ";"
					+ R(portal.m23)
					+ ";"
					+ R(portal.m30)
					+ ";"
					+ R(portal.m31)
					+ ";"
					+ R(portal.m32)
					+ ";"
					+ R(portal.m33)
			)
		);
		Plane water = waterPlaneHeight;
		entries.Add(
			(
				"waterPlaneHeight",
				R(water.normal.x)
					+ ";"
					+ R(water.normal.y)
					+ ";"
					+ R(water.normal.z)
					+ ";"
					+ R(water.distance)
			)
		);
		Ray aim = aimRay;
		entries.Add(
			(
				"aimRay",
				R(aim.origin.x)
					+ ";"
					+ R(aim.origin.y)
					+ ";"
					+ R(aim.origin.z)
					+ ";"
					+ R(aim.direction.x)
					+ ";"
					+ R(aim.direction.y)
					+ ";"
					+ R(aim.direction.z)
			)
		);
		entries.Add(("enemyCollisionLayers", V(((LayerMask)enemyCollisionLayers).value)));

		System.Numerics.Vector2 joy = joystickInput;
		entries.Add(("joystickInput", R(joy.X) + ";" + R(joy.Y)));
		System.Numerics.Vector3 spawn = spawnPointPosition;
		entries.Add(("spawnPointPosition", R(spawn.X) + ";" + R(spawn.Y) + ";" + R(spawn.Z)));
		System.Numerics.Vector4 shader = shaderEffectParameters;
		entries.Add(
			(
				"shaderEffectParameters",
				R(shader.X) + ";" + R(shader.Y) + ";" + R(shader.Z) + ";" + R(shader.W)
			)
		);
		System.Numerics.Quaternion camera = cameraRotation;
		entries.Add(
			(
				"cameraRotation",
				R(camera.X) + ";" + R(camera.Y) + ";" + R(camera.Z) + ";" + R(camera.W)
			)
		);
		System.Numerics.Plane ground = groundCollisionPlane;
		entries.Add(
			(
				"groundCollisionPlane",
				R(ground.Normal.X)
					+ ";"
					+ R(ground.Normal.Y)
					+ ";"
					+ R(ground.Normal.Z)
					+ ";"
					+ R(ground.D)
			)
		);
		System.Numerics.Matrix3x2 ui = uiTransformMatrix;
		entries.Add(
			(
				"uiTransformMatrix",
				R(ui.M11)
					+ ";"
					+ R(ui.M12)
					+ ";"
					+ R(ui.M21)
					+ ";"
					+ R(ui.M22)
					+ ";"
					+ R(ui.M31)
					+ ";"
					+ R(ui.M32)
			)
		);
		System.Numerics.Matrix4x4 rig = characterRigTransformMatrix;
		entries.Add(
			(
				"characterRigTransformMatrix",
				R(rig.M11)
					+ ";"
					+ R(rig.M12)
					+ ";"
					+ R(rig.M13)
					+ ";"
					+ R(rig.M14)
					+ ";"
					+ R(rig.M21)
					+ ";"
					+ R(rig.M22)
					+ ";"
					+ R(rig.M23)
					+ ";"
					+ R(rig.M24)
					+ ";"
					+ R(rig.M31)
					+ ";"
					+ R(rig.M32)
					+ ";"
					+ R(rig.M33)
					+ ";"
					+ R(rig.M34)
					+ ";"
					+ R(rig.M41)
					+ ";"
					+ R(rig.M42)
					+ ";"
					+ R(rig.M43)
					+ ";"
					+ R(rig.M44)
			)
		);
		System.Numerics.Complex signal = signalProcessingValue;
		entries.Add(("signalProcessingValue", R(signal.Real) + ";" + R(signal.Imaginary)));
		entries.Add(("puzzleSolutionNumber", V((System.Numerics.BigInteger)puzzleSolutionNumber)));
		return entries;
	}

	private static string V<T>(T value)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}", value);
	}

	private static string V(DateTime value)
	{
		return value.ToString("O", CultureInfo.InvariantCulture);
	}

	private static string V(DateTimeOffset value)
	{
		return value.ToString("O", CultureInfo.InvariantCulture);
	}

	private static string V(TimeSpan value)
	{
		return value.ToString("c", CultureInfo.InvariantCulture);
	}

	private static string V(Guid value)
	{
		return value.ToString("D");
	}

	private static string R(float value)
	{
		return value.ToString("R", CultureInfo.InvariantCulture);
	}

	private static string R(double value)
	{
		return value.ToString("R", CultureInfo.InvariantCulture);
	}

	private static string Escape(string value)
	{
		return value
			.Replace("\\", "\\\\")
			.Replace("\"", "\\\"")
			.Replace("\n", "\\n")
			.Replace("\r", "\\r")
			.Replace("\t", "\\t");
	}

	private static string Unescape(string value)
	{
		return value
			.Replace("\\\\", "\\")
			.Replace("\\\"", "\"")
			.Replace("\\n", "\n")
			.Replace("\\r", "\r")
			.Replace("\\t", "\t");
	}

	private static List<(string name, string value)> Parse(string json)
	{
		var entries = new List<(string name, string value)>();
		int i = 0;
		while (i < json.Length)
		{
			int nameKey = IndexOfKey(json, "\"name\"", i);
			if (nameKey < 0)
			{
				break;
			}
			int p = nameKey + 6;
			if (!ReadQuotedAfterColon(json, ref p, out string name))
			{
				break;
			}
			int valueKey = IndexOfKey(json, "\"value\"", p);
			if (valueKey < 0)
			{
				break;
			}
			p = valueKey + 7;
			if (!ReadQuotedAfterColon(json, ref p, out string value))
			{
				break;
			}
			entries.Add((Unescape(name), Unescape(value)));
			i = p;
		}
		return entries;
	}

	private static int IndexOfKey(string json, string key, int start)
	{
		// A quoted value may itself contain `"name"` text (escaped): only accept
		// matches behaving as keys (preceded by `{`, `,`, or `[` past whitespace).
		int i = start;
		while (i < json.Length)
		{
			int hit = json.IndexOf(key, i, StringComparison.Ordinal);
			if (hit < 0)
			{
				return -1;
			}
			int j = hit - 1;
			while (j >= 0 && char.IsWhiteSpace(json[j]))
			{
				j--;
			}
			if (j >= 0 && (json[j] == '{' || json[j] == ',' || json[j] == '['))
			{
				return hit;
			}
			i = hit + 1;
		}
		return -1;
	}

	private static bool ReadQuotedAfterColon(string json, ref int i, out string token)
	{
		int colon = json.IndexOf(':', i);
		if (colon < 0)
		{
			token = string.Empty;
			return false;
		}
		i = colon + 1;
		return ReadQuoted(json, ref i, out token);
	}

	private static bool ReadQuoted(string json, ref int i, out string token)
	{
		while (i < json.Length && json[i] != '"')
		{
			i++;
		}
		if (i >= json.Length)
		{
			token = string.Empty;
			return false;
		}
		i++;
		var raw = new StringBuilder();
		while (i < json.Length)
		{
			char c = json[i];
			if (c == '\\' && i + 1 < json.Length)
			{
				raw.Append(c);
				raw.Append(json[i + 1]);
				i += 2;
			}
			else if (c == '"')
			{
				i++;
				token = raw.ToString();
				return true;
			}
			else
			{
				raw.Append(c);
				i++;
			}
		}
		token = string.Empty;
		return false;
	}

	private static void Check<T>(
		string fieldName,
		T actual,
		T expected,
		ref int checks,
		ref int failures
	)
	{
		checks++;
		if (!EqualityComparer<T>.Default.Equals(actual, expected))
		{
			failures++;
			Debug.LogError(
				$"[SecureValueDemo] {fieldName} mismatch: expected {expected}, got {actual}."
			);
		}
	}
}
