# VORP-Inventory
Inventory System for VORP Core

## Requirements
- [VORP Core](https://github.com/VORPCORE/VORP-Core/releases)
- [VORP Inputs](https://github.com/VORPCORE/VORP-Inputs/releases)
- [VORP Character](https://github.com/VORPCORE/VORP-Character/releases)
- KLS

## How to install
* [Download the lastest version of VORP Inventory](https://github.com/VORPCORE/VORP-Inventory/releases)
* Copy and paste ```vorp_inventory``` folder to ```resources/vorp_inventory```
* Add ```ensure vorp_inventory``` to your ```server.cfg``` file
* To change the language go to ```resources/vorp_inventory/Config``` and change the default language, also you will have to edit the html file to change the text on the inventory menu
* Now you are ready!

## Features
* Unique weapons in order not to duplicate them.
* Each weapon has its own ammo and can have diferent type of ammo.
* Each weapon has its own modifications.
* When dropping or giving a weapon you give it with all the modifications and ammo.
* It also has usaable items.

## API For Lua
For importing the API on top of your server resource file
```vorpInventory = exports.vorp_inventory:vorp_inventoryApi()```
this will return a table for simply using the inventory
* Uses:
* Quit weapon
``` vorpInventory.subWeapon(source,weaponId)```
* Create and give new Weapon with the name of the weapon in capital letters
``` vorpInventory.addWeapon(source,weaponName)```
* Add an item with quantity
``` vorpInventory.addItem(source,item,cuantity)```
* Sub item with quantity
``` vorpInventory.subItem(source,item,subCuantity)```
* Returns the item quantity
``` vorpInventory.getItemCuantity(source,item)```

## Wiki
[Wiki VORP Inventory](http://docs.vorpcore.com:3000/vorp-inventory)

## Credits
[Inventory Pickup System from Redemrp_Inventory](https://github.com/RedEM-RP/redemrp_inventory/blob/master/client/main.lua)
Credits to @Trsak for create the base of this inventory interface
