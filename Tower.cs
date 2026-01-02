using Godot;
using System.Collections.Generic;

[Tool]
public abstract partial class Tower : StaticBody2D
{
	// Raio do alcance. Se <= 0, usa o DefaultRangeRadius da torre específica.
	[Export]
	public float RangeRadius
	{
		get => _rangeRadius;
		set
		{
			_rangeRadius = value;
			if (IsInsideTree())
				CallDeferred(nameof(UpdateRangeShapeOnly)); // atualiza no editor ao mexer no Inspector
		}
	}

	private float _rangeRadius = 0f;

	private float EffectiveRangeRadius => _rangeRadius > 0f ? _rangeRadius : DefaultRangeRadius;

	// Nome/pattern dos nós esperados na cena.
	[Export] public NodePath RangeAreaPath { get; set; } = "RangeArea";
	[Export] public NodePath CannonPath { get; set; } = "Cannon";

	// Offset (em graus) para corrigir sprites que apontam para o lado oposto.
	// Para sprite invertido 180°, deixe em 180.
	[Export] public float CannonRotationOffsetDegrees { get; set; } = 90f;

	// Máscara de colisão do Area2D (quais layers ele detect [BodyEntered]).
	// Por padrão = 1 (layer 1), que costuma ser o padrão dos inimigos.
	[Export(PropertyHint.Layers2DPhysics)]
	public uint EnemyCollisionMask { get; set; } = 1;

	protected abstract float DefaultRangeRadius { get; }

	protected Area2D RangeArea { get; private set; }
	protected Node2D CannonNode { get; private set; }
	protected Enemy CurrentTarget { get; private set; }

	private readonly HashSet<Enemy> _enemiesInRange = new();

	public override void _Ready()
	{
		// No editor: só mantém o gizmo/shape coerente com o código/Inspector.
		if (Engine.IsEditorHint())
		{
			UpdateRangeShapeOnly();
			return;
		}

		// Runtime: mantém seu comportamento atual.
		if (RangeRadius <= 0f)
			RangeRadius = DefaultRangeRadius;

		RangeArea = GetNodeOrNull<Area2D>(RangeAreaPath);
		if (RangeArea == null)
		{
			GD.PushWarning($"[{Name}] RangeArea não encontrado em '{RangeAreaPath}'. Crie um Area2D com esse nome.");
			SetProcess(false);
			return;
		}

		CannonNode = GetNodeOrNull<Node2D>(CannonPath);
		if (CannonNode == null)
		{
			GD.PushWarning($"[{Name}] Cannon não encontrado em '{CannonPath}'. Ajuste o CannonPath.");
			SetProcess(false);
			return;
		}

		ConfigureRangeArea();

		RangeArea.BodyEntered += OnRangeBodyEntered;
		RangeArea.BodyExited += OnRangeBodyExited;
	}

	private void UpdateRangeShapeOnly()
	{
		var rangeArea = GetNodeOrNull<Area2D>(RangeAreaPath);
		if (rangeArea == null)
			return;

		var rangeShape = rangeArea.GetNodeOrNull<CollisionShape2D>("RangeShape")
					 ?? rangeArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (rangeShape == null)
			return;

		if (rangeShape.Shape is not CircleShape2D circle)
		{
			circle = new CircleShape2D();
			rangeShape.Shape = circle;
		}

		circle.Radius = EffectiveRangeRadius;
	}

	public override void _ExitTree()
	{
		if (RangeArea != null)
		{
			RangeArea.BodyEntered -= OnRangeBodyEntered;
			RangeArea.BodyExited -= OnRangeBodyExited;
		}

		base._ExitTree();
	}

	public override void _Process(double delta)
	{
		if (!IsTargetValid(CurrentTarget))
		{
			ClearTarget();
			PickNextTargetIfAny();
		}

		if (CurrentTarget == null)
			return;

		AimAt(CurrentTarget);
	}

	protected virtual void AimAt(Enemy target)
	{
		Vector2 dir = target.GlobalPosition - CannonNode.GlobalPosition;
		if (dir.LengthSquared() < 0.0001f)
			return;

		var offsetRad = Mathf.DegToRad(CannonRotationOffsetDegrees);
		CannonNode.GlobalRotation = dir.Angle() + offsetRad;
	}

	protected virtual void OnEnemyEnteredRange(Enemy enemy)
	{
		// Regras simples: se ainda não tem alvo, pega o primeiro que entrar.
		if (CurrentTarget == null)
			SetTarget(enemy);
	}

	protected virtual void OnEnemyExitedRange(Enemy enemy)
	{
		if (CurrentTarget == enemy)
		{
			ClearTarget();
			PickNextTargetIfAny();
		}
	}

	private void OnRangeBodyEntered(Node2D body)
	{
		if (body is not Enemy enemy)
			return;

		_enemiesInRange.Add(enemy);
		OnEnemyEnteredRange(enemy);
	}

	private void OnRangeBodyExited(Node2D body)
	{
		if (body is not Enemy enemy)
			return;

		_enemiesInRange.Remove(enemy);
		OnEnemyExitedRange(enemy);
	}

	protected bool IsTargetValid(Enemy target)
	{
		return target != null && IsInstanceValid(target) && _enemiesInRange.Contains(target);
	}

	protected void SetTarget(Enemy enemy)
	{
		if (enemy == null || !IsInstanceValid(enemy))
			return;

		CurrentTarget = enemy;
	}

	protected void ClearTarget()
	{
		CurrentTarget = null;
	}

	private void PickNextTargetIfAny()
	{
		foreach (var enemy in _enemiesInRange)
		{
			if (enemy == null || !IsInstanceValid(enemy))
				continue;

			SetTarget(enemy);
			return;
		}
	}

	private void ConfigureRangeArea()
	{
		RangeArea.Monitoring = true;
		RangeArea.Monitorable = true;
		RangeArea.CollisionMask = EnemyCollisionMask;

		// Ajusta o shape do RangeArea se existir um CollisionShape2D filho.
		var rangeShape = RangeArea.GetNodeOrNull<CollisionShape2D>("RangeShape")
					 ?? RangeArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

		if (rangeShape == null)
		{
			GD.PushWarning($"[{Name}] RangeArea não tem CollisionShape2D (ex.: 'RangeShape').");
			return;
		}

		if (rangeShape.Shape is not CircleShape2D circle)
		{
			circle = new CircleShape2D();
			rangeShape.Shape = circle;
		}

		circle.Radius = EffectiveRangeRadius;
	}
}
