using CrawfisSoftware.TempleRun.Input;
using CrawfisSoftware.Events;

using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;

namespace CrawfisSoftware.TempleRun
{
    public class MovementInputActions : MonoBehaviour
    {
        [SerializeField] private LeftRightJumpSlide _inputAsset;

        const int PlayerNumber = 0;
        private LeftRightJumpSlide _inputActions;
        private InputAction _leftAction;
        private InputAction _rightAction;

        private void OnEnable()
        {
            if(_inputActions == null)
                _inputActions = new LeftRightJumpSlide();
            _inputActions.Enable();
            _leftAction = _inputActions.Player.Left;
            _leftAction.Enable();
            _leftAction.performed += LeftAction_performed;
            _rightAction = _inputActions.Player.Right;
            _rightAction.Enable();
            _rightAction.performed += RightAction_performed;
        }
        private void OnDisable()
        {
            _inputActions.Disable();
            _leftAction.Disable();
            _leftAction.performed -= LeftAction_performed;
            _rightAction.Disable();
            _rightAction.performed -= RightAction_performed;

        }
        private void OnDestroy()
        {
            if (_inputActions != null)
            {
                _inputActions.Dispose();
            }
        }
        private void LeftAction_performed(InputAction.CallbackContext obj)
        {
            _leftAction.Disable();
            EventsPublisherUserInitiated.Instance.PublishEvent(UserInitiatedEvents.LeftTurnRequested, this, PlayerNumber);
            StartCoroutine(EnableAfterDelay(_leftAction));
        }

        private void RightAction_performed(InputAction.CallbackContext obj)
        {
            _rightAction.Disable();
            EventsPublisherUserInitiated.Instance.PublishEvent(UserInitiatedEvents.RightTurnRequested, this, PlayerNumber);
            StartCoroutine(EnableAfterDelay(_rightAction));
        }

        private IEnumerator EnableAfterDelay(InputAction actionToEnable)
        {
            yield return new WaitForSeconds(Blackboard.Instance.GameConfig.InputCoolDownForTurns);
            actionToEnable.Enable();
        }
    }
}