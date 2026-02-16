using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Winter.Input;

namespace Winter.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Stats")]
        public int healthPoints;
        public float walkSpeed;
        public float runSpeed;
        public float jumpHeight;

        [Header("Other:")]
        public Transform graphics;
        public Transform mainColliderTransform;
        public LayerMask playerMask;

        private Vector3 worldSpaceGravityVector;
        private Rigidbody rb;

        public Vector3 worldSpaceMoveDirection { get; private set; }

        private bool isJumping;
        private bool isGrounded;
        private Collider[] nonAllocColQueryBuffer;
        private float jumpForce;
        private float groundValidationInterval = 0.2f;
        private float timer = 0;
        private bool timerOn;

        void Start()
        {
            nonAllocColQueryBuffer = new Collider[2];
            rb = GetComponent<Rigidbody>();
            worldSpaceGravityVector = -Vector3.up;
            jumpForce = Mathf.Sqrt(2 * 9.8f * jumpHeight);
        }

        void Update()
        {


            var alongCam = Camera.main.transform.rotation * new Vector3(InputManager.Instance.MoveRaw.x, 0, InputManager.Instance.MoveRaw.y);
            worldSpaceMoveDirection = Vector3.ProjectOnPlane(alongCam, mainColliderTransform.up).normalized;
            //worldSpaceMoveDirection = transform.rotation * alongCam;

            Debug.DrawLine(transform.position, transform.position + worldSpaceMoveDirection.normalized * 10f, Color.red);


            mainColliderTransform.rotation = Quaternion.Lerp(mainColliderTransform.rotation, Quaternion.LookRotation(worldSpaceMoveDirection == Vector3.zero ? Vector3.ProjectOnPlane(mainColliderTransform.forward, -worldSpaceGravityVector).normalized : worldSpaceMoveDirection.normalized, -worldSpaceGravityVector), Time.deltaTime * 8f);
            graphics.transform.rotation = mainColliderTransform.rotation;
        }

        private void CheckIfGrounded()
        {
            if (timerOn && timer < groundValidationInterval)
            {
                timer += Time.deltaTime;
                isGrounded = false;
                return;
            }

            timerOn = false;
            timer = 0;
            isGrounded = Physics.OverlapSphereNonAlloc(mainColliderTransform.position - mainColliderTransform.up, 0.2f, nonAllocColQueryBuffer, playerMask, QueryTriggerInteraction.Ignore) > 0;
        }

        void FixedUpdate()
        {
            CheckIfGrounded();

            var garvitationalForce = worldSpaceGravityVector * 9.8f;
            rb.AddForce(garvitationalForce, ForceMode.Acceleration);

            var targetVel = worldSpaceMoveDirection.normalized * walkSpeed;
            ///targetVel = Vector2.Lerp(rb.linearVelocity, targetVel, Time.fixedDeltaTime * 4f);
            targetVel = mainColliderTransform.InverseTransformVector(targetVel);
            targetVel.y = mainColliderTransform.InverseTransformVector(rb.linearVelocity).y;
            targetVel = mainColliderTransform.TransformVector(targetVel);
            rb.linearVelocity = targetVel;

            if (isGrounded)
            {
                isJumping = false;
            }

            if (InputManager.Instance.JumpPressedThisFrame && !isJumping && isGrounded)
            {
                rb.AddForce(mainColliderTransform.up * jumpForce, ForceMode.VelocityChange);
                isJumping = true;
                timerOn = true;
            }
        }



        [ContextMenu("FlipGravity")]
        public void FlipGravity()
        {
            this.worldSpaceGravityVector = -this.worldSpaceGravityVector;
        }

        [ContextMenu("Gravity along +X")]
        public void GravityAlongX()
        {
            this.worldSpaceGravityVector = Vector3.right;
        }
    }
}

