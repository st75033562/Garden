using Robomation;
using System;
using UnityEngine;

// runtime state used by blocks to save state
public class RobotRuntimeState
{
    public const int DefaultTempo = 60;
    public const int DefaultWheelSpeed = 30;

    // absolute value of the max speed of wheels
    public const int MaxSpeed = 100;
    public const float MaxBuzzerPitch = 167772.15f;
    public const int MinTempo = 1;

    private int m_leftWheelSpeed;
    private int m_rightWheelSpeed;
    private float m_buzzerPitch;
    private int m_tempo;

    public RobotRuntimeState()
    {
        reset();
    }

    public int tempo
    {
        get { return m_tempo; }
        set { m_tempo = Mathf.Max(MinTempo, value); }
    }

    public int leftWheelSpeed
    {
        get { return m_leftWheelSpeed; }
        set { m_leftWheelSpeed = Mathf.Clamp(value, -MaxSpeed, MaxSpeed); }
    }

    public int rightWheelSpeed
    {
        get { return m_rightWheelSpeed; }
        set { m_rightWheelSpeed = Mathf.Clamp(value, -MaxSpeed, MaxSpeed); }
    }

    public float buzzerPitch
    {
        get { return m_buzzerPitch; }
        set { m_buzzerPitch = Mathf.Clamp(value, 0, MaxBuzzerPitch); }
    }

    // reset the runtime state to the default state
    public void reset()
    {
        tempo = DefaultTempo;
        leftWheelSpeed = rightWheelSpeed = DefaultWheelSpeed;
        buzzerPitch = 0;
    }
}