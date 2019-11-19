using System;

public interface ISound
{
    void Play();

    void Stop();

    /// <summary>
    /// release the sound. After releasing, the sound should not be used any more
    /// </summary>
    void Release();

    /// <summary>
    /// playing state of the sound
    /// </summary>
    /// <remarks>
    /// isPlaying will be true even if isPaused returns true
    /// </remarks>
    bool isPlaying { get; }

    /// <summary>
    /// pause/unpause the sound
    /// </summary>
    /// <remarks>
    /// if the sound manager is paused, isPaused will return false
    /// </remarks>
    bool isPaused { get; set; }

    /// <summary>
    /// mute/unmute the sound
    /// </summary>
    /// <remarks>
    /// if the sound manager is mute, mute will return false
    /// </remarks>
    bool mute { get; set; }
}