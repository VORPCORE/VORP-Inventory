RegisterNetEvent('syn:getnuistuff')
AddEventHandler('syn:getnuistuff', function(x,y,mon,gol)
	local nuistuff = x 
	SendNUIMessage({
		action = "changecheck",
		check = nuistuff,
		info = y,
	})
    SendNUIMessage({
        action      = "updateStatusHud",
        show        = not IsRadarHidden(),
        money       = mon,
        gold        = gol,
        id          = GetPlayerServerId(NetworkGetEntityOwner(PlayerPedId()))
    })
end)