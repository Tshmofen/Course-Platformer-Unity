﻿using Interface.Overlay;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Interface.Manager
{
    [RequireComponent(typeof(Canvas))]
    public class PopUpManager : MonoBehaviour
    {
        #region Fields and properties

        private Camera _camera;
        
        [Header("Visuals")]
        public GameObject damagePopUp;
        public float popUpHeight = 0.5f;

        #endregion

        private void Awake()
        {
            InterfaceUtil.PopUpManager = this;
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        // shows popup over the target that flies away and disappears
        public void ShowPopUp(Vector2 worldPosition, string text, float lifeTime)
        {
            var newPopUp = Instantiate(damagePopUp, transform);
            newPopUp.GetComponent<Text>().text = text;
            
            var tempText = newPopUp.GetComponent<TemporaryText>();
            tempText.lifeTime = lifeTime;
            
            tempText.start = _camera.WorldToScreenPoint(worldPosition);
            worldPosition.y += popUpHeight;
            tempText.end = _camera.WorldToScreenPoint(worldPosition);
        }
    }
}