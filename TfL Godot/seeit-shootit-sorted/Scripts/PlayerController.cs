using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
    public const float Speed = 300.0f;
    public const float JumpVelocity = -400.0f;
    private AnimatedSprite2D _sprite;
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    [Signal] public delegate void HealthChangedEventHandler();
    [Signal] public delegate void DeathEventHandler();

    [Signal]
    public delegate void GunShotEventHandler(PackedScene gunScene, Vector2 location, Vector2 direction);



    private Node muzzle;

    PackedScene gunScene = (PackedScene)ResourceLoader.Load("res://Scenes/gunAndShooting/gun.tscn");

    // public void _Process(float delta)
    // {
    //     if (Input.IsActionJustPressed("shoot"))
    //     { 
    //         GD.PrintErr("space button works i guess");
    //         Shoot();
    //     }
    //     else{
    //         GD.PrintErr("else hit");
    //     }
    // }

    public void Shoot()
    {
        GD.Print("Shoot is being hit");
       //added direction vector here and below
        Vector2 shootDirection = _sprite.FlipH ? Vector2.Left : Vector2.Right;
        EmitSignal(SignalName.GunShot, gunScene, ((Node2D)muzzle).GlobalPosition, shootDirection);
        GD.Print("Bullet emitted from position: " + ((Node2D)muzzle).GlobalPosition);
    }

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D"); // Get the sprite node
        muzzle = GetNode("Muzzle");

        MaxHealth = 100;
        CurrentHealth = MaxHealth;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_fullscreen_toggle"))
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen
                ? DisplayServer.WindowMode.Windowed
                : DisplayServer.WindowMode.Fullscreen);
        }

        // if (Input.IsActionJustPressed("shoot"))
        // { 
        //     GD.PrintErr("space button works i guess");
        //     Shoot();
        // }
        // else{
        //     GD.PrintErr("else hit");
        // }
    }

    public void ChangeHealth(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
        EmitSignal("HealthChanged");

        if (amount < 0) // Only play TakeDamage animation if losing health
        {
            GD.Print("Player TakeDamageAnimation");
            _sprite.Play("TakeDamage");
        }

        if(CurrentHealth <= 0){
            CurrentHealth = 0;
            _sprite.Play("death");
            GD.Print("Player has died");
            //_sprite.AnimationFinished += _on_animated_sprite_2d_animation_finished;
            SetPhysicsProcess(false); // Stop player movement
            SetProcess(false);
            // EmitSignal(nameof(Death));
        }

        // if(CurrentHealth == 75)
        // {
        //     GD.Print("Player TakeDamageAnimation");
        //     _sprite.Play("TakeDamage");
        // }
    }

    private void _on_animated_sprite_2d_animation_finished()
    {
        if (_sprite.Animation == "death") // Ensure we're checking the correct animation
        {
            GD.Print("Respawning player after death animation");

            // Disconnect the signal to prevent multiple calls
            //_sprite.AnimationFinished -= _on_animated_sprite_2d_animation_finished;

            EmitSignal(nameof(Death));
        }
    }

    public const float AirControlFactor = 0.5f; // The factor to reduce horizontal speed while in the air

    public override void _PhysicsProcess(double delta)
    {
        if (_sprite.Animation == "TakeDamage" && _sprite.IsPlaying())
        {
            return; // Don't override TakeDamage animation
        }

        if (Input.IsActionJustPressed("shoot"))
        { 
            GD.Print("Gun is shooting");
            Shoot();
            
        }

        Vector2 velocity = Velocity;

        // Get the default gravity value from project settings
        float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

        // Apply gravity if not on the floor
        if (!IsOnFloor())
        {
            velocity.Y += gravity * (float)delta;
        }

        // Handle Jump
        if (Input.IsActionJustPressed("ui_up") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Get input direction for movement
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        // Apply horizontal movement with reduced speed if in the air
        if (direction != Vector2.Zero)
        {
            if (IsOnFloor()) // On the ground, move at full speed
            {
                velocity.X = direction.X * Speed;
                _sprite.Play("run");
            }
            else // In the air, reduce horizontal movement speed
            {
                velocity.X = direction.X * Speed * AirControlFactor;
                _sprite.Play("jump");
            }

            // Flip the sprite depending on movement direction
            _sprite.FlipH = direction.X < 0;
        }
        else if (IsOnFloor()) // If standing still on the ground, play idle animation
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            _sprite.Play("idle");
        }
        // Assign and move
        Velocity = velocity;
        MoveAndSlide();
    }

    public void TakeDamage(){
        GD.Print("Player has taken damage");
    }

    public void RespawnPlayer(){
        GD.Print("Player respawned");
        CurrentHealth = MaxHealth;
        EmitSignal("HealthChanged");

        //_sprite.Play("idle");
        SetPhysicsProcess(true);
        SetProcess(true);
    }
}
