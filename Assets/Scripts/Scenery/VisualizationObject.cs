﻿using System;
using UnityEngine;

namespace Scenery {
    public class VisualizationObject : VisualizationElement {
        private Renderer _ownRenderer;
        private BoxCollider _boxCollider;
        private Renderer[] _renderers;
        private MeshFilter[] _meshFilters;
        
        /// <summary>
        /// Not needed for scenery, Ingore
        /// </summary>
        public override Vector3 WorldAnchor => Vector3.zero;
        
        public override bool IsDistractor => true;

        private void Start() {
            _ownRenderer = GetComponent<Renderer>();
            _renderers = GetComponentsInChildren<Renderer>();
            _meshFilters = GetComponentsInChildren<MeshFilter>();
            _boxCollider = GetComponent<BoxCollider>();
        }

        public override void HandleNonHit() {
            try {
                var c = _ownRenderer.material.color;
                c.a = 1f;
                _ownRenderer.material.color = c;
                _ownRenderer.material.SetFloat("_Surface", 0f);
            } catch (Exception e) { }
        }

        protected override Vector3[] GetReferencePointsRenderer() {
            throw new NotImplementedException();
        }

        protected override Vector3[] GetReferencePointsCustom() {
            throw new NotImplementedException();
        }

        public override void HandleHit() {
            try {
                var c = _ownRenderer.material.color;
                c.a = 0.5f;
                _ownRenderer.material.color = c;
                _ownRenderer.material.SetFloat("_Surface", 1f);
            } catch (Exception e) { }
        }

        public void HandleHit(float alpha) {
            try {
                var c = _ownRenderer.material.color;
                c.a = alpha;
                _ownRenderer.material.color = c;
                _ownRenderer.material.SetFloat("_Surface", 1f);
            } catch (Exception e) { }
        }

        public Renderer[] GetRenderers() {
            return _renderers;
        }

        public BoxCollider GetBoxCollider() {
            return _boxCollider;
        }

        public MeshFilter[] GetMeshFilters() {
            return _meshFilters;
        }
    }
}