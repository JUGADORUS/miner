using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemInShop : MonoBehaviour
{
    [SerializeField] public Image Background;
    [SerializeField] public Image Pickaxe;
    [SerializeField] public TextMeshProUGUI Name;
    [SerializeField] public Image ButtonImage;
    [SerializeField] public Button Button;
    [SerializeField] public TextMeshProUGUI Action;
    private void Start()
    {
        LandscapeUi.FitNewItem(Button);
        ButtonNotifier.InstallIn(transform);
    }
}
