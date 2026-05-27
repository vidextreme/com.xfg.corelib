// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

namespace XFG.Subsystems
{
    /// <summary>
    /// Defines subsystem ordering categories.
    /// These values act as base offsets for StartupOrderAttribute.
    /// </summary>
    public enum SubsystemCategory
    {
        Engine = 0,
        Framework = 100,
        Platform = 200,
        Gameplay = 300,
        Simulation = 400,
        Presentation = 500,
        Networking = 600,
        Audio = 700,
        UI = 800,
        Group = 900,
        World = 1000
    }
}
