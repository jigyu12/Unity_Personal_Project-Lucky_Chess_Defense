using UnityEngine;

public class ResourceTest : MonoBehaviour
{
    public GameObject heroPrefab;
    private GameObject heroAnimationPrefab;
    void Awake()
    {
        heroAnimationPrefab = Resources.Load<GameObject>("Prefabs/Spum/Hero/Hero_00000000");
        heroAnimationPrefab.transform.position = transform.position;
        
        Instantiate(heroAnimationPrefab, transform.position, Quaternion.identity, transform);
    }
}