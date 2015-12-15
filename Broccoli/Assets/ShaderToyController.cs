using UnityEngine;

public class ShaderToyController
	: MonoBehaviour
{
    public bool autoload;
    public string key;

    private Renderer _renderer;
    private Material _material;
    private Material _generated;

    public void Update()
    {
        if (!autoload)
            return;

        if (string.IsNullOrEmpty(key))
            return;

        _renderer = _renderer ?? GetComponent<Renderer>();
        _material = _material ?? _renderer.material;

        if (_generated == null)
        {
            if (!ShaderToyCache.HasShaderToy(key))
            {
                ShaderToyCache.Request(key);
            }
            else
            {
                _generated = ShaderToyCache.GetMaterial(key);
                _material = _generated;
            }
        }
    }
}
