using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_Text Groups;

    void Update()
    {
        Groups.text = "";
        foreach (var group in UnitSelectionManager.Instance.Groups)
        {

            Groups.text += $"Group {group.Id} Has {group.Group.Count} Unit Colored {group.Color}";
        }
    }
}
