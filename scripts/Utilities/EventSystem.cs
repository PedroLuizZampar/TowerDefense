using Godot;
using System;

/// <summary>
/// Sistema centralizado de eventos para o jogo
/// Permite comunicação entre sistemas sem acoplamento direto
/// </summary>
public static class GameEvents
{
	// ---- EVENTOS DE INIMIGOS ----
	/// <summary>
	/// Emitido quando um inimigo morre
	/// Parâmetro: Enemy que morreu
	/// </summary>
	public static event Action<Enemy> OnEnemyDied;

	/// <summary>
	/// Emitido quando um inimigo alcança o final do caminho
	/// Parâmetro: Enemy que alcançou o final
	/// </summary>
	public static event Action<Enemy> OnEnemyReachedEnd;

	/// <summary>
	/// Emitido quando um inimigo toma dano
	/// Parâmetros: Enemy, dano recebido
	/// </summary>
	public static event Action<Enemy, int> OnEnemyDamageTaken;

	// ---- EVENTOS DE TORRES ----
	/// <summary>
	/// Emitido quando uma torre atira em um inimigo
	/// Parâmetros: Tower que atirou, Enemy alvo
	/// </summary>
	public static event Action<Tower, Enemy> OnTowerShoot;

	/// <summary>
	/// Emitido quando uma torre muda de alvo
	/// Parâmetros: Tower, novo alvo (pode ser null)
	/// </summary>
	public static event Action<Tower, Enemy> OnTowerTargetChanged;

	// ---- EVENTOS DE JOGO ----
	/// <summary>
	/// Emitido quando o jogo começa
	/// </summary>
	public static event Action OnGameStarted;

	/// <summary>
	/// Emitido quando o jogo é pausado
	/// </summary>
	public static event Action OnGamePaused;

	/// <summary>
	/// Emitido quando o jogo é retomado
	/// </summary>
	public static event Action OnGameResumed;

	// ---- INVOCADORES (método para disparar eventos) ----

	public static void InvokeEnemyDied(Enemy enemy)
	{
		OnEnemyDied?.Invoke(enemy);
	}

	public static void InvokeEnemyReachedEnd(Enemy enemy)
	{
		OnEnemyReachedEnd?.Invoke(enemy);
	}

	public static void InvokeEnemyDamageTaken(Enemy enemy, int damage)
	{
		OnEnemyDamageTaken?.Invoke(enemy, damage);
	}

	public static void InvokeTowerShoot(Tower tower, Enemy target)
	{
		OnTowerShoot?.Invoke(tower, target);
	}

	public static void InvokeTowerTargetChanged(Tower tower, Enemy newTarget)
	{
		OnTowerTargetChanged?.Invoke(tower, newTarget);
	}

	public static void InvokeGameStarted()
	{
		OnGameStarted?.Invoke();
	}

	public static void InvokeGamePaused()
	{
		OnGamePaused?.Invoke();
	}

	public static void InvokeGameResumed()
	{
		OnGameResumed?.Invoke();
	}
}
