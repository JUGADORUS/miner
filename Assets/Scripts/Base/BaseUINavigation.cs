using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseUINavigation : MonoBehaviour
{
    [SerializeField] GameObject Inventory;
    [SerializeField] GameObject Forge;
    [SerializeField] GameObject EnchantmentTable;

    public void OpenInventory()
    {
        Inventory.SetActive(true);
        Forge.SetActive(false);
        EnchantmentTable.SetActive(false);
    }

    public void OpenForge()
    {
        Forge.SetActive(true);
        Inventory.SetActive(false);
        EnchantmentTable.SetActive(false);
    }

    public void OpenEnchantmentTable()
    {
        EnchantmentTable.SetActive(true);
        Inventory.SetActive(false);
        Forge.SetActive(false);
    }

    public void PlayButton()
    {
        SceneManager.LoadScene("Mine");
    }
}
