﻿using UnityEngine;
using UnityEngine.UI;

namespace Interface.Overlay
{
    [RequireComponent(typeof(Text))]
    public class TemporaryText : MonoBehaviour
    {
        private float _currentTime;
        private CanvasRenderer _textRender;
        
        [Header("Visuals")]
        public float lifeTime;
        public Vector2 start;
        public Vector2 end;
        
        private void Start()
        {
            _textRender = GetComponent<CanvasRenderer>();
        }

        private void Update() => UpdateRenderState();
        
        // gradually increases sprite opacity and move it vertically 
        private void UpdateRenderState()
        {
            _currentTime += Time.deltaTime;

            var opacity = Mathf.Lerp(1, 0, _currentTime / lifeTime);
            var position = Vector2.Lerp(start, end, _currentTime / lifeTime);

            _textRender.SetAlpha(opacity);
            transform.position = position;

            if (opacity == 0) Destroy(gameObject);
        }
    }
}