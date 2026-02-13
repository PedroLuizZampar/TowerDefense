using Godot;
using System.Collections.Generic;

/// <summary>
/// Classe base abstrata para todas as torres.
/// Define comportamento comum: detecção de inimigos, targeting e rotação do canhão.
/// </summary>
[Tool]
public abstract partial class Tower : StaticBody2D
{
	/// <summary>Raio do alcance em pixels. Se <= 0, usa DefaultRangeRadius</summary>
	[Export]
	public float RangeRadius
	{
		get => _rangeRadius;
		set
		{
			_rangeRadius = value;
			if (IsInsideTree())
				CallDeferred(nameof(UpdateRangeShapeOnly));
		}
	}

	private float _rangeRadius = 0f;

	private float EffectiveRangeRadius => _rangeRadius > 0f ? _rangeRadius : DefaultRangeRadius;

	/// <summary>Caminho para o nó Area2D que detecta inimigos</summary>
	[Export] public NodePath RangeAreaPath { get; set; } = "RangeArea";

	/// <summary>Caminho para o nó que rotaciona (canhão/arma)</summary>
	[Export] public NodePath CannonPath { get; set; } = "Cannon";

	/// <summary>Offset em graus para corrigir sprite que aponta para direção errada</summary>
	[Export] public float CannonRotationOffsetDegrees { get; set; } = 90f;

	/// <summary>Máscara de colisão para detectar inimigos (física layer)</summary>
	[Export(PropertyHint.Layers2DPhysics)]
	public uint EnemyCollisionMask { get; set; } = GameConstants.EnemyCollisionMask;

	/// <summary>Alcance padrão desta torre em pixels</summary>
	protected abstract float DefaultRangeRadius { get; }

	protected Area2D RangeArea { get; private set; }
	protected Node2D CannonNode { get; private set; }
	protected Enemy CurrentTarget { get; private set; }

	private readonly HashSet<Enemy> _enemiesInRange = new();

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			UpdateRangeShapeOnly();
			return;
		}

		if (RangeRadius <= 0f)
			RangeRadius = DefaultRangeRadius;

		RangeArea = GetNodeOrNull<Area2D>(RangeAreaPath);
		if (RangeArea == null)
		{
			GD.PushWarning($"[{Name}] RangeArea não encontrado em '{RangeAreaPath}'.");
			SetProcess(false);
			return;
		}

		CannonNode = GetNodeOrNull<Node2D>(CannonPath);
		if (CannonNode == null)
		{
			GD.PushWarning($"[{Name}] Cannon não encontrado em '{CannonPath}'.");
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

		Enemy previousTarget = CurrentTarget;
		CurrentTarget = enemy;

		if (previousTarget != enemy)
			GameEvents.InvokeTowerTargetChanged(this, enemy);
	}

	protected void ClearTarget()
	{
		Enemy previousTarget = CurrentTarget;
		CurrentTarget = null;

		if (previousTarget != null)
			GameEvents.InvokeTowerTargetChanged(this, null);
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

		var rangeShape = RangeArea.GetNodeOrNull<CollisionShape2D>("RangeShape")
					 ?? RangeArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

		if (rangeShape == null)
		{
			GD.PushWarning($"[{Name}] RangeArea não tem CollisionShape2D.");
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
