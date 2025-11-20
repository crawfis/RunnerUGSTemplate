using CrawfisSoftware.TempleRun.Input;
using CrawfisSoftware.Events;

using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;

namespace CrawfisSoftware.TempleRun
{
    public class SwipeDetectorActions : MonoBehaviour
    {
        [SerializeField] private LeftRightJumpSlide _playerControls;
        const int PlayerNumber = 0;
        private InputAction _swipePressed, _swipePosition;
        private Vector2 _startPosition;
        private bool _isPressing;
        private bool _isCoolingDown = false;

        void Awake()
        {
            _playerControls = new LeftRightJumpSlide();
        }

        void OnEnable()
        {
            _playerControls.PlayerTouch.Enable();
            _playerControls.PlayerTouch.InitialPress.performed += ctx => OnPress(true, ctx.ReadValue<float>());
            //_playerControls.PlayerTouch.EndOfSwipe.canceled += ctx => OnPress(false, 0);
            _swipePressed = _playerControls.PlayerTouch.InitialPress;
            _swipePressed.Enable();
            _swipePosition = _playerControls.PlayerTouch.EndOfSwipe;
            _swipePosition.Enable();
        }

        void OnDisable()
        {
            _playerControls.PlayerTouch.InitialPress.performed -= ctx => OnPress(true, ctx.ReadValue<float>());
            //_playerControls.PlayerTouch.EndOfSwipe.canceled -= ctx => OnPress(false, 0);
            _swipePressed.Disable();
            _swipePosition.Disable();
            _playerControls.PlayerTouch.Disable();
        }

        private void Update()
        {
            var swipeDelta = _swipePosition.ReadValue<Vector2>();
            if(swipeDelta.magnitude > 100 && _isPressing)
            {
                _isPressing = false;
                //Vector2 endPosition = _swipePosition.ReadValue<Vector2>();
                //Vector2 swipeVector = endPosition - _startPosition;
                DetectSwipeDirection(swipeDelta.normalized);
            }
        }
        private void OnPress(bool pressed, float pressure)
        {
            if(_isCoolingDown) return;
            if (pressed)
            {
                _isPressing = true;
                //_startPosition = _playerControls.PlayerTouch.InitialPress.ReadValue<float>();
                //_startPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                _startPosition = _swipePressed.ReadValue<Vector2>();
            }
            else if (_isPressing)
            {
                _isPressing = false;
                Vector2 endPosition = _swipePosition.ReadValue<Vector2>();
                Vector2 swipeVector = endPosition - _startPosition;

                if (swipeVector.magnitude > 100)
                {
                    DetectSwipeDirection(swipeVector.normalized);
                }
            }
        }

        private void DetectSwipeDirection(Vector2 swipeDirection)
        {
            if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
            {
                if (swipeDirection.x > 0) RightAction_performed();
                else LeftAction_performed();
            }
        }

        // Slightly modified from MovementInputActions.cs
        private void LeftAction_performed()
        {
            _swipePressed.Disable();
            EventsPublisherUserInitiated.Instance.PublishEvent(UserInitiatedEvents.LeftTurnRequested, this, PlayerNumber);
            StartCoroutine(EnableAfterDelay(_swipePressed));
        }

        private void RightAction_performed()
        {
            _swipePressed.Disable();
            EventsPublisherUserInitiated.Instance.PublishEvent(UserInitiatedEvents.RightTurnRequested, this, PlayerNumber);
            StartCoroutine(EnableAfterDelay(_swipePressed));
        }

        private IEnumerator EnableAfterDelay(InputAction actionToEnable)
        {
            yield return new WaitForSeconds(Blackboard.Instance.GameConfig.InputCoolDownForTurns);
            actionToEnable.Enable();
        }
    }
}