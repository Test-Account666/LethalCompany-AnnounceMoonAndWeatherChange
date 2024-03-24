using System.IO;
using System.Reflection;

namespace AnnounceMoonAndWeatherChange;

using UnityEngine;

public class SlidingText : MonoBehaviour {
    private Texture2D _texture2D = null!;
    private GUIStyle? _textStyle;
    private Vector2 _position = new(10, 10);
    private Vector2 _slideDirection = new(1, 0);
    public int fontSize = 30;
    public float slideSpeed = 100f;
    public string text = "";
    public Color textColor = Color.white;
    private bool _started;
    private bool _stopRendering;

    private void Start() {
        var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (string.IsNullOrEmpty(directory))
            return;

        var path = Path.Combine(directory, "background.png");

        // Read the PNG file into a byte array
        var fileData = File.ReadAllBytes(path);

        // Create a new Texture2D instance
        _texture2D = new Texture2D(256, 256);

        // Load the image data into the Texture2D instance
        _texture2D.LoadImage(fileData);
    }

    public void Reverse() {
        _slideDirection *= -1;
    }

    private void OnGUI() {
        if (_stopRendering) {
            // Since we don't want to render anymore, let's destroy this object
            Destroy(this);
            return;
        }

        // If _textStyle is null, create a new one
        // I'd like to have this in Start(), but because of 'GUI.skin.label' I can't
        _textStyle ??= new GUIStyle(GUI.skin.label) {
            fontSize = fontSize,
            padding = new RectOffset(5, 5, 5, 5),
            alignment = TextAnchor.MiddleCenter,
            normal = new GUIStyleState {
                textColor = textColor
            }
        };

        // Calculate the width of the text
        var textSize = _textStyle.CalcSize(new GUIContent(text));

        // Is this our first time rendering?
        if (!_started) {
            _started = true;
            //We want to start the animation outside the screen
            _position = new Vector2(-textSize.x, 10);
        }

        // Update the position for sliding
        _position += _slideDirection * (slideSpeed * Time.deltaTime);

        // Adjust the texture size to match the text width and height
        var textureRect = new Rect(_position.x, _position.y, textSize.x, textSize.y);

        // Display the texture as background
        GUI.DrawTexture(textureRect, _texture2D);

        // Draw text on top of the texture
        GUI.Label(textureRect, text, _textStyle);

        if (_position.x > textSize.x + Screen.width)
            _stopRendering = true;
    }
}