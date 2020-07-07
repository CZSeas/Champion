using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Shop : AuxRoom
{
    public ShopItem[] pedestals;
    public GameObject[] genericItems;
    public GameObject[] uniqueItems;
    int seed;

    public static Shop instance;
    public Queue<GameObject> shuffledUniqueItems;

    protected override void Start() {
        if (instance != null) {
            Destroy(gameObject);
        } else {
            instance = this;
            seed = Random.Range(0, 1000);
            shuffledUniqueItems = new Queue<GameObject>(Utility.ShuffleArray(uniqueItems, seed));
            DontDestroyOnLoad(gameObject);
        }
        instance.StartCoroutine(SpawnShop());
    }

    IEnumerator SpawnShop() {
        yield return new WaitUntil(() => SceneManager.GetActiveScene().buildIndex == 1);
        SpawnItems();
        base.Start();
    }
    
    void SpawnItems() {
        foreach (ShopItem shopItem in pedestals) {
            GameObject itemToSpawn = genericItems[Random.Range(0, genericItems.Length)];
            float spawnUnique = Random.Range(0f, 1f);
            if (spawnUnique < 0.2f && shuffledUniqueItems.Count > 0) {
                itemToSpawn = shuffledUniqueItems.Dequeue();
            }
            GameObject itemModel;
            if (itemToSpawn.GetComponent<Gun>() != null) {
                itemModel = Instantiate(itemToSpawn.GetComponent<Gun>().gunDrop,
                    shopItem.transform.position, Quaternion.identity).gameObject;
                shopItem.type = ShopItem.Type.Gun;
            } else if (itemToSpawn.GetComponent<Item>() != null) {
                itemModel = Instantiate(itemToSpawn.GetComponent<Item>().itemDrop,
                    shopItem.transform.position, Quaternion.identity).gameObject;
                
                shopItem.type = ShopItem.Type.Item;
            } else {
                itemModel = Instantiate(itemToSpawn,
                    shopItem.transform.position, Quaternion.identity);
                shopItem.type = ShopItem.Type.Drop;
            }
            itemModel.transform.parent = shopItem.transform;
            itemModel.GetComponent<Collider>().enabled = false;
            itemModel.GetComponent<Rigidbody>().useGravity = false;
            shopItem.item = itemToSpawn;
            shopItem.price = itemModel.GetComponent<Drop>().price;
        }
    }

}
