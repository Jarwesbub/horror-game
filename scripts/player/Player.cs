using Godot;

public partial class Player : CharacterBody3D
{
	private Node3D _head;
	private Camera3D _camera;
	private const float MOUSE_SENSITIVITY = 0.1F;
	private const float WALK_SPEED = 5F;
	private const float SPRINT_SPEED = 10F;
	private const float JUMP_VALUE = 5F;
	float speed;
	private float gravity = 9.8F;
	private float _cameraAngle = 0F;
	private const double BOB_FREQ = 2.4; // Frequency
	private const double BOB_AMP = 0.06; // Amplitude
	double t_bob;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_head = GetNode<Node3D>("Head");
		_camera = GetNode<Camera3D>("Head/Camera3D");
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("ui_cancel"))
			Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public override void _PhysicsProcess(double delta)
	{
		Movement((float)delta);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseMotion motion) return;

		_head.RotateY(Mathf.DegToRad(-motion.Relative.X * MOUSE_SENSITIVITY));
		float change = -motion.Relative.Y * MOUSE_SENSITIVITY;

		if (!((change + _cameraAngle) < 90F) || !((change + _cameraAngle) > -90F)) return;

		_camera.RotateX(Mathf.DegToRad(change));
		_cameraAngle += change;
	}

	private void Movement(float delta)
	{
		if (!IsOnFloor())
		{
			// Add gravity.
			Velocity -= new Vector3(Velocity.X, gravity * delta, Velocity.Z);
		}

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = (_head.Transform.Basis * Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		Vector3 newVelocity = Velocity;

		CheckSprintButtonPress();

		// Player jump action. 
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			newVelocity.Y = JUMP_VALUE;
		}

		if (IsOnFloor())
		{
			if (direction != Vector3.Zero)
			{
				// Movement input is active.
				newVelocity.X = direction.X * speed;
				newVelocity.Z = direction.Z * speed;
			}
			else
			{
				// Stop player movements slowly.
				newVelocity.X = (float)Mathf.Lerp(newVelocity.X, direction.X * speed, delta * 7.0);
				newVelocity.Z = (float)Mathf.Lerp(newVelocity.Z, direction.Z * speed, delta * 7.0);
			}
		}
		else
		{
			// Aerial movement speed.
			newVelocity.X = (float)Mathf.Lerp(newVelocity.X, direction.X * speed, delta * 20.0);
			newVelocity.Z = (float)Mathf.Lerp(newVelocity.Z, direction.Z * speed, delta * 20.0);
		}

		// Head bob.
		t_bob += delta * Velocity.Length() * (IsOnFloor() ? 1.0f : 0.0f);
		var cameraTransform = _camera.Transform;
		cameraTransform.Origin = HeadBob(t_bob);
		_camera.Transform = cameraTransform;
		Velocity = newVelocity;
		MoveAndSlide(); // Move the player object.
	}

	Vector3 HeadBob(double time)
	{
		Vector3 pos = Vector3.Zero;
		pos.Y = (float)(Mathf.Sin(time * BOB_FREQ) * BOB_AMP);
		pos.X = (float)(Mathf.Cos(time * BOB_FREQ / 2) * BOB_AMP);
		return pos;
	}

	private void CheckSprintButtonPress()
	{
		// Player sprint action.
		if (Input.IsActionPressed("sprint"))
		{
			speed = SPRINT_SPEED;
		}
		else
		{
			speed = WALK_SPEED;
		}
	}
}

