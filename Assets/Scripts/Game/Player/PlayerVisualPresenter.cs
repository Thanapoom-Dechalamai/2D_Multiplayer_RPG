using UnityEngine;

namespace Game.Player
{
    public class PlayerVisualPresenter : MonoBehaviour
    {
        [SerializeField] private float _lerpSpeed = 25f;
        [SerializeField] private Animator _anim;
        [SerializeField] private SpriteRenderer _sprite;

        private PlayerController _player;
        private Transform _parent;

        // AAA Optimization: Use Hashes
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");

        private void Awake()
        {
            _player = GetComponentInParent<PlayerController>();
            _parent = _player.transform;
            if (!_anim) _anim = GetComponent<Animator>();
        }

        private void LateUpdate()
        {
            if (_player == null || _player.Rigidbody == null) return;

            // 1. Smooth Follow
            transform.position = Vector3.Lerp(transform.position, _parent.position, _lerpSpeed * Time.deltaTime);

            // 2. Animation Logic - ใช้ InputDirection ร่วมกับ Velocity จะแม่นยำกว่าในเรื่องทิศทาง
            Vector2 vel = _player.Rigidbody.linearVelocity;
            bool moving = vel.sqrMagnitude > 0.05f; // เพิ่ม Threshold นิดหน่อยป้องกัน Animation กระตุก

            _anim.SetBool(IsMoving, moving);
            if (moving)
            {
                // ใช้ค่าที่ Normalized แล้วสำหรับ Blend Tree เพื่อให้รูปไม่เพี้ยน
                Vector2 animDir = vel.normalized;
                _anim.SetFloat(MoveX, animDir.x);
                _anim.SetFloat(MoveY, animDir.y);

                if (Mathf.Abs(animDir.x) > 0.01f) _sprite.flipX = animDir.x < 0;
            }
        }
    }
}