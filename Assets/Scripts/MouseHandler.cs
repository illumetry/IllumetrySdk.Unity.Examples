using System.Runtime.InteropServices;
using UnityEngine;
using Illumetry.Unity;
using System;

public class MouseHandler : MonoBehaviour
{
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Point pos);

    public event Action<int, int> OnUpdatedPosition;

    [SerializeField] private Illumetry.Unity.Display _display;
    private bool focused = true;
    
   
    [StructLayout(LayoutKind.Sequential)]
    public struct Point {
        public int X;
        public int Y;
        public static implicit operator Vector2(Point p) {
            return new Vector2(p.X, p.Y);
        }
    }

    private void OnApplicationFocus(bool focus) {
        focused = focus;
    }


    private void Awake() {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void OnEnable() {
        RendererLCD.OnBeforeRendererL += OnBeforeRender;
        RendererLCD.OnBeforeRendererR += OnBeforeRender;
        MonoRendererLCD.OnBeforeRenderer += OnBeforeRender;
    }

    private void OnDisable() {
        RendererLCD.OnBeforeRendererL -= OnBeforeRender;
        RendererLCD.OnBeforeRendererR -= OnBeforeRender;
        MonoRendererLCD.OnBeforeRenderer -= OnBeforeRender;
    }

    private Rect GetCameraViewportRect(bool leftEye) {
        float normalizedEyeHeight = _display.DisplayProperties.Resolution.y / (float)Screen.height;
        return new Rect(0, leftEye ? (1.0f - normalizedEyeHeight) : 0.0f, 1.0f, normalizedEyeHeight);
    }

    private void OnBeforeRender() {

        if (_display == null || _display.DisplayProperties == null) {
            return;
        }

        Point cursor;
        GetCursorPos(out cursor);
        
        if (!Application.isEditor && focused) {

            var leftEyeRect = GetCameraViewportRect(true);
            int minY = (int)(Screen.height - leftEyeRect.height * Screen.height);

            if (cursor.Y < minY) {
                cursor.Y = minY;
                SetCursorPos(cursor.X, cursor.Y);
            }
        }

        OnUpdatedPosition?.Invoke(cursor.X, cursor.Y);
    }
}
