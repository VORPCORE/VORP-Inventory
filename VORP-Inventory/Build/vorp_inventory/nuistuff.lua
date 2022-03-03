RegisterNetEvent('syn:getnuistuff')
AddEventHandler('syn:getnuistuff', function(x,y)
	local nuistuff = x 
	SendNUIMessage({
		action = "changecheck",
		check = nuistuff,
		info = y,
	})
end)