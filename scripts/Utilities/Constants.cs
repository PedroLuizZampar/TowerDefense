using Godot;

/// <summary>
/// Constantes globais do jogo
/// </summary>
public static class GameConstants
{
	// ---- LAYERS E CAMADAS ----
	/// <summary>
	/// Máscara de colisão padrão para inimigos.
	/// Use para detecção em towers e outras áreas.
	/// </summary>
	public const uint EnemyCollisionMask = 1;

	// ---- VELOCIDADES ----
	/// <summary>
	/// Velocidade padrão mínima de inimigos (pixels por segundo)
	/// </summary>
	public const float MinEnemySpeed = 50f;

	/// <summary>
	/// Velocidade padrão máxima de inimigos (pixels por segundo)
	/// </summary>
	public const float MaxEnemySpeed = 300f;

	// ---- HEALTH (Vida) ----
	/// <summary>
	/// Vida mínima de qualquer entidade
	/// </summary>
	public const int MinHealth = 1;

	// ---- TORRES ----
	/// <summary>
	/// Raio mínimo de alcance de uma torre (pixels)
	/// </summary>
	public const float MinTowerRange = 50f;

	/// <summary>
	/// Raio máximo de alcance de uma torre (pixels)
	/// </summary>
	public const float MaxTowerRange = 500f;

	// ---- PATH2D ----
	/// <summary>
	/// Nome padrão do Path2D para caminhos de inimigos acima
	/// </summary>
	public const string DefaultPathNameUp = "PathUp";

	/// <summary>
	/// Nome padrão do Path2D para caminhos de inimigos abaixo
	/// </summary>
	public const string DefaultPathNameDown = "PathDown";
}
