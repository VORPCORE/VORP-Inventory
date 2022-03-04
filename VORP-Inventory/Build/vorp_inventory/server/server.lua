local VorpCore = {}

TriggerEvent("getCore",function(core)
    VorpCore = core
end)

RegisterServerEvent("hud:get_money")
AddEventHandler("hud:get_money", function()
    local _source = source
    local User = VorpCore.getUser(_source).getUsedCharacter
    local money = User.money
    local gold = User.gold
    TriggerClientEvent("hud:send_money", _source, money, gold)
end)

VorpInv = exports.vorp_inventory:vorp_inventoryApi()


RegisterServerEvent("vorpinventory:check_slots")
AddEventHandler("vorpinventory:check_slots", function()
    local _source = tonumber(source)
    local eq = 0
    if _source ~= 0 and _source >= 1 and _source ~= nil then
        eq = VorpInv.getUserInventory(_source)
    else
        print("source player ".._source)
    end
    local test = eq
    local slot_check = 0
    if test ~= nil then
        for i = 1, #test do
            slot_check = slot_check + test[i].count
        end
    else
    slot_check = 0
    end
    local stufftosend = tonumber(slot_check)
    local part2 = 200 -- max carry limit
    TriggerClientEvent("syn:getnuistuff", _source, stufftosend,part2)
end)



RegisterServerEvent("vorpinventory:getLabelFromId")
AddEventHandler("vorpinventory:getLabelFromId",function(id, item2, cb)
    local _source = id
    local inventory = VorpInv.getUserInventory(_source)
    local label = "not found"
     for i,item in ipairs(inventory) do
        if item.name == item2 then 
            label = item.label
        break end
    end
    cb(label) 
end)


