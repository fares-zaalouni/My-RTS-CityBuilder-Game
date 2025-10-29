using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] UnitSO UnitSO;
    public float MovementSpeed { get; set; }
    public CharacterController _CharacterController { get; private set; }

    void Awake()
    {
        MovementSpeed = UnitSO.MovementSpeed;
    }

    void Start()
    {
        _CharacterController = GetComponent<CharacterController>();
    }
}
