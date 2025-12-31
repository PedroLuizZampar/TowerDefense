using Godot;

public abstract partial class Enemy : CharacterBody2D
{
	// ---- SINAIS (opcional para o jogo reagir: pontuação, dinheiro, etc.) ----
    // MUDAR
	[Signal] public delegate void DiedEventHandler();
	[Signal] public delegate void ReachedEndEventHandler();

	// ---- DADOS COMUNS (definidos nas classes filhas) ----
	// Nome do inimigo.
	protected abstract string EnemyName { get; }

	// Vida máxima do inimigo.
	protected abstract int MaxHealth { get; }

	// Velocidade do inimigo (pixels por segundo).
	protected abstract float MoveSpeed { get; }

	// Se true, o inimigo some ao chegar no final do caminho.
	[Export] public bool DespawnOnFinish { get; set; } = true;

	// ---- CAMINHOS (2 opções) ----
    
    // Encontra os dois caminhos pelo nome
	[Export] public string FallbackPathUp { get; set; } = "PathUp";
	[Export] public string FallbackPathDown { get; set; } = "PathDown";

	// Força qual caminho usar: -1 = aleatório, 0 = Up, 1 = Down.
	[Export] public int ForcedPathIndex { get; set; } = -1;

	// ---- ESTADO (runtime) ----
	public int CurrentHealth { get; private set; }
	public string DisplayName => EnemyName;

	protected Path2D SelectedPath;

	private float _distanceAlongPath;
	private float _selectedPathLength;
	private Vector2 _lastGlobalPosition;

    // --- INÍCIO DO JOGO ---
	public override void _Ready()
	{
		// Inicializa vida.
		CurrentHealth = Mathf.Max(1, MaxHealth);

		// Seleciona o caminho e posiciona no início.
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

		// Avança ao longo do caminho conforme a velocidade.
		_distanceAlongPath += MoveSpeed * (float)delta;

		// Chegou no fim do caminho.
		if (_distanceAlongPath >= _selectedPathLength)
		{
			OnReachedEnd();
			return;
		}

		// Calcula a nova posição global a partir da curva.
		Vector2 targetGlobalPosition = SelectedPath.ToGlobal(curve.SampleBaked(_distanceAlongPath));
		Vector2 movement = targetGlobalPosition - _lastGlobalPosition;

		GlobalPosition = targetGlobalPosition;
		_lastGlobalPosition = targetGlobalPosition;

		UpdateWalkAnimation(movement);
	}

	// ---------------- VIDA ----------------
	public virtual void TakeDamage(int damage)
	{
		if (damage <= 0)
			return;

		CurrentHealth = Mathf.Max(0, CurrentHealth - damage);

		if (CurrentHealth <= 0)
			Die();
	}

	protected virtual void Die()
	{
		EmitSignal(SignalName.Died);
		OnDied();
		QueueFree();
	}

	// ---------------- MOVIMENTO / CAMINHO ----------------
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

		return pathUp ?? pathDown; // Retorna o que não for nulo
	}

    // Obtém um Path2D a partir de um NodePath (se fornecido).
    // Retorna null se o NodePath for inválido ou não apontar para um Path2D.
	private Path2D GetPathFromNodePath(NodePath nodePath)
	{
		if (nodePath == null || nodePath.IsEmpty)
			return null;

		return GetNodeOrNull<Path2D>(nodePath);
	}

    // Procura um Path2D pelo nome do nó, primeiro como irmão, depois na cena atual.
	private Path2D FindPath2DByName(string nodeName)
	{
		if (string.IsNullOrWhiteSpace(nodeName))
			return null;

		// Primeiro tenta achar como irmão (mesmo pai).
		var parent = GetParent();
		if (parent != null)
		{
			var sibling = parent.GetNodeOrNull<Path2D>(nodeName);
			if (sibling != null)
				return sibling;
		}

		// Depois tenta diretamente na cena atual.
		var scene = GetTree()?.CurrentScene;
		if (scene != null)
		{
			var inScene = scene.GetNodeOrNull<Path2D>(nodeName);
			if (inScene != null)
				return inScene;
		}

		return null;
	}

    // Chegou no fim do caminho.
    // Emite sinal e despawna ou para o processamento.
	protected virtual void OnReachedEnd()
	{
		EmitSignal(SignalName.ReachedEnd);

		if (DespawnOnFinish)
			QueueFree();
		else
			SetPhysicsProcess(false);
	}

	// ---------------- ANIMAÇÃO ----------------
	// Declaramos a existência do método, mas a implementação é responsabilidade de cada inimigo.
	// Ex.: Slime vai decidir qual AnimatedSprite2D usar e quais animações tocar.
	protected abstract void UpdateWalkAnimation(Vector2 movement);

	// ---------------- EXTENSÃO (para classes filhas) ----------------
	protected virtual void OnSpawned() { }
	protected virtual void OnDied() { }
}
