using System;
using System.Collections;
using UnityEngine;

namespace AnnounceMoonAndWeatherChange.WeatherWarningAnimations.Impl;

public class BlinkingAnimation : WarningAnimation {
    private const int MAX_PULSE_TIMES = 4;
    private const float ALPHA_CHANGE_VALUE = .01F;
    private float _alpha;
    private int _pulseTimes;
    private bool _started;

    public override string AnimationName => "Blinking";

    internal static void Initialize() =>
        AnimationManager.AddAnimation("Blinking", typeof(BlinkingAnimation));

    protected override void OnGuiUpdate() {
        // Is this our first time rendering?
        if (!_started) {
            _started = true;
            StartFadeIn();
        }

        position = new(Screen.width - textSize.x, Screen.height / 2F);

        var backgroundColor = new Color(1, 0, 0, _alpha);
        var color = new Color(textColor.r, textColor.g, textColor.b, _alpha);

        GUI.color = backgroundColor;
        // Adjust the texture size to match the text width and height
        var textureRect = new Rect(position.x, position.y, textSize.x, textSize.y);

        // Display the texture as background
        GUI.DrawTexture(textureRect, texture2D);

        GUI.color = color;
        // Draw text on top of the texture
        GUI.Label(textureRect, text, textStyle);
    }

    public override void SpeedUp() =>
        stopRendering = true;

    public void StartFadeOut() =>
        StartCoroutine(FadeOut());

    public void StartFadeIn() =>
        StartCoroutine(FadeIn());

    private IEnumerator FadeOut() {
        while (_alpha > 0) {
            _alpha -= ALPHA_CHANGE_VALUE * Math.Max(animationSpeed * internalSpeedMultiplier * Time.deltaTime, 0);
            yield return null;
        }

        _alpha = 0;
        StartFadeIn();
    }

    private IEnumerator FadeIn() {
        if (_pulseTimes >= MAX_PULSE_TIMES) {
            stopRendering = true;
            yield break;
        }

        while (_alpha < 1) {
            _alpha += Math.Min(ALPHA_CHANGE_VALUE * (animationSpeed * internalSpeedMultiplier * Time.deltaTime), 1);
            yield return null;
        }

        _pulseTimes += 1;

        _alpha = 1;
        StartFadeOut();
    }
}