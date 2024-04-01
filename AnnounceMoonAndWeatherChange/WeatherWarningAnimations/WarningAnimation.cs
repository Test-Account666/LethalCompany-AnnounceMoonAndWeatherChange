using System.IO;
using System.Reflection;
using UnityEngine;

namespace AnnounceMoonAndWeatherChange.WeatherWarningAnimations;

public abstract class WarningAnimation : MonoBehaviour {
    public string text = "";
    public Color textColor = Color.white;
    public int fontSize = 30;
    public float animationSpeed = 100f;
    protected float internalSpeedMultiplier = 1F;
    protected Vector2 position = new(10, 10);
    protected bool stopRendering;
    protected Vector2 textSize = Vector2.zero;
    protected GUIStyle? textStyle;
    protected Texture2D texture2D = null!;
    public abstract string AnimationName { get; }

    private void Start() {
        AnnounceMoonAndWeatherChange.Logger.LogDebug("Starting animation!");

        var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (string.IsNullOrEmpty(directory))
            return;

        var path = Path.Combine(directory, "background.png");

        // Read the PNG file into a byte array
        var fileData = File.ReadAllBytes(path);

        // Create a new Texture2D instance
        texture2D = new(256, 256);

        // Load the image data into the Texture2D instance
        texture2D.LoadImage(fileData);
    }

    private void OnDestroy() =>
        AnnounceMoonAndWeatherChange.Logger.LogDebug("Stopping animation this instant!");

    private void OnGUI() {
        if (stopRendering) {
            // Since we don't want to render anymore, let's destroy this object
            Destroy(this);
            return;
        }

        // If _textStyle is null, create a new one
        // I'd like to have this in Start(), but because of 'GUI.skin.label' I can't
        textStyle ??= new(GUI.skin.label) {
            fontSize = fontSize,
            padding = new(5, 5, 5, 5),
            alignment = TextAnchor.MiddleCenter,
            normal = new() {
                textColor = textColor,
            },
        };

        // Calculate the width of the text
        textSize = textStyle.CalcSize(new(text));

        OnGuiUpdate();
    }

    public virtual void SpeedUp() =>
        internalSpeedMultiplier = 4.5F;

    protected abstract void OnGuiUpdate();
}