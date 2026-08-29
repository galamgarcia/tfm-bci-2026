/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEditor;
using UnityEngine;
using Bit.Gameplay;

namespace Bit.Editor
{
    /// <summary>Provides preview controls for Bit's complete visual controller set.</summary>
    [CustomEditor(typeof(BitEyeController))]
    public sealed class BitControllerEditor : UnityEditor.Editor
    {
        // Duration of the editor-only jump flight before landing.
        private const float JumpPreviewFlightDuration = 0.7f;
        // Maximum local height used by the editor-only jump flight.
        private const float JumpPreviewHeight = 0.35f;
        // Duration of the editor-only landing squash and rebound.
        private const float LandingPreviewDuration = 0.5f;
        // Body controller used by the active editor jump preview.
        private BitBodyController _previewBody;
        // Visual root position captured when the editor jump starts.
        private Vector3 _previewOrigin;
        // Editor timestamp at which the jump preview started.
        private double _previewStartTime;
        // Whether the editor is currently animating a jump preview.
        private bool _isJumpPreviewing;
        // Whether the landing pose has been sent during the current preview.
        private bool _hasPreviewLanded;

        private void OnEnable()
        {
            EditorApplication.update += RefreshPreview;
        }

        private void OnDisable()
        {
            EditorApplication.update -= RefreshPreview;
            ResetJumpPreview();
        }

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null)
            {
                return;
            }

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Look preview", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            DrawDirectionButton("Neutral", BitLookDirection.Neutral);
            DrawDirectionButton("Left", BitLookDirection.Left);
            DrawDirectionButton("Right", BitLookDirection.Right);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DrawDirectionButton("Up", BitLookDirection.Up);
            DrawDirectionButton("Down", BitLookDirection.Down);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Blink"))
            {
                ((BitEyeController)target).Blink();
            }

            BitVfxManager vfx = ((BitEyeController)target).GetComponentInChildren<BitVfxManager>();
            EditorGUILayout.EndHorizontal();

            BitBodyController body = ((BitEyeController)target).GetComponent<BitBodyController>();
            if (body == null)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("States preview", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset"))
            {
                ResetJumpPreview();
                ((BitEyeController)target).ResetEyeState();
                body.ResetBodyState();
                ((BitEyeController)target).ResetRelaxation();
                vfx?.StopRelaxationVfx();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Floating Idle"))
            {
                ((BitEyeController)target).StartEyesIdle();
                body.StartFloatingIdle();
            }

            if (GUILayout.Button("Start Breathing Idle"))
            {
                ((BitEyeController)target).StartEyesIdle();
                body.StartBreathingIdle();
            }

            if (GUILayout.Button("Stop Idle"))
            {
                ((BitEyeController)target).StopEyesIdle();
                body.StopBodyIdle();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Jump"))
            {
                StartJumpPreview(body);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Concentration"))
            {
                body.PlayConcentrationTransition();
            }

            if (GUILayout.Button("Relaxed"))
            {
                ((BitEyeController)target).SetRelaxation(1f);
                vfx?.SetRelaxationIntensity(1f);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>Starts the complete editor jump preview from the current visual position.</summary>
        /// <param name="body">Body controller whose visual root will be previewed.</param>
        private void StartJumpPreview(BitBodyController body)
        {
            ResetJumpPreview();
            _previewBody = body;
            Transform visualRoot = body.GetVisualRoot();
            if (visualRoot == null)
            {
                _previewBody = null;
                return;
            }

            _previewOrigin = visualRoot.localPosition;
            _previewStartTime = EditorApplication.timeSinceStartup;
            _isJumpPreviewing = true;
            _hasPreviewLanded = false;
            body.StartJump();
        }

        /// <summary>Updates the editor jump arc and triggers the landing pose at its end.</summary>
        private void RefreshJumpPreview()
        {
            if (!_isJumpPreviewing || _previewBody == null)
            {
                return;
            }

            Transform visualRoot = _previewBody.GetVisualRoot();
            if (visualRoot == null)
            {
                ResetJumpPreview();
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - _previewStartTime);
            if (!_hasPreviewLanded)
            {
                if (elapsed < JumpPreviewFlightDuration)
                {
                    float progress = elapsed / JumpPreviewFlightDuration;
                    float height = Mathf.Sin(progress * Mathf.PI) * JumpPreviewHeight;
                    visualRoot.localPosition = _previewOrigin + Vector3.up * height;
                    return;
                }

                visualRoot.localPosition = _previewOrigin;
                ((BitEyeController)target).PlayLandingExpression();
                _previewBody.PlayLandingSquash();
                _hasPreviewLanded = true;
                _previewStartTime = EditorApplication.timeSinceStartup;
                return;
            }

            visualRoot.localPosition = _previewOrigin;
            if (elapsed >= LandingPreviewDuration)
            {
                _previewBody.ResetBodyState();
                ResetJumpPreview();
            }
        }

        /// <summary>Cancels the editor jump preview without changing serialized prefab values.</summary>
        private void ResetJumpPreview()
        {
            if (_previewBody != null)
            {
                _previewBody.ResetBodyState();
            }

            _previewBody = null;
            _isJumpPreviewing = false;
            _hasPreviewLanded = false;
        }

        /// <summary>Draws a preview button for a specific cardinal direction.</summary>
        /// <param name="label">Text displayed on the button.</param>
        /// <param name="direction">Direction sent to the eye controller.</param>
        private void DrawDirectionButton(string label, BitLookDirection direction)
        {
            if (!GUILayout.Button(label))
            {
                return;
            }

            Undo.RecordObject(target, "Set Bit look direction");
            ((BitEyeController)target).SetLookDirection(direction);
        }

        /// <summary>Requests Scene View repainting while the edit-mode preview is active.</summary>
        private void RefreshPreview()
        {
            if (!Application.isPlaying)
            {
                RefreshJumpPreview();
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
        }
    }
}
