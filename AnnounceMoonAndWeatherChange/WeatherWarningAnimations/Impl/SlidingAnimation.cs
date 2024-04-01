using UnityEngine;

namespace AnnounceMoonAndWeatherChange.WeatherWarningAnimations.Impl;

public class SlidingAnimation : WarningAnimation {
    private readonly Vector2 _slideDirection = new(1, 0);
    private bool _started;

    public override string AnimationName => "Sliding";

    internal static void Initialize() =>
        AnimationManager.AddAnimation("Sliding", typeof(SlidingAnimation));

    protected override void OnGuiUpdate() {
        // Is this our first time rendering?
        if (!_started) {
            _started = true;
            //We want to start the animation outside the screen
            position = new(-textSize.x, 10);
        }

        // Update the position for sliding
        position += _slideDirection * (animationSpeed * internalSpeedMultiplier * Time.deltaTime);

        // Adjust the texture size to match the text width and height
        var textureRect = new Rect(position.x, position.y, textSize.x, textSize.y);

        // Display the texture as background
        GUI.DrawTexture(textureRect, texture2D);

        // Draw text on top of the texture
        GUI.Label(textureRect, text, textStyle);

        if (position.x > textSize.x + Screen.width)
            stopRendering = true;
    }
}