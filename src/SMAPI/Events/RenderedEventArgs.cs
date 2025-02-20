using System;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewModdingAPI.Events;

/// <summary>Event arguments for an <see cref="IDisplayEvents.Rendered"/> event.</summary>
public class RenderedEventArgs : EventArgs
{
    /*********
    ** Accessors
    *********/
    /// <summary>The sprite batch being drawn. Add anything you want to appear on-screen to this sprite batch.</summary>
    public SpriteBatch SpriteBatch => Game1.spriteBatch;
}
