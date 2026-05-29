using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    [DisallowMultipleComponent]
    public class ItemOutline : MonoBehaviour
    {
        public enum Mode
        {
            OutlineAll,
            OutlineVisible,
            OutlineHidden
        }

        [SerializeField]
        private Mode outlineMode = Mode.OutlineAll;

        [SerializeField]
        private Color outlineColor = Color.white;

        [SerializeField, Range(0f, 10f)]
        private float outlineWidth = 2f;

        public Mode OutlineMode
        {
            get => outlineMode;
            set
            {
                outlineMode = value;
                _needsUpdate = true;
            }
        }

        public Color OutlineColor
        {
            get => outlineColor;
            set
            {
                outlineColor = value;
                _needsUpdate = true;
            }
        }

        public float OutlineWidth
        {
            get => outlineWidth;
            set
            {
                outlineWidth = value;
                _needsUpdate = true;
            }
        }

        private Renderer[] _renderers;
        private Material _outlineMaskMaterial;
        private Material _outlineFillMaterial;
        private bool _needsUpdate;
        private bool _isApplied;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();

            Shader maskShader = Resources.Load<Shader>("Shaders/ProjectOutlineMask") ?? Shader.Find("MatchFactoryCore/OutlineMask");
            Shader fillShader = Resources.Load<Shader>("Shaders/ProjectOutlineFill") ?? Shader.Find("MatchFactoryCore/OutlineFill");

            if (maskShader != null)
            {
                _outlineMaskMaterial = new Material(maskShader);
                _outlineMaskMaterial.name = "OutlineMask (Instance)";
            }
            else
            {
                Debug.LogError("Custom Outline Mask shader not found!");
            }

            if (fillShader != null)
            {
                _outlineFillMaterial = new Material(fillShader);
                _outlineFillMaterial.name = "OutlineFill (Instance)";
            }
            else
            {
                Debug.LogError("Custom Outline Fill shader not found!");
            }

            _needsUpdate = true;
        }

        private void OnEnable()
        {
            ApplyOutline();
        }

        private void OnDisable()
        {
            RemoveOutline();
        }

        private void OnDestroy()
        {
            RemoveOutline();
            if (_outlineMaskMaterial != null) Destroy(_outlineMaskMaterial);
            if (_outlineFillMaterial != null) Destroy(_outlineFillMaterial);
        }

        private void Update()
        {
            if (_needsUpdate)
            {
                _needsUpdate = false;
                UpdateMaterialProperties();
            }
        }

        private void ApplyOutline()
        {
            if (_isApplied || _renderers == null) return;

            foreach (var renderer in _renderers)
            {
                if (renderer == null) continue;

                var materials = renderer.sharedMaterials.ToList();
                if (_outlineMaskMaterial != null && !materials.Contains(_outlineMaskMaterial)) 
                    materials.Add(_outlineMaskMaterial);
                if (_outlineFillMaterial != null && !materials.Contains(_outlineFillMaterial)) 
                    materials.Add(_outlineFillMaterial);

                renderer.materials = materials.ToArray();
            }

            _isApplied = true;
        }

        private void RemoveOutline()
        {
            if (!_isApplied || _renderers == null) return;

            foreach (var renderer in _renderers)
            {
                if (renderer == null) continue;

                var materials = renderer.sharedMaterials.ToList();
                if (_outlineMaskMaterial != null) materials.Remove(_outlineMaskMaterial);
                if (_outlineFillMaterial != null) materials.Remove(_outlineFillMaterial);

                renderer.materials = materials.ToArray();
            }

            _isApplied = false;
        }

        private void UpdateMaterialProperties()
        {
            if (_outlineFillMaterial == null || _outlineMaskMaterial == null) return;

            _outlineFillMaterial.SetColor("_OutlineColor", outlineColor);
            _outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);

            switch (outlineMode)
            {
                case Mode.OutlineAll:
                    _outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    _outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    break;

                case Mode.OutlineVisible:
                    _outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    _outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                    break;

                case Mode.OutlineHidden:
                    _outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    _outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                    break;
            }
        }

        private void OnValidate()
        {
            _needsUpdate = true;
        }
    }
}
