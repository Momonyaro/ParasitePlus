using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class SettingsManager
{
    public const string GENERAL_VOLUME_ID = "volume_general";
    public const string VOCAL_VOLUME_ID = "volume_vocal";
    public const string MUSIC_VOLUME_ID = "volume_music";
    public const string CURSOR_SPEED_ID = "cursor_speed";
    public const string GAME_COMPLETED_ID = "game_completed";

    public static (string optionPrefix, int offsetValue)[] CURSOR_SPEEDS { get; private set; } = new (string optionPrefix, int offsetValue)[]
    {
        ("SLOWER", 700),
        ("SLOW", 800),
        ("FAST", 1000),
        ("V. FAST", 1200)
    };
}
