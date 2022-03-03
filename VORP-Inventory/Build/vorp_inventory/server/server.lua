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

