Citizen.CreateThread(function()
    while true do
        Citizen.Wait(10)
            TriggerServerEvent("hud:get_money")
                Citizen.Wait(1000)
    
                local _source   = source
                local xPlayer   = PlayerPedId()
                local PlayerId  = GetPlayerServerId(NetworkGetEntityOwner(xPlayer))

     SendNUIMessage({

                action      = "updateStatusHud",
                show        = not IsRadarHidden(),
                money       = mon,
                gold        = gol,
                id          = PlayerId

        })
        
    end
end)

RegisterNetEvent("hud:send_money")
AddEventHandler("hud:send_money", function(_money, _gold)
    mon = _money
    gol = _gold
end)


RegisterNetEvent('syn:getnuistuff')
AddEventHandler('syn:getnuistuff', function(x,y)
	local nuistuff = x 
	SendNUIMessage({
		action = "changecheck",
		check = nuistuff,
		info = y,
	})
end)
