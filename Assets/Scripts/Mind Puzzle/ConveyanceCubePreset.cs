using UnityEngine;

[CreateAssetMenu(fileName = "ConveyanceCubePreset", menuName = "ScriptableObjects/ConveyanceCubePreset", order = 1)]
public class ConveyanceCubePreset : ScriptableObject
{
    public Color color;
    public Material primaryMaterial;
    public Material secondaryMaterial;
    public Material coreMaterial;
}
