using Godot;

/// <summary>
/// Classe base abstrata para todos os inimigos.
/// Define comportamento comum: movimento em paths, vida, dano e animação.
/// </summary>
public abstract partial class Enemy : CharacterBody2D
{
	/// <summary>Nome único do tipo de inimigo (ex: "Slime", "Orc")</summary>
	protected abstract string EnemyName { get; }

	/// <summary>Vida máxima deste inimigo</summary>
	protected abstract int MaxHealth { get; }

	/// <summary>Velocidade de movimento em pixels por segundo</summary>
	protected abstract float MoveSpeed { get; }

	/// <summary>Se verdadeiro, o inimigo é removido ao atingir o final do caminho</summary>
	[Export] public bool DespawnOnFinish { get; set; } = true;

	/// <summary>Nome do nó Path2D para o caminho superior</summary>
	[Export] public string FallbackPathUp { get; set; } = GameConstants.DefaultPathNameUp;

	/// <summary>Nome do nó Path2D para o caminho inferior</summary>
	[Export] public string FallbackPathDown { get; set; } = GameConstants.DefaultPathNameDown;

	/// <summary>Força qual caminho usar: -1 = aleatório, 0 = Up, 1 = Down</summary>
	[Export] public int ForcedPathIndex { get; set; } = -1;

	/// <summary>Vida atual do inimigo</summary>
	public int CurrentHealth { get; private set; }

	/// <summary>Nome para exibição (ex: UI, logs)</summary>
	public string DisplayName => EnemyName;

	protected Path2D SelectedPath;

	private float _distanceAlongPath;
	private float _selectedPathLength;
	private Vector2 _lastGlobalPosition;

	public override void _Ready()
	{
		CurrentHealth = Mathf.Max(GameConstants.MinHealth, MaxHealth);

		SelectedPath = SelectPath();
		if (SelectedPath == null)
		{
			GD.PushWarning($"[{EnemyName}] Nenhum Path2D encontrado. Crie nós '{FallbackPathUp}'/'{FallbackPathDown}'.");
			SetPhysicsProcess(false);
			return;
		}

		var curve = SelectedPath.Curve;
		if (curve == null)
		{
			GD.PushWarning($"[{EnemyName}] Path2D selecionado não possui Curve.");
			SetPhysicsProcess(false);
			return;
		}

		_distanceAlongPath = 0f;
		_selectedPathLength = curve.GetBakedLength();

		_lastGlobalPosition = SelectedPath.ToGlobal(curve.SampleBaked(_distanceAlongPath));
		GlobalPosition = _lastGlobalPosition;

		OnSpawned();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (SelectedPath?.Curve == null)
			return;

		var curve = SelectedPath.Curve;

		_distanceAlongPath += MoveSpeed * (float)delta;

		if (_distanceAlongPath >= _selectedPathLength)
		{
			OnReachedEnd();
			return;
		}

		Vector2 targetGlobalPosition = SelectedPath.ToGlobal(curve.SampleBaked(_distanceAlongPath));
		Vector2 movement = targetGlobalPosition - _lastGlobalPosition;

		GlobalPosition = targetGlobalPosition;
		_lastGlobalPosition = targetGlobalPosition;

		UpdateWalkAnimation(movement);
	}

	/// <summary>Reduz a vida do inimigo e emite eventos</summary>
	public virtual void TakeDamage(int damage)
	{
		if (damage <= 0)
			return;

		CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
		GameEvents.InvokeEnemyDamageTaken(this, damage);

		if (CurrentHealth <= 0)
			Die();
	}

	protected virtual void Die()
	{
		GameEvents.InvokeEnemyDied(this);
		OnDied();
		QueueFree();
	}

	protected virtual Path2D SelectPath()
	{
		Path2D pathUp = FindPath2DByName(FallbackPathUp);
		Path2D pathDown = FindPath2DByName(FallbackPathDown);

		if (ForcedPathIndex == 0)
			return pathUp;
		if (ForcedPathIndex == 1)
			return pathDown;

		if (pathUp != null && pathDown != null)
			return GD.Randf() < 0.5f ? pathUp : pathDown;

		return pathUp ?? pathDown;
	}

	private Path2D FindPath2DByName(string nodeName)
	{
		if (string.IsNullOrWhiteSpace(nodeName))
			return null;

		var parent = GetParent();
		if (parent != null)
		{
			var sibling = parent.GetNodeOrNull<Path2D>(nodeName);
			if (sibling != null)
				return sibling;
		}

		var scene = GetTree()?.CurrentScene;
		if (scene != null)
		{
			var inScene = scene.GetNodeOrNull<Path2D>(nodeName);
			if (inScene != null)
				return inScene;
		}

		return null;
	}

	protected virtual void OnReachedEnd()
	{
		GameEvents.InvokeEnemyReachedEnd(this);

		if (DespawnOnFinish)
			QueueFree();
		else
			SetPhysicsProcess(false);
	}

	/// <summary>Cada inimigo implementa sua própria animação de caminhada</summary>
	protected abstract void UpdateWalkAnimation(Vector2 movement);

	/// <summary>Chamado quando o inimigo é spawned (pode ser sobrescrito)</summary>
	protected virtual void OnSpawned() { }

	/// <summary>Chamado quando o inimigo morre (pode ser sobrescrito)</summary>
	protected virtual void OnDied() { }
}
