using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpawnMolecule : MonoBehaviour
{
    public GameObject moleculeSpawnMenu;
    private Transform t;

    private void Start()
    {
        t = moleculeSpawnMenu.GetComponent<Transform>();
    }

    public void Spawn()
    {
        // Spawns molecule
        string prefabName = this.gameObject.name;
        GameObject molecule = Resources.Load<GameObject>(prefabName);
        float offset = 1.5f;
        Vector3 position = new Vector3(t.position.x, t.position.y, t.position.z + offset);
        GameObject moleculeInstance = GameObject.Instantiate(molecule, position, Quaternion.identity);
        int index = moleculeInstance.name.IndexOf("_");
        moleculeInstance.name = moleculeInstance.name.Substring(0, index);
    }
}
