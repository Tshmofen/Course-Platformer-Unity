﻿using System.Collections;
using System.Globalization;
using Interface.Overlay;
using UnityEngine;
using Util;

namespace Entity.Damage
{
    [RequireComponent(typeof(HealthStats), typeof(BoxCollider2D))]
    public class DamageReceiver : MonoBehaviour
    {
        #region Fields and properties
        
        [Header("Visuals")]
        public bool showHealthBar = true;
        public GameObject healthBinderPrefab;
        [Header("External")] 
        public GameObject bindersRoot;
        public EntityManager manager;
        
        private HealthStats _health;
        private BoxCollider2D _collider;
        private bool _isReceiveDamage = true;
        
        private float _notReceiveDamageTime;

        private GameObject _binder;
        private bool _isBinderSet;

        // animation hashed strings
        private static readonly int HashToDie = Animator.StringToHash("toDie");
        private static readonly int HashToInjure = Animator.StringToHash("toInjure");

        #endregion
        
        private void Start()
        {
            _health = GetComponent<HealthStats>();
            _collider = GetComponent<BoxCollider2D>();
            _isReceiveDamage = true;
        }

        // Also destroys created health bar
        private void OnDestroy()
        {
            if (_isBinderSet)
                Destroy(_binder);
        }

        public void ReceiveDamage(DamageStats damage, DamageType type)
        {
            if (_isReceiveDamage)
            {
                var damageAmount = CalculateDamage(damage, type);
                _health.CurrentHealth -= (int)damageAmount;
                if (_health.CurrentHealth < 0) _health.CurrentHealth = 0;

                ShowPopUp(damageAmount);
                EnableHealthBar();

                if (_health.CurrentHealth == 0)
                    StartDieState();
                else
                    StartInjureState();
            }
        }

        public void KillReceiver()
        {
            _health.CurrentHealth = 0;
            StartDieState();
        }

        public void NotReceiveDamageFor(float duration) => StartCoroutine(NotReceiveDamageApply(duration));

        private float CalculateDamage(DamageStats damage, DamageType type)
        {
            var damageAmount = type switch
            {
                DamageType.LightDamage => damage.damageLight * (1 - _health.lightArmor),
                DamageType.HeavyDamage => damage.damageHeavy * (1 - _health.heavyArmor),
                DamageType.PierceDamage => damage.damagePierce * (1 - _health.pierceArmor),
                _ => 1
            };

            return damageAmount < 1 ? 1 : damageAmount;
        }

        private void ShowPopUp(float damage)
        {
            var abovePosition = _collider.bounds.size;
            abovePosition /= 2;
            InterfaceUtil
                .PopUpManager
                .ShowPopUp(
                    transform.position + abovePosition,
                    damage.ToString(CultureInfo.InvariantCulture),
                    1
                );
        }

        private void EnableHealthBar()
        {
            if (showHealthBar && !_isBinderSet)
            { 
                _isBinderSet = true;
                _binder = Instantiate(healthBinderPrefab, bindersRoot.transform);
                var binderScript = _binder.GetComponent<HealthBinder>();
                
                binderScript.health = _health;
                binderScript.target = transform;
                binderScript.isFollower = true;
                binderScript.offset = _collider.bounds.size / 2;
            }
        }

        // toDie state should have corresponding behaviour
        private void StartDieState()
        {
            manager.animator.SetTrigger(HashToDie);
        }

        // toInjure state should have corresponding behaviour
        private void StartInjureState()
        {
            manager.animator.SetTrigger(HashToInjure);
        }

        private IEnumerator NotReceiveDamageApply(float duration)
        {
            for (var time = 0f; time <= duration; time += Time.deltaTime)
            {
                _isReceiveDamage = false;
                yield return null;
            }
            _isReceiveDamage = true;
        }
    }
}