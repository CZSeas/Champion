using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
[RequireComponent(typeof(ItemController))]
public class Player : Entity {

    public Camera cam;
    PlayerController playerController;
    GunController gunController;
    ItemController itemController;
    public Animator animator;
    public Transform crosshair;
    public GameObject cape;
    float sqrDistToMuzzle;

    int currentMoney = 0;

    ShopItem currentShopItem;
    ShopItem.Type typeOfItemToPickup;

    protected override void Start() {
        base.Start();
        DontDestroyOnLoad(gameObject);
        playerController = gameObject.GetComponent<PlayerController>();
        gunController = gameObject.GetComponent<GunController>();
        GunController.ChangeGun += ChangeGun;
        ChangeGun();
        itemController = gameObject.GetComponent<ItemController>();
        cam = Camera.main;
    }

    public override void TakeHit(int damage, Vector3 hitPoint, Vector3 hitDirection, Vector3 force, float hitSpeed) {
        if (playerController.GetState() != PlayerController.State.Invincible) {
            base.TakeHit(damage, hitPoint, hitDirection, force, hitSpeed);
            StartCoroutine(playerController.KnockBack(force));
        }
    }

    public PlayerController GetPlayerController() {
        return playerController;
    }

    public void ChangeGun() {
        Vector3 muzzlePos = gunController.MuzzlePosition();
        sqrDistToMuzzle = (new Vector2(muzzlePos.x, muzzlePos.z)
            - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude;
    }

    public void IncreaseMoney(int money) {
        currentMoney += money;
    }

    public void DecreaseMoney(int money) {
        currentMoney -= money;
    }

    public int GetCurrentMoney() {
        return currentMoney;
    }

    void OnTriggerEnter(Collider c) {
        ShopItem shopItem = c.GetComponent<ShopItem>();
        if (shopItem != null) {
            UIController.instance.DisplayBottomText(shopItem.PriceString());
            typeOfItemToPickup = shopItem.type;
            currentShopItem = shopItem;
        }
    }
    
    void OnTriggerExit(Collider c) {
        ShopItem shopItem = c.GetComponent<ShopItem>();
        if (shopItem != null) {
            UIController.instance.HideBottomText();
            currentShopItem = null;
        }
    }
    
    void LateUpdate() {
        //Look Input
        if (cam != null) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight());
            float rayDist;
            if (groundPlane.Raycast(ray, out rayDist)) {
                Vector3 point = ray.GetPoint(rayDist);
                playerController.lookAt(point);
                crosshair.position = point;
                crosshair.LookAt(cam.transform.position);
            }
        }
    }

    void AnimateMovement(float xInput, float zInput) {
        //Vector3 moveDir = new Vector3(xInput, 0, zInput);
        //if (moveDir.magnitude > 1f) {
        //    moveDir = moveDir.normalized;
        //}
        //moveDir = transform.InverseTransformDirection(moveDir);
        //animator.SetFloat("xVelocity", moveDir.x);
        //animator.SetFloat("zVelocity", moveDir.z);
    }

    void Update() {

        //Movement input
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        playerController.setVelocity(moveDir.normalized);
        AnimateMovement(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        

        //Dash
        if (Input.GetKeyDown("space")) {
            StartCoroutine(playerController.Dash());
        }

        //Look Input
        if (cam != null) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight());
            float rayDist;
            if (groundPlane.Raycast(ray, out rayDist)) {
                Vector3 point = ray.GetPoint(rayDist);
                float sqrDistToPoint = (new Vector2(point.x, point.z)
                    - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude;
                if (sqrDistToPoint > sqrDistToMuzzle) {
                    gunController.Aim(point);
                }
            }
        }

        //WEAPON INPUTS

        if (Input.GetMouseButton(0)) {
            gunController.Shoot();
        }
        if (Input.GetKeyDown("q")) {
            gunController.Ult();
        }
        if (Input.GetKeyDown("r")) {
            gunController.Reload();
        }

        //ITEM INPUTS

        if (Input.GetKeyDown("e")) {
            itemController.UseItemA();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            itemController.UseItemB();
        }

        if (Input.GetKeyDown(KeyCode.Tab)) {
            itemController.SwapItems();
        }

        //SHOP INPUTS

        if (currentShopItem != null && Input.GetKeyDown("f") && currentMoney >= currentShopItem.price) {
            switch (typeOfItemToPickup) {
                case ShopItem.Type.Item:
                    itemController.EquipItem((Item) currentShopItem.item.gameObject.GetComponent<Item>());
                    break;
                case ShopItem.Type.Gun:
                    gunController.EquipGun((Gun) currentShopItem.item.gameObject.GetComponent<Gun>());
                    break;
                case ShopItem.Type.Drop:
                    float randomDir = UnityEngine.Random.Range(0f, 1f);
                    Vector3 randomDist = (new Vector3(randomDir, 2, 1 - randomDir))
                        * playerController.collisionRadius * 3;
                    Destroy(Instantiate((Drop) currentShopItem.item.gameObject.GetComponent<Drop>(), transform.position + randomDist,
                        Quaternion.identity), 30);
                    break;

            }
            currentMoney -= currentShopItem.price;
            currentShopItem.enabled = false;
            currentShopItem.gameObject.GetComponent<Collider>().enabled = false;
            currentShopItem.transform.GetChild(1).gameObject.SetActive(false);
            currentShopItem = null;
        }

    }
}