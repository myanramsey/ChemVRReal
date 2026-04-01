using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpawnMolecule : MonoBehaviour
{
    public GameObject moleculeSpawnMenu;
    private Transform t;

    // Spawn slots spread along the X axis, shared across all spawn buttons.
    // Cycles: slot 0 -> slot 1 -> slot 2 -> slot 0 -> ...
    private static readonly Vector3[] spawnOffsets = new Vector3[]
    {
        new Vector3(-3.75f, 0f, 1.5f),
        new Vector3(-2.25f, 0f, 1.5f),
        new Vector3(-0.75f, 0f, 1.5f),
        new Vector3( 0.75f, 0f, 1.5f),
        new Vector3( 2.25f, 0f, 1.5f),
        new Vector3( 3.75f, 0f, 1.5f),
    };
    private static int spawnSlot = 0;

    private void Start()
    {
        t = moleculeSpawnMenu.GetComponent<Transform>();
    }

    public void Spawn()
    {
        // Spawns molecule at the next slot in the round-robin line
        string prefabName = this.gameObject.name;
        GameObject molecule = Resources.Load<GameObject>(prefabName);
        Vector3 position = t.position + spawnOffsets[spawnSlot];
        spawnSlot = (spawnSlot + 1) % spawnOffsets.Length;
        GameObject moleculeInstance = GameObject.Instantiate(molecule, position, Quaternion.identity);
        int index = moleculeInstance.name.IndexOf("_");
        moleculeInstance.name = moleculeInstance.name.Substring(0, index);

        Rigidbody rb = moleculeInstance.GetComponent<Rigidbody>();
        var grab = moleculeInstance.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (grab != null)
        {
            grab.useDynamicAttach = true; 
            grab.throwOnDetach = false;

            grab.selectExited.AddListener((args) => {
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }
            });
        }
    }
}
