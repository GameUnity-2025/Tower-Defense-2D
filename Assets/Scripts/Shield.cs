using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Shield : MonoBehaviour
    {
        [SerializeField] private int _defaultHP = 200;
        [SerializeField] private float _defaultDuration = 8f;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private AudioClip _onClip;
        [SerializeField] private AudioClip _offClip;
        [SerializeField] private AudioClip _hitClip;

        private int _currentHP;
        private bool _active;
        private Coroutine _routine;

        public bool IsActive => _active;
        public int CurrentHP => _currentHP;

        // Activate shield with optional overrides
        public void Activate(int hp = -1, float duration = -1f)
        {
            if (_renderer == null)
            {
                Debug.LogWarning("Shield: _renderer chưa gán.");
                return;
            }

            if (hp <= 0) hp = _defaultHP;
            if (duration <= 0f) duration = _defaultDuration;

            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(ShieldRoutine(hp, duration));
        }

        // Absorb damage, trả về phần dư damage cần áp lên HP chính
        public int AbsorbDamage(int damage)
        {
            if (!_active || _currentHP <= 0) return damage;

            _currentHP -= damage;
            if (_hitClip != null) AudioSource.PlayClipAtPoint(_hitClip, transform.position);

            if (_currentHP <= 0)
            {
                int leftover = -_currentHP;
                _currentHP = 0;
                BreakShield();
                return leftover;
            }

            return 0;
        }

        private IEnumerator ShieldRoutine(int hp, float duration)
        {
            _currentHP = hp;
            _active = true;
            _renderer.enabled = true;

            if (_onClip != null)
                AudioSource.PlayClipAtPoint(_onClip, transform.position);

            // Bung khiên ra từ nhỏ đến đầy
            _renderer.transform.localScale = Vector3.zero;
            Color c = _renderer.color;
            c.a = 0f;
            _renderer.color = c;

            float growTime = 0.3f;
            for (float t = 0; t < growTime; t += Time.deltaTime)
            {
                float p = t / growTime;
                _renderer.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, p);
                c.a = Mathf.Lerp(0f, 0.7f, p);
                _renderer.color = c;
                yield return null;
            }
            _renderer.transform.localScale = Vector3.one;
            c.a = 0.7f;
            _renderer.color = c;

            float timer = 0f;
            float rotationSpeed = 30f;

            while (timer < duration && _active)
            {
                timer += Time.deltaTime;
                _renderer.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

                if (duration - timer <= 2f)
                {
                    float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                    c.a = 0.3f + alpha * 0.4f;
                    _renderer.color = c;
                }

                yield return null;
            }

            // tan biến mượt
            if (_active)
                yield return FadeOut();

            BreakShield(false);
        }

        private IEnumerator FadeOut()
        {
            Color c = _renderer.color;
            Vector3 startScale = _renderer.transform.localScale;

            for (float t = 0; t < 0.3f; t += Time.deltaTime)
            {
                float p = t / 0.3f;
                c.a = Mathf.Lerp(0.7f, 0f, p);
                _renderer.color = c;
                _renderer.transform.localScale = Vector3.Lerp(startScale, startScale * 1.2f, p);
                yield return null;
            }
        }

        private void BreakShield(bool playSound = true)
        {
            _active = false;
            if (_renderer != null)
            {
                _renderer.enabled = false;
                _renderer.transform.rotation = Quaternion.identity;
                _renderer.transform.localScale = Vector3.one;
            }
            if (playSound && _offClip != null)
                AudioSource.PlayClipAtPoint(_offClip, transform.position);

            if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        }

        // Optional: manual deactivate
        public void Deactivate()
        {
            _active = false;
            if (_renderer != null) _renderer.enabled = false;
            if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        }
    }
}