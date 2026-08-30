/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Controls BIT icon in the connection popup.</summary>
    public sealed class BitIconManager : MonoBehaviour
    {
        [Header("Connection Indicator")]
        [Tooltip("Horizontal image filled as the headset connection completes.")]
        [SerializeField] private Image connectionFill;

        [Header("Connection Indicator")]
        [Tooltip("Dim cyan line visible before the connection completes.")]
        [SerializeField] private Image connectionTrack;

        [Header("Connection Indicator")]
        [Tooltip("Glow image behind the connection line.")]
        [SerializeField] private Image connectionGlow;

        [Header("Connection Animation")]
        [Tooltip("Dashed cyan images animated from the headset toward BIT while connecting.")]
        [SerializeField] private Image[] connectionDashes;

        [Header("Connection Animation")]
        [Tooltip("Speed at which the active dash travels toward BIT.")]
        [SerializeField] private float dashAnimationSpeed = 5f;

        // Elapsed time used to advance the connecting dash animation.
        private float _animationTime;
        // Indicates whether the dashed connection animation is active.
        private bool _isConnecting;

        /// <summary>Sets the connection indicator fill between empty and complete.</summary>
        /// <param name="progress">Connection progress from zero to one.</param>
        public void SetConnectionProgress(float progress)
        {
            float value = Mathf.Clamp01(progress);
            _isConnecting = value <= 0f;
            if (connectionFill != null)
            {
                connectionFill.fillAmount = value;
                connectionFill.color = new Color(0.1f, 0.81f, 1f, value > 0f ? 1f : 0f);
            }

            if (connectionTrack != null)
            {
                connectionTrack.color = new Color(0.1f, 0.81f, 1f, value > 0f ? 0.2f : 0f);
            }

            if (connectionGlow != null)
            {
                connectionGlow.color = new Color(0.1f, 0.81f, 1f, value > 0f ? 0.16f : 0.08f);
            }

            if (value > 0f)
            {
                SetDashAlpha(0f);
            }
        }

        private void Update()
        {
            if (!_isConnecting || connectionDashes == null || connectionDashes.Length == 0)
            {
                return;
            }

            _animationTime += Time.unscaledDeltaTime;
            float position = Mathf.Repeat(_animationTime * dashAnimationSpeed, connectionDashes.Length);
            for (int index = 0; index < connectionDashes.Length; index++)
            {
                float distance = Mathf.Abs(index - position);
                float alpha = distance < 0.5f ? 1f : distance < 1.5f ? 0.55f : 0.28f;
                connectionDashes[index].color = new Color(0.1f, 0.81f, 1f, alpha);
            }
        }

        /// <summary>Sets the alpha of every connecting dash.</summary>
        /// <param name="alpha">Alpha value applied to the dashes.</param>
        private void SetDashAlpha(float alpha)
        {
            if (connectionDashes == null) { return; }
            foreach (Image dash in connectionDashes)
            {
                if (dash != null)
                {
                    dash.color = new Color(0.1f, 0.81f, 1f, alpha);
                }
            }
        }
    }
}
