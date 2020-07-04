exports('vorp_inventoryApi',function()
    local self = {}
    self.subWeapon = function(source,weaponid)
        TriggerEvent("vorpCore:subWeapon",source,tonumber(weaponid))
    end

    self.createWeapon = function(source,weaponName,ammoaux,compaux)
        TriggerEvent("vorpCore:registerWeapon",source,tostring(weaponName),ammoaux,compaux)
    end

    self.giveWeapon = function(source,weaponid,target)
        TriggerEvent("vorpCore:giveWeapon",source,weaponid,target)
    end

    self.addItem = function(source,itemName,cuantity)
        TriggerEvent("vorpCore:addItem",source,tostring(itemName),tonumber(cuantity))
    end

    self.subItem = function(source,itemName,cuantity)
        TriggerEvent("vorpCore:subItem",source,tostring(itemName),tonumber(cuantity))
    end

    self.getItemCount = function(source,item)
        local count = 0
        TriggerEvent("vorpCore:getItemCount",source,function(itemcount)
            count = itemcount
        end,tostring(item))
        return count
    end

    self.addBullets = function(source,weaponId,type,cuantity)
        TriggerEvent("vorpCoreClient:addBullets",source,weaponId,type,cuantity)
    end

    self.getWeaponBullets = function(source,weaponId)
        local bull
        TriggerEvent("vorpCore:getWeaponBullets",source,function(bullets)
            bull = bullets
        end,weaponId)
        return bull
    end
    
    self.getWeaponComponents = function(source,weaponId)
        local comp
        TriggerEvent("vorpCore:getWeaponComponents",source,function(components)
            comp = components
        end,weaponId) 
        return comp
    end

    self.getUserWeapons = function(source)
        local weapList
        TriggerEvent("vorpCore:getUserWeapons",source,function(weapons)
            weap = weapons
        end)
        return weapList
    end

    self.getUserWeapon = function(source,weaponId)
        local weap
        TriggerEvent("vorpCore:getUserWeapon",source,function(weapon)
            weap = weapon
        end,weaponId)
        return weap
    end
        
    self.RegisterUsableItem = function(itemName,cb)
        TriggerEvent("vorpCore:registerUsableItem",itemName,cb)
    end

    self.getUserInventory = function(source)
        local inv
        TriggerEvent("vorpCore:getUserInventory",source,function(invent)
            inv = invent
        end)
        return inv
    end

    self.CloseInv = function(source)
        TriggerClientEvent("vorp_inventory:CloseInv",source)
    end
    
    return self
end)
