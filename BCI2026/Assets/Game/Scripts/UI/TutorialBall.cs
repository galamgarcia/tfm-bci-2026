/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace BciGame.UI
{
    /// <summary>
    /// Represents the spawned visual ball and applies bounded canvas-space movement.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class TutorialBall : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Used to position the ball in tutorial screen.")]
        [SerializeField] private RectTransform initialBallTransform;
        
        // Movement component attached to this ball GameObject.
        private TutorialBallMoveComponent _moveComponent;

        // Gets or sets the ball position in its parent canvas space.
        public Vector2 Position
        {
            get => initialBallTransform.anchoredPosition;
            set => initialBallTransform.anchoredPosition = value;
        }

        private void Awake()
        {
            if (initialBallTransform == null)
            {
                initialBallTransform = GetComponent<RectTransform>();
            }

            _moveComponent = GetComponent<TutorialBallMoveComponent>();
            if (_moveComponent == null)
            {
                _moveComponent = gameObject.AddComponent<TutorialBallMoveComponent>();
            }
        }
    
        public TutorialBallMoveComponent GetMoveComponent()
        {
            return _moveComponent;
        }
    }
}
