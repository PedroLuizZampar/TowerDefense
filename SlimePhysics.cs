using Godot;
using System;

public partial class SlimePhysics : CharacterBody2D
{
	public const float Speed = 300.0f;

	private AnimatedSprite2D _animacao;
	private Vector2 direction = Vector2.Zero;

    public override void _Ready()
    {
        _animacao = GetNode<AnimatedSprite2D>("WalkAnimation");
    }

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		velocity = direction * Speed;

		animation(direction);

		Velocity = velocity;
		MoveAndSlide();
	}

	void animation(Vector2 direction)
	{
		if (direction.X > 0)
		{
			_animacao.Play("R_Walk");
			//Animacao andar para direita
		}
		else if (direction.X < 0)
		{
			_animacao.Play("L_Walk");		
			//Animacao andar para esquerda
		}
		else if (direction.Y > 0)
		{
			_animacao.Play("D_Walk");
		} else if (direction.Y < 0)
		{
			_animacao.Play("U_Walk");
		}
	}	
}
