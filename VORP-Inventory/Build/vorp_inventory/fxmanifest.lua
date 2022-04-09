game 'rdr3'

fx_version 'adamant'
rdr3_warning 'I acknowledge that this is a prerelease build of RedM, and I am aware my resources *will* become incompatible once RedM ships.'

ui_page 'html/ui.html'

client_scripts {'config.lua', 'vorpinventory_cl.net.dll', 'client/client.lua'}
server_scripts {'config.lua', 'vorpinventory_sv.net.dll', 'vorpInventoryAPI.lua', 'server/server.lua'}

server_exports {'vorp_inventoryApi'}

files {'Newtonsoft.Json.dll', 'html/**/*', 'config.json'}
