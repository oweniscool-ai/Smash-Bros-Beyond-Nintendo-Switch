using Godot;
public partial class FighterBase : CharacterBody2D
{
    [Export] public float MoveSpeed = 400.0f;
    [Export] public float JumpVelocity = -900.0f;
    [Export] public float Gravity = 2500.0f;

    // Combat & Core Mechanics Matrix
    public float DamagePercent = 0.0f;
    public int CatalystTier = 0; // 0 to 3 Evolution Meter
    public float AuraCharge = 100.0f;
    
    // State Machine Enumeration
    public enum State { Idle, Run, Jump, Fall, Attack, AuraDash, Stunned }
    public State CurrentState = State.Idle;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        // Apply Gravity if not grounded
        if (!IsOnFloor())
        {
            velocity.Y += Gravity * (float)delta;
            if (CurrentState != State.AuraDash) CurrentState = velocity.Y < 0 ? State.Jump : State.Fall;
        }
        else
        {
            if (CurrentState == State.Jump || CurrentState == State.Fall) CurrentState = State.Idle;
        }

        // Handle Input & State Transitions
        HandleCombatStates((float)delta);
        
        Velocity = velocity;
        MoveAndSlide();
    }

    private void HandleCombatStates(float delta)
    {
        // Directional Input Capture
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            Vector2 velocity = Velocity;
            velocity.Y = JumpVelocity;
            Velocity = velocity;
            CurrentState = State.Jump;
        }

        // Momentum Tether (Replaces traditional shield with a high-speed micro-dash)
        if (Input.IsActionJustPressed("aura_dash") && AuraCharge >= 25.0f)
        {
            ExecuteAuraDash(direction);
        }

        // Catalyst Meter Progression Trigger
        if (DamagePercent >= 100.0f && CatalystTier < 3)
        {
            TriggerCatalystEvolution();
        }
    }

    private void ExecuteAuraDash(Vector2 dashDirection)
    {
        CurrentState = State.AuraDash;
        AuraCharge -= 25.0f;
        Vector2 dashDir = dashDirection == Vector2.Zero ? new Vector2(Transform.X.X, 0).Normalized() : dashDirection.Normalized();
        Velocity = dashDir * 1200.0f; // Instantaneous momentum burst slicing through states
    }

    private void TriggerCatalystEvolution()
    {
        CatalystTier++;
        DamagePercent = 0.0f; // Reset percentage cycle while granting permanent tier buff
        // Expand active move-set properties dynamically based on CatalystTier
    }

    public void ApplyHit(float baseDamage, float knockbackPower, Vector2 hitAngle)
    {
        if (CurrentState == State.AuraDash) return; // Invulnerability frames during Aura Dash execution
        
        DamagePercent += baseDamage;
        CurrentState = State.Stunned;
        
        // Dynamic Knockback calculation scaling exponentially with accumulated damage percentage
        float calculatedKnockback = knockbackPower * (1.0f + (DamagePercent / 50.0f));
        Velocity = hitAngle.Normalized() * calculatedKnockback;
    }
}
