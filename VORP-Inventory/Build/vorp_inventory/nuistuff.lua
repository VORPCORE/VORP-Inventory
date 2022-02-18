RegisterNetEvent('syn:getnuistuff')
AddEventHandler('syn:getnuistuff', function(x)
	local nuistuff = x 
	SendNUIMessage({
		action = "changecheck",
		check = nuistuff,
	})
end)